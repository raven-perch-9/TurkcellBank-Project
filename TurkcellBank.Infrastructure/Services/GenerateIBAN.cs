using TurkcellBank.Application.Common.Abstractions;

namespace TurkcellBank.Infrastructure.Services
{
    public class GenerateIBAN : IGenerateIBAN
    {
        private const string CountryCode = "TR"; // Example country code for Turkey
        private const string BankCode = "43210"; // Example bank code, should be replaced with actual bank code
        private const string ReservedDigit = "0"; //TR-specific reserved digit
        public string Generate(int userID, int accountID)
        {
            string userPart = userID.ToString().PadLeft(6, '0');
            string accountPart = accountID.ToString().PadLeft(10, '0');
            string accountNumber = userPart + accountPart;
            return $"{CountryCode}{BankCode}{ReservedDigit}{accountNumber}";
        }
         public bool Validate(string IBAN)
         {
             if (string.IsNullOrWhiteSpace(IBAN))
                   return false;

              if (!IBAN.StartsWith(CountryCode))
                    return false;

              if (IBAN.Length != (CountryCode.Length + BankCode.Length + ReservedDigit.Length + 16))
                   return false;

             return true;
         }

        public (int UserID, int AccountID) Parse(string IBAN)
        {
            if(!Validate(IBAN))
            {
                throw new ArgumentException("Invalid IBAN format.", nameof(IBAN));
            }
            
            string accountNumber = IBAN.Substring(0);
            int userID = int.Parse(accountNumber.Substring(0, 6));
            int accountID = int.Parse(accountNumber.Substring(6, 10));

            return (userID, accountID);
        }
    }
}
