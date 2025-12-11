//using BankApi.Db;
//using BankApi.Models;
//using BankApi.Repositories.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using System.Threading.Tasks;

//namespace BankApi.Repositories.Implementations
//{
//    public class AuthRepository : IAuthRepository
//    {
//        private readonly AppDbContext _context;

//        public AuthRepository(AppDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<User?> GetUserByEmailAsync(string email)
//        {
//            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
//        }

//        public async Task AddUserAsync(User user)
//        {
//            await _context.Users.AddAsync(user);
//            await _context.SaveChangesAsync();
//        }

//        public async Task<User?> GetUserWithAccountsAsync(int userId)
//        {
//            return await _context.Users
//                .Include(u => u.Accounts)
//                .FirstOrDefaultAsync(u => u.Id == userId);
//        }
//    }
//}

using BankApi.Db;
using BankApi.Models;
using BankApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BankApi.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(AppDbContext context, ILogger<AuthRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            _logger.LogInformation("AuthRepository: Fetching user by email {Email}", email);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                _logger.LogWarning("AuthRepository: No user found with email {Email}", email);
            }
            else
            {
                _logger.LogInformation("AuthRepository: User with email {Email} retrieved successfully", email);
            }

            return user;
        }

        public async Task AddUserAsync(User user)
        {
            _logger.LogInformation("AuthRepository: Adding new user with email {Email}", user.Email);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("AuthRepository: User {UserId} created successfully", user.Id);
        }

        public async Task<User?> GetUserWithAccountsAsync(int userId)
        {
            _logger.LogInformation("AuthRepository: Fetching user {UserId} with accounts", userId);

            var user = await _context.Users
                .Include(u => u.Accounts)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("AuthRepository: User {UserId} not found", userId);
            }
            else
            {
                _logger.LogInformation("AuthRepository: User {UserId} retrieved with {Count} accounts",
                    userId, user.Accounts?.Count ?? 0);
            }

            return user;
        }
    }
}
