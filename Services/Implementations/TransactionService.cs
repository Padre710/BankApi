//using BankApi.Constants;
//using BankApi.Db;
//using BankApi.DTO;
//using BankApi.Models;
//using BankApi.Repositories.Interfaces;
//using BankApi.Repositories.Implementations;
//using BankApi.Services.Interfaces;
//using Microsoft.EntityFrameworkCore;

//namespace BankApi.Services.Implementations
//{
//    public class TransactionService : ITransactionService
//    {
//        private readonly ITransactionRepository _repo;
//        private readonly AppDbContext _context;

//        public TransactionService(ITransactionRepository repo, AppDbContext context)
//        {
//            _repo = repo;
//            _context = context;
//        }

//        public async Task<List<TransactionReadDto>> GetByAccountId(int accountId, int userId, string role)
//        {
//            if (role == Roles.Customer)
//            {
//                var owner = await _context.Accounts
//                    .Where(a => a.Id == accountId)
//                    .Select(a => a.UserId)
//                    .FirstOrDefaultAsync();

//                if (owner != userId)
//                    throw new Exception("Account not found.");
//            }

//            return (await _repo.GetByAccountId(accountId))
//                .Select(MapToDto).ToList();
//        }

//        public async Task<List<TransactionReadDto>> GetAll() =>
//            (await _repo.GetAll()).Select(MapToDto).ToList();

//        public async Task<List<TransactionReadDto>> GetByTransactionId(int id, int userId, string role)
//        {
//            if (role == Roles.Customer)
//            {
//                var check = await _context.Transactions
//                    .Where(t => t.Id == id)
//                    .Join(_context.Accounts,
//                        t => t.AccountId,
//                        a => a.Id,
//                        (t, a) => new { t.Id, a.UserId })
//                    .FirstOrDefaultAsync();

//                if (check == null || check.UserId != userId)
//                    throw new Exception("Transaction not found.");
//            }

//            var single = await _repo.GetByTransactionId(id);

//            if (single == null)
//                throw new Exception("Transaction not found.");

//            return new List<TransactionReadDto> { MapToDto(single) };
//        }

//        public async Task<List<TransactionReadDto>> GetByTransferId(Guid transferId) =>
//            (await _repo.GetByTransferId(transferId))
//            .Select(MapToDto).ToList();

//        public async Task<List<TransactionReadDto>> GetByUserId(int userId, string role, int currentUserId)
//        {
//            if (role == Roles.Customer && currentUserId != userId)
//                throw new Exception("Unauthorized.");

//            return (await _repo.GetByUserId(userId))
//                .Select(MapToDto).ToList();
//        }

//        public async Task<TransactionResponseDto> Deposit(DepositDto dto)
//        {
//            var account = await _context.Accounts.FindAsync(dto.AccountId)
//                ?? throw new Exception($"Account {dto.AccountId} not found.");

//            // removed amount <= 0 validation

//            account.Balance += dto.Amount;

//            var txn = new Transaction
//            {
//                Amount = dto.Amount,
//                AccountId = dto.AccountId,
//                TransactionType = TransactionType.Deposit,
//                TransactionDirection = TransactionDirection.Credit
//            };

//            await _repo.AddTransaction(txn);
//            await _repo.Save();

//            return new TransactionResponseDto
//            {
//                message = "Deposit Successful",
//                Txn = "Credit",
//                Amount = dto.Amount.ToString("F2"),
//                Date = txn.timestamp.ToString("dd MMM yyyy HH:mm"),
//                Balance = account.Balance.ToString("F2")
//            };
//        }

//        public async Task<TransactionResponseDto> Withdraw(WithdrawDto dto)
//        {
//            var account = await _context.Accounts.FindAsync(dto.AccountId)
//                ?? throw new Exception($"Account {dto.AccountId} not found.");

//            // removed dto.Amount <= 0 check

//            if (dto.Amount > account.Balance)
//                throw new Exception("Insufficient Balance");

//            account.Balance -= dto.Amount;

//            var txn = new Transaction
//            {
//                Amount = dto.Amount,
//                AccountId = dto.AccountId,
//                TransactionType = TransactionType.Withdraw,
//                TransactionDirection = TransactionDirection.Debit
//            };

//            await _repo.AddTransaction(txn);
//            await _repo.Save();

//            return new TransactionResponseDto
//            {
//                message = "Withdrawal Successful",
//                Txn = "Debit",
//                Amount = dto.Amount.ToString("F2"),
//                Date = txn.timestamp.ToString("dd MMM yyyy HH:mm"),
//                Balance = account.Balance.ToString("F2")
//            };
//        }

