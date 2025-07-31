using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkcellBank.Domain
{
    public class Account
    {
        public string ID { get; set; }
        public string UserID { get; set; }
        public string IBAN { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
    }
}
