using BankApi.DTO;
using BankApi.Models;

namespace BankApi.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<List<TransactionReadDto>> GetByAccountId(int accountId, int userId, string role);
        Task<List<TransactionReadDto>> GetAll();
        Task<List<TransactionReadDto>> GetByTransactionId(int transactionId, int userId, string role);
        Task<List<TransactionReadDto>> GetByTransferId(Guid transferId);
        Task<List<TransactionReadDto>> GetByUserId(int userId, string role, int currentUserId);
        Task<TransactionResponseDto> Deposit(DepositDto dto);
        Task<TransactionResponseDto> Withdraw(WithdrawDto dto);
        Task<TransactionResponseDto> Transfer(TransferDto dto, int userId, string role);
        Task<List<TransactionReadDto>> Filter(TransactionType? type, TransactionDirection? direction,
            DateTime? from, DateTime? to);
    }
}
