using BankApi.Models;

namespace BankApi.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task<List<Transaction>> GetByAccountId(int accountId);
        Task<List<Transaction>> GetAll();
        Task<Transaction?> GetByTransactionId(int transactionId);
        Task<List<Transaction>> GetByTransferId(Guid transferId);
        Task<List<Transaction>> GetByUserId(int userId);
        Task AddTransaction(Transaction transaction);
        Task AddTransactions(List<Transaction> transactions);
        Task<List<Transaction>> FilterTransactions(TransactionType? type, TransactionDirection? direction,
            DateTime? dateFrom, DateTime? dateTo);
        Task Save();
    }
}