//        public async Task<TransactionResponseDto> Transfer(TransferDto dto, int userId, string role)
//        {
//            if (role == Roles.Customer)
//            {
//                var owner = await _context.Accounts
//                    .Where(a => a.Id == dto.FromAccountId)
//                    .Select(a => a.UserId)
//                    .FirstOrDefaultAsync();

//                if (owner != userId)
//                    throw new Exception("Unauthorized transfer.");
//            }

//            var from = await _context.Accounts.FindAsync(dto.FromAccountId);
//            var to = await _context.Accounts.FindAsync(dto.ToAccountId);

//            if (from == null || to == null)
//                throw new Exception("Account(s) not found.");

//            if (from.Currency != to.Currency)
//                throw new Exception("Currencies must match.");

//            // removed dto.Amount <= 0 validation

//            if (dto.Amount > from.Balance)
//                throw new Exception("Insufficient balance");

//            var transferId = Guid.NewGuid();

//            from.Balance -= dto.Amount;
//            to.Balance += dto.Amount;

//            var debit = new Transaction
//            {
//                Amount = dto.Amount,
//                AccountId = dto.FromAccountId,
//                TransactionType = TransactionType.Transfer,
//                TransactionDirection = TransactionDirection.Debit,
//                TransferId = transferId
//            };

//            var credit = new Transaction
//            {
//                Amount = dto.Amount,
//                AccountId = dto.ToAccountId,
//                TransactionType = TransactionType.Transfer,
//                TransactionDirection = TransactionDirection.Credit,
//                TransferId = transferId
//            };

//            await _repo.AddTransactions(new List<Transaction> { debit, credit });
//            await _repo.Save();

//            return new TransactionResponseDto
//            {
//                message = $"Transfer to Account {dto.ToAccountId} successful",
//                Txn = "Debit",
//                Amount = dto.Amount.ToString("F2"),
//                Date = debit.timestamp.ToString("dd MMM yyyy HH:mm"),
//                Balance = from.Balance.ToString("F2")
//            };
//        }

//        public async Task<List<TransactionReadDto>> Filter(TransactionType? type, TransactionDirection? direction,
//            DateTime? from, DateTime? to)
//        {
//            return (await _repo.FilterTransactions(type, direction, from, to))
//                .Select(MapToDto).ToList();
//        }

//        private static TransactionReadDto MapToDto(Transaction t) =>
//            new TransactionReadDto
//            {
//                Id = t.Id,
//                Amount = t.Amount,
//                AccountId = t.AccountId,
//                TransactionType = t.TransactionType,
//                TransactionDirection = t.TransactionDirection,
//                TransferId = t.TransferId,
//                Timestamp = t.timestamp
//            };
//    }

//}

