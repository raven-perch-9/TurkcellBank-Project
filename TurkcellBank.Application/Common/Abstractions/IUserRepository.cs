using System.Threading.Tasks;

namespace TurkcellBank.Application.Common.Abstractions
{
    public interface IUserRepository
    {
        Task<Domain.Entities.User?> GetByIdAsync(int id);
        Task<Domain.Entities.User?> GetByEmailAsync(string email);
        Task<Domain.Entities.User?> GetByUsernameAsync(string username);
        Task<Domain.Entities.User?> GetByEmailOrUsernameAsync(string input);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
       
        void Update(Domain.Entities.User user);
        void Delete(Domain.Entities.User user);
        Task SaveChangesAsync();
        Task AddAsync(Domain.Entities.User user);
    }
}
