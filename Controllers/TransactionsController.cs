using BankApi.Constants;
using BankApi.DTO;
using BankApi.Models;
using BankApi.Services;
using BankApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _service;
        public TransactionsController(ITransactionService service)
        {
            _service = service;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        private string Role => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        [HttpGet("byAccount/{accountId}")]
        [Authorize]
        public async Task<IActionResult> GetByAccountId(int accountId)
        {
            return Ok(await _service.GetByAccountId(accountId, CurrentUserId, Role));
        }

        [HttpGet]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAll());
        }

        [HttpGet("byTransaction/{id}")]
        [Authorize]
        public async Task<IActionResult> GetByTransactionId(int id)
        {
            return Ok(await _service.GetByTransactionId(id, CurrentUserId, Role));
        }

        [HttpGet("byTransfer/{transferId}")]
        [Authorize(Policy = Policies.TellerOrAdmin)]
        public async Task<IActionResult> GetByTransferId(Guid transferId)
        {
            return Ok(await _service.GetByTransferId(transferId));
        }

        [HttpGet("byUser/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            return Ok(await _service.GetByUserId(userId, Role, CurrentUserId));
        }

        [HttpPost("deposit")]
        [Authorize(Policy = Policies.TellerOrAdmin)]
        public async Task<IActionResult> Deposit(DepositDto dto)
        {
            return Ok(await _service.Deposit(dto));
        }

        [HttpPost("withdraw")]
        [Authorize(Policy = Policies.TellerOrAdmin)]
        public async Task<IActionResult> Withdraw(WithdrawDto dto)
        {
            return Ok(await _service.Withdraw(dto));
        }

        [HttpPost("transfer")]
        [Authorize]
        public async Task<IActionResult> Transfer(TransferDto dto)
        {
            return Ok(await _service.Transfer(dto, CurrentUserId, Role));
        }

        /// <summary>
        /// Returns a monthly statement for one account: opening balance, every
        /// transaction in the month with a running balance, totals, and closing balance.
        /// Customers can only request their own accounts; Tellers and Admins can request any.
        /// </summary>
        /// <param name="accountId">The account to produce a statement for.</param>
        /// <param name="year">Four-digit year, e.g. 2026.</param>
        /// <param name="month">Month number, 1-12.</param>
        /// <response code="200">The statement.</response>
        /// <response code="400">Invalid or future month/year.</response>
        /// <response code="404">Account not found (or not yours).</response>
        [HttpGet("statement/{accountId}")]
        [Authorize]
        [ProducesResponseType(typeof(StatementDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStatement(int accountId, [FromQuery] int year, [FromQuery] int month)
        {
            return Ok(await _service.GetMonthlyStatement(accountId, year, month, CurrentUserId, Role));
        }

        /// <summary>
        /// Same statement as above, returned as a downloadable PDF file.
        /// </summary>
        /// <param name="accountId">The account to produce a statement for.</param>
        /// <param name="year">Four-digit year, e.g. 2026.</param>
        /// <param name="month">Month number, 1-12.</param>
        /// <response code="200">A PDF file.</response>
        /// <response code="400">Invalid or future month/year.</response>
        /// <response code="404">Account not found (or not yours).</response>
        [HttpGet("statement/{accountId}/pdf")]
        [Authorize]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStatementPdf(int accountId, [FromQuery] int year, [FromQuery] int month)
        {
            var bytes = await _service.GetMonthlyStatementPdf(accountId, year, month, CurrentUserId, Role);
            return File(bytes, "application/pdf", $"statement_{accountId}_{year}-{month:D2}.pdf");
        }

        [HttpGet("filter")]
        [Authorize(Policy = Policies.TellerOrAdmin)]
        public async Task<IActionResult> Filter(
            [FromQuery] TransactionType? type,
            [FromQuery] TransactionDirection? direction,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            return Ok(await _service.Filter(type, direction, from, to));
        }
    }
}
