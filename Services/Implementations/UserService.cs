using BankApi.Constants;
using BankApi.Models;
using BankApi.Repositories.Interfaces;
using BankApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApi.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(string role)
        {
            _logger.LogInformation("Request to fetch all users by role: {Role}", role);

            if (role != Roles.Admin)
            {
                _logger.LogWarning("Unauthorized attempt to fetch all users. Role: {Role}", role);
                throw new UnauthorizedAccessException("Only admins can view all users.");
            }

            var users = await _userRepository.GetAllUsersAsync();

            _logger.LogInformation("Fetched {Count} users successfully.", users?.Count() ?? 0);
            return users;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            _logger.LogInformation("Fetching user by ID: {Id}", id);

            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
                _logger.LogWarning("User with ID {Id} not found.", id);
            else
                _logger.LogInformation("User with ID {Id} fetched successfully.", id);

            return user;
        }

        public async Task<string> UpdateUserAsync(int id, User updatedUser, string role)
        {
            _logger.LogInformation("Attempting to update user ID {Id} by role {Role}", id, role);

            if (role != Roles.Admin)
            {
                _logger.LogWarning("Unauthorized user update attempt for ID {Id} by role {Role}", id, role);
                throw new UnauthorizedAccessException("Only admins can update users.");
            }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Update failed — User with ID {Id} not found.", id);
                return $"User with ID {id} not found.";
            }

            _logger.LogInformation("Updating user ID {Id}...", id);

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.Role = updatedUser.Role;

            await _userRepository.UpdateUserAsync(user);

            _logger.LogInformation("User ID {Id} updated successfully.", id);
            return "User updated successfully.";
        }

        public async Task<string> DeleteUserAsync(int id, string role)
        {
            _logger.LogInformation("Attempting to delete user ID {Id} by role {Role}", id, role);

            if (role != Roles.Admin)
            {
                _logger.LogWarning("Unauthorized delete attempt for ID {Id} by role {Role}", id, role);
                throw new UnauthorizedAccessException("Only admins can delete users.");
            }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Delete failed — User with ID {Id} not found.", id);
                return $"User with ID {id} not found.";
            }

            await _userRepository.DeleteUserAsync(user);

            _logger.LogInformation("User ID {Id} deleted successfully.", id);
            return "User deleted successfully.";
        }

        public async Task<IEnumerable<User>> GetCustomersAsync()
        {
            _logger.LogInformation("Fetching all customers...");
            var customers = await _userRepository.GetCustomersAsync();
            _logger.LogInformation("Fetched {Count} customers.", customers?.Count() ?? 0);
            return customers;
        }

        public async Task<IEnumerable<User>> GetTellersAsync()
        {
            _logger.LogInformation("Fetching all tellers...");
            var tellers = await _userRepository.GetTellersAsync();
            _logger.LogInformation("Fetched {Count} tellers.", tellers?.Count() ?? 0);
            return tellers;
        }
    }
}
