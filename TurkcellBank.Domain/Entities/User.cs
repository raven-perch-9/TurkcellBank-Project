//This class is used for storing the data to the SQL DataBase

using System.ComponentModel.DataAnnotations.Schema;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Domain.Entities
{
    [Table("UserData")]
    public class User
    {
        public int ID { get; set; }
        public UserRole Role { get; set; } = UserRole.Customer;
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public ICollection<Account> Accounts { get; set; } = null!;
    }
}
