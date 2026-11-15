using BankApi.Constants;
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
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        private string Role => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        [HttpGet]
        [Authorize(Policy = Policies.TellerOrAdmin)]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var account = await _accountService.GetAccountByIdAsync(id, CurrentUserId, Role);
            if (account == null)
                return NotFound();

            return Ok(account);
        }

        [HttpGet("{id}/balance")]
        [Authorize]
        public async Task<IActionResult> GetBalanceById(int id)
        {
            var balance = await _accountService.GetBalanceByIdAsync(id, CurrentUserId, Role);
            if (balance == null)
                return NotFound();

            return Ok(balance);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] AccountCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token or missing user ID.");
            var name = User.FindFirst("firstName")?.Value + " " + User.FindFirst("lastName")?.Value;

            var userId = int.Parse(userIdClaim);

            var newAccount = await _accountService.CreateAccountAsync(userId, name, dto);
            return CreatedAtAction(nameof(GetById), new { id = newAccount.Id }, newAccount);
        }

        [HttpGet("by-user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetAccountsByUser(int userId)
        {
            if (Role == Roles.Customer && CurrentUserId != userId)
                return Forbid();

            var result = await _accountService.GetAccountsByUserAsync(userId);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Update([FromBody] AccountUpdateDto dto)
        {
            await _accountService.UpdateAccountAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            await _accountService.DeleteAccountAsync(id);
            return NoContent();
        }

        [HttpGet("search")]
        [Authorize(Policy = Policies.TellerOrAdmin)]
        public async Task<IActionResult> SearchAccount([FromQuery] string accountNumber)
        {
            var result = await _accountService.SearchAccountAsync(accountNumber);
            return Ok(result);
        }
    }
}
