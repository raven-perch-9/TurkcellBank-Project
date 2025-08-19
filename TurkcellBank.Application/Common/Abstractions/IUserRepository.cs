using System.Threading.Tasks;
using TurkcellBank.Domain;

namespace TurkcellBank.Application.Common.Abstractions
{
    public interface IUserRepository
    {
        Task<TurkcellBank.Domain.User?> GetByIdAsync(int id);
        Task<TurkcellBank.Domain.User?> GetByEmailAsync(string email);
        Task<TurkcellBank.Domain.User?> GetByUsernameAsync(string username);
        Task<TurkcellBank.Domain.User?> GetByEmailOrUsernameAsync(string input);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
       
        void Update(TurkcellBank.Domain.User user);
        void Delete(TurkcellBank.Domain.User user);
        Task SaveChangesAsync();
        Task AddAsync(TurkcellBank.Domain.User user);
    }
}
