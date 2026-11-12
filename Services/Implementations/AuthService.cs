using BankApi.DTO;
using BankApi.Helpers;
using BankApi.Models;
using BankApi.Services;
using BankApi.Repositories.Interfaces;
using BankApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BankApi.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly RedisCacheService _redis;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IAuthRepository authRepository,
            JwtHelper jwtHelper,
            RedisCacheService redis,
            ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _jwtHelper = jwtHelper;
            _redis = redis;
            _logger = logger;
        }

        public async Task<string> RegisterAsync(RegisterDto dto)
        {
            _logger.LogInformation("Attempting registration for email: {Email}", dto.Email);

            var existingUser = await _authRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed — Email already exists: {Email}", dto.Email);
                return "Email already exists.";
            }

            PasswordHelper.CreatePasswordHash(dto.Password, out byte[] hash, out byte[] salt);

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Role = dto.Role,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            await _authRepository.AddUserAsync(user);

            _logger.LogInformation("User registered successfully: {Email}", dto.Email);

            return "User registered successfully";
        }

        public async Task<(string token, User user)> LoginAsync(LoginDto dto)
        {
            string redisKey = $"login_attempts:{dto.Email.ToLower()}";
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            // 1) Read attempts
            var attemptsValue = await _redis.GetAsync<string>(redisKey);
            int attempts = attemptsValue == null ? 0 : int.Parse(attemptsValue);

            _logger.LogInformation("Current failed login attempts for {Email}: {Attempts}", dto.Email, attempts);

            // 2) Enforce rate limit
            if (attempts >= 5)
            {
                _logger.LogWarning("Rate limit reached for {Email}. Attempts: {Attempts}", dto.Email, attempts);
                throw new Exception("Too many failed login attempts. Please wait 1 minute.");
            }

            // 3) Look up user
            var user = await _authRepository.GetUserByEmailAsync(dto.Email);
            if (user == null)
            {
                attempts++;
                await _redis.SetAsync<string>(redisKey, attempts.ToString(), TimeSpan.FromMinutes(1));

                _logger.LogWarning("Invalid login — User not found. Email: {Email}, Attempts: {Attempts}", dto.Email, attempts);

                throw new Exception("Invalid Email or password");
            }

            // 4) Verify password
            if (!PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
            {
                attempts++;
                await _redis.SetAsync<string>(redisKey, attempts.ToString(), TimeSpan.FromMinutes(1));

                _logger.LogWarning("Invalid login — Wrong password. Email: {Email}, Attempts: {Attempts}", dto.Email, attempts);

                throw new Exception("Invalid Email or password");
            }

            // 5) Success → clear attempts
            await _redis.RemoveAsync(redisKey);

            _logger.LogInformation("Successful login for {Email}. Failed attempts reset.", dto.Email);

            // 6) Generate token
            var token = _jwtHelper.GenerateToken(user);

            _logger.LogInformation("JWT generated for {Email}", dto.Email);

            return (token, user);
        }

        public async Task<User?> GetProfileAsync(int userId)
        {
            _logger.LogInformation("Fetching profile for UserId: {UserId}", userId);

            var user = await _authRepository.GetUserWithAccountsAsync(userId);

            if (user == null)
                _logger.LogWarning("Profile not found for UserId: {UserId}", userId);
            else
                _logger.LogInformation("Profile fetched successfully for UserId: {UserId}", userId);

            return user;
        }
    }
}