using BankApi.Constants;
using BankApi.Db;
using BankApi.DTO;
using BankApi.Models;
using BankApi.Repositories.Interfaces;
using BankApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BankApi.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repo;
        private readonly AppDbContext _context;

        public TransactionService(ITransactionRepository repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task<List<TransactionReadDto>> GetByAccountId(int accountId, int userId, string role)
        {
            try
            {
                Log.Information("GetByAccountId called for Account {AccountId} by User {UserId} with Role {Role}", accountId, userId, role);

                if (role == Roles.Customer)
                {
                    var owner = await _context.Accounts
                        .Where(a => a.Id == accountId)
                        .Select(a => a.UserId)
                        .FirstOrDefaultAsync();

                    if (owner != userId)
                    {
                        Log.Warning("Unauthorized access attempt to Account {AccountId} by User {UserId}", accountId, userId);
                        throw new Exception("Account not found.");
                    }
                }

                var transactions = (await _repo.GetByAccountId(accountId)).Select(MapToDto).ToList();

                Log.Information("{Count} transactions returned for Account {AccountId}", transactions.Count, accountId);
                return transactions;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching transactions for Account {AccountId}", accountId);
                throw;
            }
        }

        public async Task<List<TransactionReadDto>> GetAll()
        {
            try
            {
                Log.Information("Fetching all transactions");
                var transactions = (await _repo.GetAll()).Select(MapToDto).ToList();
                Log.Information("{Count} transactions returned", transactions.Count);
                return transactions;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching all transactions");
                throw;
            }
        }

        public async Task<List<TransactionReadDto>> GetByTransactionId(int id, int userId, string role)
        {
            try
            {
                Log.Information("GetByTransactionId called for Transaction {TransactionId} by User {UserId} with Role {Role}", id, userId, role);

                if (role == Roles.Customer)
                {
                    var check = await _context.Transactions
                        .Where(t => t.Id == id)
                        .Join(_context.Accounts,
                            t => t.AccountId,
                            a => a.Id,
                            (t, a) => new { t.Id, a.UserId })
                        .FirstOrDefaultAsync();

                    if (check == null || check.UserId != userId)
                    {
                        Log.Warning("Unauthorized access attempt for Transaction {TransactionId} by User {UserId}", id, userId);
                        throw new Exception("Transaction not found.");
                    }
                }

                var single = await _repo.GetByTransactionId(id);
                if (single == null)
                {
                    Log.Warning("Transaction {TransactionId} not found", id);
                    throw new Exception("Transaction not found.");
                }

                Log.Information("Transaction {TransactionId} returned successfully", id);
                return new List<TransactionReadDto> { MapToDto(single) };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching Transaction {TransactionId}", id);
                throw;
            }
        }

        public async Task<List<TransactionReadDto>> GetByTransferId(Guid transferId)
        {
            try
            {
                Log.Information("GetByTransferId called for Transfer {TransferId}", transferId);
                var transactions = (await _repo.GetByTransferId(transferId)).Select(MapToDto).ToList();
                Log.Information("{Count} transactions returned for Transfer {TransferId}", transactions.Count, transferId);
                return transactions;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching Transfer {TransferId}", transferId);
                throw;
            }
        }

        public async Task<List<TransactionReadDto>> GetByUserId(int userId, string role, int currentUserId)
        {
            try
            {
                Log.Information("GetByUserId called for User {UserId} by User {CurrentUserId} with Role {Role}", userId, currentUserId, role);

                if (role == Roles.Customer && currentUserId != userId)
                {
                    Log.Warning("Unauthorized access attempt by User {CurrentUserId} to User {UserId}'s transactions", currentUserId, userId);
                    throw new Exception("Unauthorized.");
                }

                var transactions = (await _repo.GetByUserId(userId)).Select(MapToDto).ToList();
                Log.Information("{Count} transactions returned for User {UserId}", transactions.Count, userId);
                return transactions;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching transactions for User {UserId}", userId);
                throw;
            }
        }

        public async Task<TransactionResponseDto> Deposit(DepositDto dto)
        {
            try
            {
                Log.Information("Deposit called for Account {AccountId}, Amount {Amount}", dto.AccountId, dto.Amount);

                var account = await _context.Accounts.FindAsync(dto.AccountId)
                    ?? throw new Exception($"Account {dto.AccountId} not found.");

                account.Balance += dto.Amount;

                var txn = new Transaction
                {
                    Amount = dto.Amount,
                    AccountId = dto.AccountId,
                    TransactionType = TransactionType.Deposit,
                    TransactionDirection = TransactionDirection.Credit
                };

                await _repo.AddTransaction(txn);
                await _repo.Save();

                Log.Information("Deposit successful for Account {AccountId}, Amount {Amount}", dto.AccountId, dto.Amount);

                return new TransactionResponseDto
                {
                    message = "Deposit Successful",
                    Txn = "Credit",
                    Amount = dto.Amount.ToString("F2"),
                    Date = txn.timestamp.ToString("dd MMM yyyy HH:mm"),
                    Balance = account.Balance.ToString("F2")
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deposit failed for Account {AccountId}, Amount {Amount}", dto.AccountId, dto.Amount);
                throw;
            }
        }

        public async Task<TransactionResponseDto> Withdraw(WithdrawDto dto)
        {
            try
            {
                Log.Information("Withdraw called for Account {AccountId}, Amount {Amount}", dto.AccountId, dto.Amount);

                var account = await _context.Accounts.FindAsync(dto.AccountId)
                    ?? throw new Exception($"Account {dto.AccountId} not found.");

                if (dto.Amount > account.Balance)
                {
                    Log.Warning("Insufficient balance for Withdraw: Account {AccountId}, Amount {Amount}, Balance {Balance}", dto.AccountId, dto.Amount, account.Balance);
                    throw new Exception("Insufficient Balance");
                }

                account.Balance -= dto.Amount;

                var txn = new Transaction
                {
                    Amount = dto.Amount,
                    AccountId = dto.AccountId,
                    TransactionType = TransactionType.Withdraw,
                    TransactionDirection = TransactionDirection.Debit
                };

                await _repo.AddTransaction(txn);
                await _repo.Save();

                Log.Information("Withdrawal successful for Account {AccountId}, Amount {Amount}", dto.AccountId, dto.Amount);

                return new TransactionResponseDto
                {
                    message = "Withdrawal Successful",
                    Txn = "Debit",
                    Amount = dto.Amount.ToString("F2"),
                    Date = txn.timestamp.ToString("dd MMM yyyy HH:mm"),
                    Balance = account.Balance.ToString("F2")
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Withdrawal failed for Account {AccountId}, Amount {Amount}", dto.AccountId, dto.Amount);
                throw;
            }
        }

        public async Task<TransactionResponseDto> Transfer(TransferDto dto, int userId, string role)
        {
            try
            {
                Log.Information("Transfer called: From {FromAccountId} To {ToAccountId}, Amount {Amount}, User {UserId}, Role {Role}", dto.FromAccountId, dto.ToAccountId, dto.Amount, userId, role);

                if (role == Roles.Customer)
                {
                    var owner = await _context.Accounts
                        .Where(a => a.Id == dto.FromAccountId)
                        .Select(a => a.UserId)
                        .FirstOrDefaultAsync();

                    if (owner != userId)
                    {
                        Log.Warning("Unauthorized transfer attempt by User {UserId}", userId);
                        throw new Exception("Unauthorized transfer.");
                    }
                }

                var from = await _context.Accounts.FindAsync(dto.FromAccountId);
                var to = await _context.Accounts.FindAsync(dto.ToAccountId);

                if (from == null || to == null)
                {
                    Log.Warning("Transfer failed. Account(s) not found. From {FromAccountId}, To {ToAccountId}", dto.FromAccountId, dto.ToAccountId);
                    throw new Exception("Account(s) not found.");
                }

                if (from.Currency != to.Currency)
                    throw new Exception("Currencies must match.");

                if (dto.Amount > from.Balance)
                {
                    Log.Warning("Insufficient balance for Transfer. From {FromAccountId}, Amount {Amount}, Balance {Balance}", dto.FromAccountId, dto.Amount, from.Balance);
                    throw new Exception("Insufficient balance");
                }

                var transferId = Guid.NewGuid();

                from.Balance -= dto.Amount;
                to.Balance += dto.Amount;

                var debit = new Transaction
                {
                    Amount = dto.Amount,
                    AccountId = dto.FromAccountId,
                    TransactionType = TransactionType.Transfer,
                    TransactionDirection = TransactionDirection.Debit,
                    TransferId = transferId
                };

                var credit = new Transaction
                {
                    Amount = dto.Amount,
                    AccountId = dto.ToAccountId,
                    TransactionType = TransactionType.Transfer,
                    TransactionDirection = TransactionDirection.Credit,
                    TransferId = transferId
                };

                await _repo.AddTransactions(new List<Transaction> { debit, credit });
                await _repo.Save();

                Log.Information("Transfer successful: {Amount} from Account {FromAccountId} to {ToAccountId}", dto.Amount, dto.FromAccountId, dto.ToAccountId);

                return new TransactionResponseDto
                {
                    message = $"Transfer to Account {dto.ToAccountId} successful",
                    Txn = "Debit",
                    Amount = dto.Amount.ToString("F2"),
                    Date = debit.timestamp.ToString("dd MMM yyyy HH:mm"),
                    Balance = from.Balance.ToString("F2")
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Transfer failed: From {FromAccountId} To {ToAccountId}, Amount {Amount}", dto.FromAccountId, dto.ToAccountId, dto.Amount);
                throw;
            }
        }

        public async Task<List<TransactionReadDto>> Filter(TransactionType? type, TransactionDirection? direction,
            DateTime? from, DateTime? to)
        {
            try
            {
                Log.Information("FilterTransactions called: Type {Type}, Direction {Direction}, From {From}, To {To}", type, direction, from, to);

                var transactions = (await _repo.FilterTransactions(type, direction, from, to)).Select(MapToDto).ToList();

                Log.Information("{Count} transactions returned from filter", transactions.Count);
                return transactions;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "FilterTransactions failed");
                throw;
            }
        }

        private static TransactionReadDto MapToDto(Transaction t) =>
            new TransactionReadDto
            {
                Id = t.Id,
                Amount = t.Amount,
                AccountId = t.AccountId,
                TransactionType = t.TransactionType,
                TransactionDirection = t.TransactionDirection,
                TransferId = t.TransferId,
                Timestamp = t.timestamp
            };
    }
}
