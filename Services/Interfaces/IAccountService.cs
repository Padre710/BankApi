using BankApi.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApi.Services.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountReadDto>> GetAllAccountsAsync();
        Task<AccountReadDto?> GetAccountByIdAsync(int id);
        Task<double?> GetBalanceByIdAsync(int id);
        Task<AccountReadDto> CreateAccountAsync(int userId, string name, AccountCreateDto dto);
        Task<object> GetAccountsByUserAsync(int userId);
        Task UpdateAccountAsync(AccountUpdateDto dto);
        Task<object> SearchAccountAsync(string accountNumber);
        Task DeleteAccountAsync(int id);
    }
}

