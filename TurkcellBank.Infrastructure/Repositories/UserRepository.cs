using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Domain;
using TurkcellBank.Infrastructure.Data;

namespace TurkcellBank.Infrastructure.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public Task<TurkcellBank.Domain.User?> GetByIdAsync(int id)
        {
            return _db.Users
                .Include(u => u.Accounts)
                .FirstOrDefaultAsync(u => u.ID == id);
        }
        public Task<User?> GetByEmailAsync(string email)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public Task<User?> GetByUsernameAsync(string username)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
        public Task<User?> GetByEmailOrUsernameAsync(string input)
        {
            return _db.Users.FirstOrDefaultAsync(u =>
                u.Email == input || u.Username == input);
        }
        public Task<bool> EmailExistsAsync(string email)
        {
            return _db.Users.AnyAsync(u => u.Email == email);
        }
        public Task<bool> UsernameExistsAsync(string username)
        {
            return _db.Users.AnyAsync(u => u.Username == username);
        }
        public async Task AddAsync(User user)
        {
            await _db.Users.AddAsync(user);
        }
        public void Update(User user)
        {
            _db.Users.Update(user);
        }
        public void Delete(User user)
        {
            _db.Users.Remove(user);
        }
        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
