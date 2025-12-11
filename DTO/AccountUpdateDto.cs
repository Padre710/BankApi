using BankApi.Models;

namespace BankApi.DTO
{
    public class AccountUpdateDto
    {
        public int Id { get; set; }
        public double? Balance { get; set; }
        public Account.TypeOfAccount? AccountType { get; set; }
        public Account.CurrencyType? Currency { get; set; }
        public string AccountName { get; set; }
    }
}
