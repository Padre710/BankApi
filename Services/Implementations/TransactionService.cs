using BankApi.Constants;
using BankApi.DTO;
using BankApi.Exceptions;
using BankApi.Helpers;
using BankApi.Models;
using BankApi.Repositories.Interfaces;
using BankApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankApi.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository transactionRepo,
            IAccountRepository accountRepo,
            ILogger<TransactionService> logger)
        {
            _transactionRepo = transactionRepo;
            _accountRepo = accountRepo;
            _logger = logger;
        }

        public async Task<List<TransactionReadDto>> GetByAccountId(int accountId, int userId, string role)
        {
            try
            {
                _logger.LogInformation("GetByAccountId called for Account {AccountId} by User {UserId} with Role {Role}", accountId, userId, role);

                if (role == Roles.Customer)
                {
                    var account = await _accountRepo.GetByIdAsync(accountId);
                    if (account == null || account.UserId != userId)
                    {
                        _logger.LogWarning("Unauthorized access attempt to Account {AccountId} by User {UserId}", accountId, userId);
                        throw new NotFoundException("Account not found.");
                    }
                }

                var transactions = (await _transactionRepo.GetByAccountId(accountId)).Select(MapToDto).ToList();

                _logger.LogInformation("{Count} transactions returned for Account {AccountId}", transactions.Count, accountId);
                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions for Account {AccountId}", accountId);
                throw;
            }
        }

        public async Task<List<TransactionReadDto>> GetAll()
        {
            try
            {
                _logger.LogInformation("Fetching all transactions");
                var transactions = (await _transactionRepo.GetAll()).Select(MapToDto).ToList();
                _logger.LogInformation("{Count} transactions returned", transactions.Count);
                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all transactions");
                throw;
            }
        }

        public async Task<List<TransactionReadDto>> GetByTransactionId(int id, int userId, string role)
        {
            try
            {
                _logger.LogInformation("GetByTransactionId called for Transaction {TransactionId} by User {UserId} with Role {Role}", id, userId, role);

                var single = await _transactionRepo.GetByTransactionId(id);
                if (single == null)
                {
                    _logger.LogWarning("Transaction {TransactionId} not found", id);
                    throw new NotFoundException("Transaction not found.");
                }

                if (role == Roles.Customer)
                {
                    var account = await _accountRepo.GetByIdAsync(single.AccountId);
                    if (account == null || account.UserId != userId)
                    {
                        _logger.LogWarning("Unauthorized access attempt for Transaction {TransactionId} by User {UserId}", id, userId);
                        throw new NotFoundException("Transaction not found.");
                    }
                }

                _logger.LogInformation("Transaction {TransactionId} returned successfully", id);
                return new List<TransactionReadDto> { MapToDto(single) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Transaction {TransactionId}", id);
                throw;
            }
        }

        public async Task<List<TransactionReadDto>> GetByTransferId(Guid transferId)
        {
            try
            {
                _logger.LogInformation("GetByTransferId called for Transfer {TransferId}", transferId);
                var transactions = (await _transactionRepo.GetByTransferId(transferId)).Select(MapToDto).ToList();
                _logger.LogInformation("{Count} transactions returned for Transfer {TransferId}", transactions.Count, transferId);
                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Transfer {TransferId}", transferId);
                throw;
            }
        }

        public async Task<List<TransactionReadDto>> GetByUserId(int userId, string role, int currentUserId)
        {
            try
            {
                _logger.LogInformation("GetByUserId called for User {UserId} by User {CurrentUserId} with Role {Role}", userId, currentUserId, role);

                if (role == Roles.Customer && currentUserId != userId)
                {
                    _logger.LogWarning("Unauthorized access attempt by User {CurrentUserId} to User {UserId}'s transactions", currentUserId, userId);
                    throw new ForbiddenException("Unauthorized.");
                }

                var transactions = (await _transactionRepo.GetByUserId(userId)).Select(MapToDto).ToList();
                _logger.LogInformation("{Count} transactions returned for User {UserId}", transactions.Count, userId);
                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions for User {UserId}", userId);
                throw;
            }
        }

        public async Task<TransactionResponseDto> Deposit(DepositDto dto)
        {
            try
            {
                _logger.LogInformation("Deposit called for Account {AccountId}, Amount {Amount}", dto.AccountId, dto.Amount);

                var account = await _accountRepo.GetByIdAsync(dto.AccountId)
                    ?? throw new NotFoundException($"Account {dto.AccountId} not found.");

                account.Balance += dto.Amount;

                var txn = new Transaction
                {
                    Amount = dto.Amount,
                    AccountId = dto.AccountId,
                    TransactionType = TransactionType.Deposit,
                    TransactionDirection = TransactionDirection.Credit
                };

                await _transactionRepo.ExecuteInTransactionAsync(async () =>
                {
                    await _accountRepo.UpdateAsync(account);
                    await _transactionRepo.AddTransaction(txn);
                    await _transactionRepo.Save();
                });

                _logger.LogInformation("Deposit successful for Account {AccountId}, Amount {Amount}", dto.AccountId, dto.Amount);

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
                _logger.LogError(ex, "Deposit failed for Account {AccountId}, Amount {Amount}", dto.AccountId, dto.Amount);
                throw;
            }
        }

        public async Task<TransactionResponseDto> Withdraw(WithdrawDto dto)
        {
            try
            {
                _logger.LogInformation("Withdraw called for Account {AccountId}, Amount {Amount}", dto.AccountId, dto.Amount);

                var account = await _accountRepo.GetByIdAsync(dto.AccountId)
                    ?? throw new NotFoundException($"Account {dto.AccountId} not found.");

                if (dto.Amount > account.Balance)
                {
                    _logger.LogWarning("Insufficient balance for Withdraw: Account {AccountId}, Amount {Amount}, Balance {Balance}", dto.AccountId, dto.Amount, account.Balance);
                    throw new InsufficientBalanceException("Insufficient Balance");
                }

                account.Balance -= dto.Amount;

                var txn = new Transaction
                {
                    Amount = dto.Amount,
                    AccountId = dto.AccountId,
                    TransactionType = TransactionType.Withdraw,
                    TransactionDirection = TransactionDirection.Debit
                };

                await _transactionRepo.ExecuteInTransactionAsync(async () =>
                {
                    await _accountRepo.UpdateAsync(account);
                    await _transactionRepo.AddTransaction(txn);
                    await _transactionRepo.Save();
                });

                _logger.LogInformation("Withdrawal successful for Account {AccountId}, Amount {Amount}", dto.AccountId, dto.Amount);

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
                _logger.LogError(ex, "Withdrawal failed for Account {AccountId}, Amount {Amount}", dto.AccountId, dto.Amount);
                throw;
            }
        }

        public async Task<TransactionResponseDto> Transfer(TransferDto dto, int userId, string role)
        {
            try
            {
                _logger.LogInformation("Transfer called: From {FromAccountId} To {ToAccountId}, Amount {Amount}, User {UserId}, Role {Role}", dto.FromAccountId, dto.ToAccountId, dto.Amount, userId, role);

                if (dto.FromAccountId == dto.ToAccountId)
                {
                    _logger.LogWarning("Transfer rejected: same account {AccountId} on both sides", dto.FromAccountId);
                    throw new BadRequestException("Cannot transfer to the same account.");
                }

                var from = await _accountRepo.GetByIdAsync(dto.FromAccountId);
                var to = await _accountRepo.GetByIdAsync(dto.ToAccountId);

                if (from == null || to == null)
                {
                    _logger.LogWarning("Transfer failed. Account(s) not found. From {FromAccountId}, To {ToAccountId}", dto.FromAccountId, dto.ToAccountId);
                    throw new NotFoundException("Account(s) not found.");
                }

                if (role == Roles.Customer && from.UserId != userId)
                {
                    _logger.LogWarning("Unauthorized transfer attempt by User {UserId}", userId);
                    throw new ForbiddenException("Unauthorized transfer.");
                }

                if (from.Currency != to.Currency)
                    throw new BadRequestException("Currencies must match.");

                if (dto.Amount > from.Balance)
                {
                    _logger.LogWarning("Insufficient balance for Transfer. From {FromAccountId}, Amount {Amount}, Balance {Balance}", dto.FromAccountId, dto.Amount, from.Balance);
                    throw new InsufficientBalanceException("Insufficient balance.");
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

                await _transactionRepo.ExecuteInTransactionAsync(async () =>
                {
                    await _accountRepo.UpdateAsync(from);
                    await _accountRepo.UpdateAsync(to);
                    await _transactionRepo.AddTransactions(new List<Transaction> { debit, credit });
                    await _transactionRepo.Save();
                });

                _logger.LogInformation("Transfer successful: {Amount} from Account {FromAccountId} to {ToAccountId}", dto.Amount, dto.FromAccountId, dto.ToAccountId);

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
                _logger.LogError(ex, "Transfer failed: From {FromAccountId} To {ToAccountId}, Amount {Amount}", dto.FromAccountId, dto.ToAccountId, dto.Amount);
                throw;
            }
        }

        public async Task<List<TransactionReadDto>> Filter(TransactionType? type, TransactionDirection? direction,
            DateTime? from, DateTime? to)
        {
            try
            {
                _logger.LogInformation("FilterTransactions called: Type {Type}, Direction {Direction}, From {From}, To {To}", type, direction, from, to);

                var transactions = (await _transactionRepo.FilterTransactions(type, direction, from, to)).Select(MapToDto).ToList();

                _logger.LogInformation("{Count} transactions returned from filter", transactions.Count);
                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FilterTransactions failed");
                throw;
            }
        }

        public async Task<StatementDto> GetMonthlyStatement(int accountId, int year, int month, int userId, string role)
        {
            try
            {
                _logger.LogInformation("GetMonthlyStatement called for Account {AccountId}, {Year}-{Month}, User {UserId}, Role {Role}", accountId, year, month, userId, role);

                if (year < 2000 || year > DateTime.Now.Year)
                    throw new ArgumentException("Year is out of range.");

                if (month < 1 || month > 12)
                    throw new ArgumentException("Month must be between 1 and 12.");

                var periodStart = new DateTime(year, month, 1);
                var periodEndExclusive = periodStart.AddMonths(1);

                if (periodStart > DateTime.Now)
                    throw new ArgumentException("Cannot generate a statement for a future month.");

                var account = await _accountRepo.GetByIdAsync(accountId);

                if (account == null || (role == Roles.Customer && account.UserId != userId))
                {
                    _logger.LogWarning("Statement denied or account missing: Account {AccountId}, User {UserId}", accountId, userId);
                    throw new NotFoundException("Account not found.");
                }

                var all = (await _transactionRepo.GetByAccountId(accountId))
                    .OrderBy(t => t.timestamp)
                    .ThenBy(t => t.Id)
                    .ToList();

                var afterPeriod = all
                    .Where(t => t.timestamp >= periodEndExclusive)
                    .Sum(t => Signed(t));

                var inPeriod = all
                    .Where(t => t.timestamp >= periodStart && t.timestamp < periodEndExclusive)
                    .ToList();

                var transferIds = inPeriod
                    .Where(t => t.TransferId.HasValue)
                    .Select(t => t.TransferId!.Value)
                    .Distinct()
                    .ToList();

                var otherLegs = await _transactionRepo.GetTransferCounterpartsAsync(transferIds, accountId);
                var closing = account.Balance - afterPeriod;
                var opening = closing - inPeriod.Sum(t => Signed(t));

                var running = opening;
                var lines = new List<StatementLineDto>();
                foreach (var t in inPeriod)
                {
                    running += Signed(t);
                    lines.Add(new StatementLineDto
                    {
                        Date = t.timestamp,
                        Description = Describe(t, otherLegs),
                        Type = t.TransactionType.ToString(),
                        Direction = t.TransactionDirection.ToString(),
                        Amount = Math.Round(t.Amount, 2),
                        BalanceAfter = Math.Round(running, 2),
                        TransferId = t.TransferId
                    });
                }

                var statement = new StatementDto
                {
                    AccountId = account.Id,
                    AccountNumber = account.AccountNumber,
                    AccountName = account.AccountName,
                    Currency = account.Currency.ToString(),
                    Year = year,
                    Month = month,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEndExclusive.AddDays(-1),
                    OpeningBalance = Math.Round(opening, 2),
                    TotalCredits = Math.Round(inPeriod.Where(t => t.TransactionDirection == TransactionDirection.Credit).Sum(t => t.Amount), 2),
                    TotalDebits = Math.Round(inPeriod.Where(t => t.TransactionDirection == TransactionDirection.Debit).Sum(t => t.Amount), 2),
                    ClosingBalance = Math.Round(closing, 2),
                    TransactionCount = lines.Count,
                    Lines = lines
                };

                _logger.LogInformation("Statement built for Account {AccountId}, {Year}-{Month}: {Count} transactions", accountId, year, month, lines.Count);
                return statement;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building statement for Account {AccountId}, {Year}-{Month}", accountId, year, month);
                throw;
            }
        }

        public async Task<byte[]> GetMonthlyStatementPdf(int accountId, int year, int month, int userId, string role)
        {
            var statement = await GetMonthlyStatement(accountId, year, month, userId, role);
            return StatementPdfGenerator.Generate(statement);
        }

        private static decimal Signed(Transaction t) =>
            t.TransactionDirection == TransactionDirection.Credit ? t.Amount : -t.Amount;

        private static string Describe(Transaction t, List<Transaction> otherLegs)
        {
            if (t.TransactionType == TransactionType.Deposit) return "Deposit";
            if (t.TransactionType == TransactionType.Withdraw) return "Withdrawal";

            var other = otherLegs.FirstOrDefault(o => o.TransferId == t.TransferId)?.Account;
            var who = other == null
                ? "unknown account"
                : $"{other.AccountName} (****{LastFour(other.AccountNumber)})";

            return t.TransactionDirection == TransactionDirection.Debit
                ? $"Transfer to {who}"
                : $"Transfer from {who}";
        }

        private static string LastFour(string number) =>
            number.Length <= 4 ? number : number.Substring(number.Length - 4);

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
