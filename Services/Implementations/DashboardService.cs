using BankApi.DTO;
using BankApi.Repositories.Interfaces;
using BankApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BankApi.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IUserRepository _userRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly RedisCacheService _redis;
        private readonly ILogger<DashboardService> _logger;

        private const string CacheKey = "dashboard_summary";

        public DashboardService(
            IUserRepository userRepo,
            IAccountRepository accountRepo,
            ITransactionRepository transactionRepo,
            RedisCacheService redis,
            ILogger<DashboardService> logger)
        {
            _userRepo = userRepo;
            _accountRepo = accountRepo;
            _transactionRepo = transactionRepo;
            _redis = redis;
            _logger = logger;
        }

        public async Task<DashboardResponseDto> GetSummaryAsync()
        {
            _logger.LogInformation("DashboardService: Fetching summary");

            // 1. Try cache first
            var cached = await _redis.GetAsync<DashboardSummaryDto>(CacheKey);
            if (cached != null)
            {
                _logger.LogInformation("DashboardService: Returning cached summary");
                return new DashboardResponseDto
                {
                    Cached = true,
                    Data = cached
                };
            }

            // 2. Fetch from repositories
            var summary = new DashboardSummaryDto
            {
                TotalUsers = await _userRepo.GetCountAsync(),
                TotalAccounts = await _accountRepo.GetCountAsync(),
                TotalTransactions = await _transactionRepo.GetCountAsync(),
                TotalBalance = await _accountRepo.GetTotalBalanceAsync()
            };

            // 3. Cache for 10 minutes
            await _redis.SetAsync(CacheKey, summary, TimeSpan.FromMinutes(10));
            _logger.LogInformation("DashboardService: Summary computed and cached for 10 minutes");

            return new DashboardResponseDto
            {
                Cached = false,
                Data = summary
            };
        }
    }
}
