using Microsoft.Identity.Client;

namespace BankApi.Models
{
    public enum TransactionType
    {
        Deposit,
        Withdraw,
        Transfer
    }
    
    public enum TransactionDirection
    {
        Debit,
        Credit
    } 
    public class Transaction
    {
        public int Id { get; set; }

        public int AccountId { get; set; } // FK
        public Account Account { get; set; }
        public TransactionType TransactionType { get; set; } 
        public TransactionDirection TransactionDirection { get; set; }
        public double Amount { get; set; }
        public DateTime timestamp { get; set; } = DateTime.Now;
        public Guid? TransferId { get; set; }
    }
}
