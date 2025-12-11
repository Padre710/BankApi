using BankApi.Constants;
using BankApi.Db;
using BankApi.DTO;
using BankApi.Models;
using BankApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly AppDbContext _context;

        public AccountsController(IAccountService accountService, AppDbContext context)
        {
            _accountService = accountService;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = Roles.AdminOrTeller)]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> GetById(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // If Customer → verify they own the account
            if (role == Roles.Customer)
            {
                var accountCheck = await _context.Accounts
                    .Where(a => a.Id == id)
                    .Select(a => new { a.UserId })
                    .FirstOrDefaultAsync();

                if (accountCheck == null || accountCheck.UserId != userId)
                    return NotFound(); // hides existence
            }

            // Admin/Teller or verified Customer
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound();

            return Ok(account);
        }

        [HttpGet("{id}/balance")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> GetBalanceById(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // If the caller is a Customer → verify this account belongs to them
            if (role == Roles.Customer)
            {
                var account = await _context.Accounts
                    .Where(a => a.Id == id)
                    .Select(a => new { a.UserId })
                    .FirstOrDefaultAsync();

                if (account == null || account.UserId != userId)
                    return NotFound(); // Hide account existence for security
            }

            // Admin / Teller OR verified customer
            var balance = await _accountService.GetBalanceByIdAsync(id);

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

            var newAccount = await _accountService.CreateAccountAsync(userId,name, dto);
            return CreatedAtAction(nameof(GetById), new { id = newAccount.Id }, newAccount);
        }

        [HttpGet("by-user/{userId}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> GetAccountsByUser(int userId)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var Id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (userRole == Roles.Customer && Id != userId)
                return Forbid();

            var result = await _accountService.GetAccountsByUserAsync(userId);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Update([FromBody] AccountUpdateDto dto)
        {
            await _accountService.UpdateAccountAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            await _accountService.DeleteAccountAsync(id);
            return NoContent();
        }

        [HttpGet("search")]
        [Authorize(Roles = Roles.AdminOrTeller)]
        public async Task<IActionResult> SearchAccount([FromQuery] string accountNumber)
        {
            var result = await _accountService.SearchAccountAsync(accountNumber);
            return Ok(result);
        }
    }
}
