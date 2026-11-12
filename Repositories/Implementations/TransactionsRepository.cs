using BankApi.Db;
using BankApi.Models;
using BankApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankApi.Repositories.Implementations
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TransactionRepository> _logger;

        public TransactionRepository(AppDbContext context, ILogger<TransactionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Transaction>> GetByAccountId(int accountId)
        {
            _logger.LogInformation("Fetching transactions for AccountId: {AccountId}", accountId);

            return await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderBy(t => t.timestamp)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetAll()
        {
            _logger.LogInformation("Fetching ALL transactions");
            return await _context.Transactions.ToListAsync();
        }

        public async Task<Transaction?> GetByTransactionId(int transactionId)
        {
            _logger.LogInformation("Fetching transaction by TransactionId: {TransactionId}", transactionId);

            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId);
        }

        public async Task<List<Transaction>> GetByTransferId(Guid transferId)
        {
            _logger.LogInformation("Fetching transactions by TransferId: {TransferId}", transferId);

            return await _context.Transactions
                .Where(t => t.TransferId == transferId)
                .OrderBy(t => t.timestamp)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetByUserId(int userId)
        {
            _logger.LogInformation("Fetching transactions for UserId: {UserId}", userId);

            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToListAsync();

            return await _context.Transactions
                .Where(t => accounts.Contains(t.AccountId))
                .OrderBy(t => t.timestamp)
                .ToListAsync();
        }

        public async Task AddTransaction(Transaction transaction)
        {
            _logger.LogInformation(
                "Adding transaction: AccountId={AccountId}, Amount={Amount}, Type={Type}, Direction={Direction}",
                transaction.AccountId,
                transaction.Amount,
                transaction.TransactionType,
                transaction.TransactionDirection
            );

            await _context.Transactions.AddAsync(transaction);
        }

        public async Task AddTransactions(List<Transaction> transactions)
        {
            _logger.LogInformation(
                "Adding {Count} transactions in batch",
                transactions.Count
            );

            await _context.Transactions.AddRangeAsync(transactions);
        }

        public async Task<List<Transaction>> FilterTransactions(
            TransactionType? type, TransactionDirection? direction,
            DateTime? dateFrom, DateTime? dateTo)
        {
            _logger.LogInformation(
                "Filtering transactions: Type={Type}, Direction={Direction}, DateFrom={DateFrom}, DateTo={DateTo}",
                type, direction, dateFrom, dateTo
            );

            var query = _context.Transactions.AsQueryable();

            if (type.HasValue)
                query = query.Where(t => t.TransactionType == type);

            if (direction.HasValue)
                query = query.Where(t => t.TransactionDirection == direction);

            if (dateFrom.HasValue)
                query = query.Where(t => t.timestamp >= dateFrom);

            if (dateTo.HasValue)
                query = query.Where(t => t.timestamp <= dateTo);

            return await query
                .OrderByDescending(t => t.timestamp)
                .ToListAsync();
        }

        public async Task Save()
        {
            _logger.LogInformation("Saving changes to database");
            await _context.SaveChangesAsync();
        }
    }
}
