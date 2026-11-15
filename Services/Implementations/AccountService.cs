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
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepo;
        private readonly IUserRepository _userRepo;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IAccountRepository accountRepo,
            IUserRepository userRepo,
            ILogger<AccountService> logger)
        {
            _accountRepo = accountRepo;
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<AccountReadDto>> GetAllAccountsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all accounts...");

                var accounts = await _accountRepo.GetAllAsync();

                _logger.LogInformation("Fetched {Count} accounts.", accounts.Count());

                return accounts.Select(a => new AccountReadDto
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    Balance = a.Balance,
                    AccountType = a.AccountType.ToString(),
                    Currency = a.Currency.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all accounts");
                throw;
            }
        }

        public async Task<AccountReadDto?> GetAccountByIdAsync(int id, int currentUserId, string role)
        {
            try 
            {
                _logger.LogInformation("Fetching account by ID {Id}", id);

                var account = await _accountRepo.GetByIdAsync(id);

                if (account == null)
                {
                    _logger.LogWarning("Account with ID {Id} not found.", id);
                    return null;
                }

                // If customer -> hide existence if not the owner
                if (role == Roles.Customer && account.UserId != currentUserId)
                {
                    _logger.LogWarning("Customer {UserId} denied access to Account {AccountId}", currentUserId, id);
                    return null;
                }

                _logger.LogInformation("Account with ID {Id} retrieved.", id);

                return new AccountReadDto
                {
                    Id = account.Id,
                    AccountNumber = account.AccountNumber,
                    AccountName = account.AccountName,
                    Balance = account.Balance,
                    AccountType = account.AccountType.ToString(),
                    Currency = account.Currency.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching account by id {Id}", id);
                throw;
            }
        }

        public async Task<decimal?> GetBalanceByIdAsync(int id, int currentUserId, string role)
        {
            try
            {
                _logger.LogInformation("Fetching balance for account ID {Id}", id);
                var account = await _accountRepo.GetByIdAsync(id);

                if (account == null)
                {
                    _logger.LogWarning("Account ID {Id} not found when checking balance", id);
                    return null;
                }

                if (role == Roles.Customer && account.UserId != currentUserId)
                {
                    _logger.LogWarning("Customer {UserId} denied balance check for Account {AccountId}", currentUserId, id);
                    return null;
                }

                _logger.LogInformation("Balance fetched for account ID {Id}", id);
                return account.Balance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching balance for account id {Id}", id);
                throw;
            }
        }

        public async Task<AccountReadDto> CreateAccountAsync(int userId, string name, AccountCreateDto dto)
        {
            try
            {
                _logger.LogInformation("Creating account for user ID {UserId}", userId);

                var uniqueAccountNumber = await AccountNumberHelper.GenerateUniqueAccountNumberAsync(_accountRepo);

                var account = new Account
                {
                    UserId = userId,
                    AccountName = name,
                    AccountNumber = uniqueAccountNumber,
                    Balance = 0,
                    AccountType = dto.AccountType,
                    Currency = dto.Currency
                };

                await _accountRepo.AddAsync(account);
                await _accountRepo.SaveChangesAsync();

                _logger.LogInformation("Account created successfully. AccountNumber: {AccountNumber}", uniqueAccountNumber);

                return new AccountReadDto
                {
                    Id = account.Id,
                    AccountNumber = account.AccountNumber,
                    AccountName = account.AccountName,
                    Balance = account.Balance,
                    AccountType = account.AccountType.ToString(),
                    Currency = account.Currency.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account for user {UserId}", userId);
                throw;
            }
        }

        public async Task<object> GetAccountsByUserAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching accounts for user ID {UserId}", userId);

                var user = await _userRepo.GetUserWithAccountsAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User ID {UserId} not found when fetching accounts.", userId);
                    throw new NotFoundException("User not found.");
                }

                _logger.LogInformation("Fetched {Count} accounts for user ID {UserId}", user.Accounts?.Count ?? 0, userId);

                var accounts = user.Accounts?.Select(a => new
                {
                    a.Id,
                    a.AccountNumber,
                    a.AccountName,
                    a.AccountType,
                    a.Currency,
                    a.Balance
                }) ?? Enumerable.Empty<object>();

                return new
                {
                    message = $"Accounts for {user.FirstName} {user.LastName}",
                    accounts
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching accounts for user {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateAccountAsync(AccountUpdateDto dto)
        {
            try
            {
                _logger.LogInformation("Updating account ID {Id}", dto.Id);

                var account = await _accountRepo.GetByIdAsync(dto.Id);
                if (account == null)
                {
                    _logger.LogError("Account update failed. Account ID {Id} not found.", dto.Id);
                    throw new NotFoundException("Account not found.");
                }

                if (dto.Balance.HasValue)
                    account.Balance = dto.Balance.Value;

                if (dto.AccountType.HasValue)
                    account.AccountType = dto.AccountType.Value;

                if (dto.Currency.HasValue)
                    account.Currency = dto.Currency.Value;

                if (!string.IsNullOrEmpty(dto.AccountName))
                    account.AccountName = dto.AccountName;

                await _accountRepo.UpdateAsync(account);
                await _accountRepo.SaveChangesAsync();

                _logger.LogInformation("Account ID {Id} updated successfully.", dto.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account ID {Id}", dto.Id);
                throw;
            }
        }

        public async Task<object> SearchAccountAsync(string accountNumber)
        {
            try
            {
                _logger.LogInformation("Searching for accounts matching: {AccountNumber}", accountNumber);

                if (string.IsNullOrEmpty(accountNumber))
                {
                    _logger.LogWarning("Search failed — Empty account number provided.");
                    return new { success = false, message = "Please provide an account number to search." };
                }

                var accounts = await _accountRepo.SearchByAccountNumberAsync(accountNumber);

                if (!accounts.Any())
                {
                    _logger.LogWarning("No accounts found matching: {AccountNumber}", accountNumber);
                    return new { success = false, message = "No matching accounts found." };
                }

                _logger.LogInformation("{Count} accounts matched: {AccountNumber}", accounts.Count(), accountNumber);

                var result = accounts.Select(a => new
                {
                    a.Id,
                    a.AccountNumber,
                    a.AccountName,
                    a.AccountType,
                    a.Currency,
                    a.Balance,
                    Owner = a.User != null ? a.User.FirstName + " " + a.User.LastName : string.Empty
                });

                return new { success = true, data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching accounts for fragment {AccountNumber}", accountNumber);
                throw;
            }
        }

        public async Task DeleteAccountAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting account ID {Id}", id);

                var account = await _accountRepo.GetByIdAsync(id);
                if (account == null)
                {
                    _logger.LogWarning("Delete failed — Account ID {Id} not found.", id);
                    throw new NotFoundException("Account not found.");
                }

                await _accountRepo.DeleteAsync(account);
                await _accountRepo.SaveChangesAsync();

                _logger.LogInformation("Account ID {Id} deleted successfully", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account ID {Id}", id);
                throw;
            }
        }
    }
}
