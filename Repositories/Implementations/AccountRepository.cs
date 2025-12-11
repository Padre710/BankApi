//using BankApi.Db;
//using BankApi.Models;
//using BankApi.Repositories.Interfaces;
//using Microsoft.EntityFrameworkCore;

//namespace BankApi.Repositories.Implementations
//{
//    public class AccountRepository : IAccountRepository
//    {
//        private readonly AppDbContext _context;

//        public AccountRepository(AppDbContext context)
//        {
//            _context = context;
//        }
//        public async Task<IEnumerable<Account>> GetAllAsync()
//            => await _context.Accounts.ToListAsync();

//        public async Task<Account?> GetByIdAsync(int id)
//            => await _context.Accounts.FindAsync(id);

//        public async Task<IEnumerable<Account>> GetByUserIdAsync(int userId)
//            => await _context.Accounts.Where(a => a.UserId == userId).ToListAsync();

//        public async Task AddAsync(Account account)
//            => await _context.Accounts.AddAsync(account);

//        public async Task UpdateAsync(Account account)
//            =>  _context.Accounts.Update(account);

//        public async Task DeleteAsync(Account account)
//            => _context.Accounts.Remove(account);

//        public async Task<bool> AccountExistsAsync(string accountNumber)
//            => await _context.Accounts.AnyAsync(a => a.AccountNumber == accountNumber);

//        public async Task SaveChangesAsync()
//            => await _context.SaveChangesAsync();
//        public async Task<IEnumerable<Account>> SearchByAccountNumberAsync(string accountNumber)
//           =>  await _context.Accounts
//                .Include(a => a.User)
//                .Where(a => a.AccountNumber.Contains(accountNumber))
//                .ToListAsync();

//    }
//}
using BankApi.Db;
using BankApi.Models;
using BankApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankApi.Repositories.Implementations
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AccountRepository> _logger;

        public AccountRepository(AppDbContext context, ILogger<AccountRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            _logger.LogInformation("Repository: Fetching all accounts");
            return await _context.Accounts.ToListAsync();
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Repository: Fetching account ID {Id}", id);
            return await _context.Accounts.FindAsync(id);
        }

        public async Task<IEnumerable<Account>> GetByUserIdAsync(int userId)
        {
            _logger.LogInformation("Repository: Fetching accounts for user {UserId}", userId);
            return await _context.Accounts.Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task AddAsync(Account account)
        {
            _logger.LogInformation("Repository: Adding new account for user {Id}", account.UserId);
            await _context.Accounts.AddAsync(account);
        }

        public async Task UpdateAsync(Account account)
        {
            _logger.LogInformation("Repository: Updating account ID {Id}", account.Id);
            _context.Accounts.Update(account);
        }

        public async Task DeleteAsync(Account account)
        {
            _logger.LogInformation("Repository: Deleting account ID {Id}", account.Id);
            _context.Accounts.Remove(account);
        }

        public async Task<bool> AccountExistsAsync(string accountNumber)
        {
            _logger.LogInformation("Repository: Checking if account {Acc} exists", accountNumber);
            return await _context.Accounts.AnyAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task<IEnumerable<Account>> SearchByAccountNumberAsync(string accountNumber)
        {
            _logger.LogInformation("Repository: Searching accounts containing '{Num}'", accountNumber);

            return await _context.Accounts
                .Include(a => a.User)
                .Where(a => a.AccountNumber.Contains(accountNumber))
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            _logger.LogInformation("Repository: Saving changes to database");
            await _context.SaveChangesAsync();
        }
    }
}
