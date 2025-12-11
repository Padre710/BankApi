using BankApi.DTO;
using BankApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (result == "Email already exists.")
                return BadRequest(result);

            return Ok(new { message = result });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var (token, user) = await _authService.LoginAsync(dto);
                return Ok(new
                {
                    message = "Login successful",
                    token,
                    user = new
                    {
                        user.Id,
                        user.FirstName,
                        user.Email,
                        Role = user.Role.ToString()
                    }
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid or missing token.");

            int userId = int.Parse(userIdClaim);
            var user = await _authService.GetProfileAsync(userId);

            if (user == null)
                return NotFound("User not found.");

            return Ok(new
            {
                message = $"Hi, {user.FirstName} 👋",
                user.FirstName,
                user.Email,
                Role = user.Role.ToString(),
                Accounts = user.Accounts.Select(a => new
                {
                    a.Id,
                    a.AccountNumber,
                    a.AccountType,
                    a.Currency,
                    a.Balance
                })
            });
        }
    }
}

