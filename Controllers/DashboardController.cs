using BankApi.Constants;
using BankApi.Db;
using BankApi.Models;
using BankApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Controllers
{
    [ApiController]
    [Authorize (Roles = Roles.AdminOrTeller) ]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RedisCacheService _redis;

        public DashboardController(AppDbContext context, RedisCacheService redis)
        {
            _context = context;
            _redis = redis;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            const string cacheKey = "dashboard_summary";

            //  Try cache first
            var cached = await _redis.GetAsync<object>(cacheKey);
            if (cached != null)
                return Ok(new
                {
                    cached = true,
                    data = cached
                });

            // 2️⃣ If cache is empty, fetch from DB
            var summary = new
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalAccounts = await _context.Accounts.CountAsync(),
                TotalTransactions = await _context.Transactions.CountAsync(),
                TotalBalance = await _context.Accounts.SumAsync(a => a.Balance)
            };

            // 3️⃣ Save to cache for 10 minutes
            await _redis.SetAsync(cacheKey, summary, TimeSpan.FromMinutes(10));

            return Ok(new
            {
                cached = false,
                data = summary
            });
        }
    }
}
