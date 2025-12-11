//using BankApi.Db;
//using BankApi.Models;
//using BankApi.Repositories.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace BankApi.Repositories.Implementations
//{
//    public class UserRepository : IUserRepository
//    {
//        private readonly AppDbContext _context;

//        public UserRepository(AppDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IEnumerable<User>> GetAllUsersAsync()
//        {
//            return await _context.Users.ToListAsync();
//        }

//        public async Task<User?> GetUserByIdAsync(int id)
//        {
//            return await _context.Users.FindAsync(id);
//        }

//        public async Task UpdateUserAsync(User user)
//        {
//            _context.Users.Update(user);
//            await _context.SaveChangesAsync();
//        }

//        public async Task DeleteUserAsync(User user)
//        {
//            _context.Users.Remove(user);
//            await _context.SaveChangesAsync();
//        }

//        public async Task<IEnumerable<User>> GetCustomersAsync()
//        {
//            return await _context.Users
//                .Where(u => u.Role == Role.Customer)
//                .ToListAsync();
//        }

//        public async Task<IEnumerable<User>> GetTellersAsync()
//        {
//            return await _context.Users
//                .Where(u => u.Role == Role.Teller)
//                .ToListAsync();
//        }
//    }
//}

using BankApi.Db;
using BankApi.Models;
using BankApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankApi.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            _logger.LogInformation("Repository: Fetching all users");

            var users = await _context.Users.ToListAsync();

            _logger.LogInformation("Repository: Retrieved {Count} users", users.Count);

            return users;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            _logger.LogInformation("Repository: Fetching user by ID {Id}", id);

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                _logger.LogWarning("Repository: User with ID {Id} not found", id);
            }
            else
            {
                _logger.LogInformation("Repository: User with ID {Id} retrieved successfully", id);
            }

            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            _logger.LogInformation("Repository: Updating user {UserId}", user.Id);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Repository: User {UserId} updated successfully", user.Id);
        }

        public async Task DeleteUserAsync(User user)
        {
            _logger.LogInformation("Repository: Deleting user {UserId}", user.Id);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Repository: User {UserId} deleted successfully", user.Id);
        }

        public async Task<IEnumerable<User>> GetCustomersAsync()
        {
            _logger.LogInformation("Repository: Fetching all Customers");

            var customers = await _context.Users
                .Where(u => u.Role == Role.Customer)
                .ToListAsync();

            _logger.LogInformation("Repository: Retrieved {Count} customers", customers.Count);

            return customers;
        }

        public async Task<IEnumerable<User>> GetTellersAsync()
        {
            _logger.LogInformation("Repository: Fetching all Tellers");

            var tellers = await _context.Users
                .Where(u => u.Role == Role.Teller)
                .ToListAsync();

            _logger.LogInformation("Repository: Retrieved {Count} tellers", tellers.Count);

            return tellers;
        }
    }
}
