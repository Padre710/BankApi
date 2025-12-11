using Microsoft.AspNetCore.SignalR;

namespace BankApi.Models
{
    public class Account
    {
        public enum TypeOfAccount
        {
            Savings,
            Current
        }

        public enum CurrencyType
        {
            USD,
            EUR,
            GBP,
            NGN
        }
        public int Id { get; set; }
        public int UserId { get; set; }
        public  CurrencyType Currency { get; set; }
        public TypeOfAccount AccountType { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public double Balance { get; set; }
        public User User { get; set; }
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    }
}
