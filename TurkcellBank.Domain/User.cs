//This class is used for storing the data to the SQL DataBase

using System.ComponentModel.DataAnnotations.Schema;

namespace TurkcellBank.Domain
{
    [Table("UserData")]
    public class User
    {
        public int ID { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = "Customer"; // default
        public DateTime CreatedAt { get; set; }
        public bool IsActive {  get; set; }
    }
}
