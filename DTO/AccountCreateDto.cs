using BankApi.Models;

namespace BankApi.DTO
{
    public class AccountCreateDto

    {
        public Account.TypeOfAccount AccountType { get; set; }
        public Account.CurrencyType Currency { get; set; }


    }
}
