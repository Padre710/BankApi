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

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        private string Role => User.FindFirst(ClaimTypes.Role)?.Value;

        [HttpGet("byAccount/{accountId}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> GetByAccountId(int accountId)
        {
            return Ok(await _service.GetByAccountId(accountId, CurrentUserId, Role));
        }

        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAll());
        }

        [HttpGet("byTransaction/{id}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> GetByTransactionId(int id)
        {
            return Ok(await _service.GetByTransactionId(id, CurrentUserId, Role));
        }

        [HttpGet("byTransfer/{transferId}")]
        [Authorize(Roles = Roles.AdminOrTeller)]
        public async Task<IActionResult> GetByTransferId(Guid transferId)
        {
            return Ok(await _service.GetByTransferId(transferId));
        }

        [HttpGet("byUser/{userId}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            return Ok(await _service.GetByUserId(userId, Role, CurrentUserId));
        }

        [HttpPost("deposit")]
        [Authorize(Roles = Roles.AdminOrTeller)]
        public async Task<IActionResult> Deposit(DepositDto dto)
        {
            return Ok(await _service.Deposit(dto));
        }

        [HttpPost("withdraw")]
        [Authorize(Roles = Roles.AdminOrTeller)]
        public async Task<IActionResult> Withdraw(WithdrawDto dto)
        {
            return Ok(await _service.Withdraw(dto));
        }

        [HttpPost("transfer")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> Transfer(TransferDto dto)
        {
            return Ok(await _service.Transfer(dto, CurrentUserId, Role));
        }

        [HttpGet("filter")]
        [Authorize(Roles = Roles.AdminOrTeller)]
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
