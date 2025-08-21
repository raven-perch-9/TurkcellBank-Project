using TurkcellBank.Domain;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.Abstractions
{
    public interface IGenerateIBAN
    {
        string Generate(int userID, int accountID);
        bool Validate(string IBAN);
        (int UserID, int AccountID) Parse(string IBAN);
    }
}
