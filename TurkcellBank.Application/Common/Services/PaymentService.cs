using TurkcellBank.Application.Common.Services.Interfaces;
using TurkcellBank.Application.Common.DTOs;
using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Domain.Enums;
using TurkcellBank.Domain.Entities;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TurkcellBank.Application.Common.Services
{
    public sealed class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _payments;
        public PaymentService(IPaymentRepository payments) => _payments = payments;

        private static readonly TimeSpan ThreeDSWindow = TimeSpan.FromMinutes(5);

        public async Task<PaymentResponseDTO> InitiateAsync(PaymentRequestDTO req)
        {
            var p = new Payment
            {
                OrderId = req.OrderId,
                Amount = req.Amount,
                Currency = req.Currency,
                Status = PaymentStatus.Initiated,
                CreatedAt = DateTime.UtcNow
            };

            string? code = null;

            if (needThreeDS(req))
            {
                p.Status = PaymentStatus.Requires3DS;
                code = GenerateCode();
                p.ThreeDSCodeHash = Hash(code);
                p.ThreeDSExpiresAt = DateTime.UtcNow.Add(ThreeDSWindow);
            }

            await _payments.AddAsync(p);
            await _payments.SaveChangesAsync();

            var dto = ToResponse(p);

            dto.ThreeDSCode = code;

            return dto;
        }

        public async Task<PaymentResponseDTO> VerifyThreeDSAsync(ThreeDSVerifyDTO req)
        {
            var p = await _payments.GetByIdAsync(req.PaymentId);
            if (p == null || p.Status != PaymentStatus.Requires3DS ||
                p.ThreeDSExpiresAt < DateTime.UtcNow ||
                p.ThreeDSCodeHash != Hash(req.Code))
            {
                throw new KeyNotFoundException("Payment not found");
            }
            p.Status = PaymentStatus.Authorized;
            p.AuthorizedAt = DateTime.UtcNow;

            p.ThreeDSCodeHash = null;
            p.ThreeDSExpiresAt = null;

            await _payments.SaveChangesAsync();
            return ToResponse(p);
        }

        public async Task<PaymentResponseDTO> CaptureAsync(int paymentId)
        {
            var p = await _payments.GetByIdAsync(paymentId);
            if (p == null || p.Status != PaymentStatus.Authorized)
            {
                throw new KeyNotFoundException("Payment not found");
            }
            p.Status = PaymentStatus.Captured;
            p.CapturedAt = DateTime.UtcNow;
            await _payments.SaveChangesAsync();
            return ToResponse(p);
        }

        public async Task<IReadOnlyList<PaymentResponseDTO>> ListByUserAsync(int userId)
        {
            var items = await _payments.ListByUserAsync(userId);
            return items.Select(ToResponse).ToList();
        }

        public async Task<PaymentResponseDTO?> GetAsync(int paymentId)
            => (await _payments.GetByIdAsync(paymentId)) is Payment p ? ToResponse(p) : null;

        private static bool needThreeDS(PaymentRequestDTO r) => r.Amount > 0;

        private static string GenerateCode()
        {
            using var rng = RandomNumberGenerator.Create();
            Span<byte> bytes = stackalloc byte[4];
            rng.GetBytes(bytes);
            return (BitConverter.ToUInt32(bytes) % 1_000_000).ToString();
        }

        private static string Hash(string s)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s)));
        }

        private static PaymentResponseDTO ToResponse(Payment p) => new()
        {
            PaymentId = p.Id,
            Status = p.Status,
            CardMask = p.CardMask,
            CreatedAt = p.CreatedAt,
            AuthorizedAt = p.AuthorizedAt,
            CapturedAt = p.CapturedAt
        };
    }
}
