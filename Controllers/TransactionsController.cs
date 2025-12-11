//using BankApi.Constants;
//using BankApi.Db;
//using BankApi.DTO;
//using BankApi.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Identity.Client;
//using System.Security.Claims;

//namespace BankApi.Controllers
//{

//    [ApiController]
//    [Route("api/[controller]")]
//    public class TransactionsController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        public TransactionsController(AppDbContext context)
//        {
//            _context = context;
//        }
//      // BY ACCOUNT ID
//        [HttpGet("byAccount/{accountId}")]
//        [Authorize(Roles = Roles.AllRoles)]
//        public async Task<ActionResult<IEnumerable<TransactionReadDto>>> GetByAccountId(int accountId)
//        {
//            var role = User.FindFirst(ClaimTypes.Role)?.Value;
//            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

//            // If Customer → verify the account belongs to them
//            if (role == "Customer")
//            {
//                var account = await _context.Accounts
//                    .Where(a => a.Id == accountId)
//                    .Select(a => new { a.UserId })
//                    .FirstOrDefaultAsync();

//                if (account == null || account.UserId != userId)
//                    return NotFound($"Account {accountId} not found.");
//            }

//            // Admin/Teller or verified Customer
//            var transactions = await _context.Transactions
//                .Where(t => t.AccountId == accountId)
//                .OrderBy(t => t.timestamp)
//                .ToListAsync();

//            if (transactions == null || transactions.Count == 0)
//                return NotFound($"No transactions found for account {accountId}.");

//            return Ok(transactions);
//        }
//        // GET ALL 
//        [HttpGet]
//        [Authorize(Roles = Roles.Admin)]
//        public async Task<ActionResult<IEnumerable<TransactionReadDto>>> GetAll()
//        {
//            var transactions = await _context.Transactions.ToListAsync();
//            return Ok(transactions);
//        }
//        // GET BY TRANSACTION ID
//        [HttpGet("byTransaction/{TransactionId}")]
//        [Authorize(Roles = Roles.AllRoles)]
//        public async Task<ActionResult<Transaction>> GetByTransactionId(int TransactionId)
//        {
//            var role = User.FindFirst(ClaimTypes.Role)?.Value;
//            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

//            // If Customer → verify the transaction belongs to one of their accounts
//            if (role == "Customer")
//            {
//                var transactionCheck = await _context.Transactions
//                    .Where(t => t.Id == TransactionId)
//                    .Join(_context.Accounts,
//                          t => t.AccountId,
//                          a => a.Id,
//                          (t, a) => new { t.Id, a.UserId })
//                    .FirstOrDefaultAsync();

//                if (transactionCheck == null || transactionCheck.UserId != userId)
//                {
//                    return NotFound($"Transaction {TransactionId} not found.");
//                }
//            }

//            // Admin/Teller or verified Customer
//            var transactions = await _context.Transactions
//                .Where(a => a.Id == TransactionId)
//                .OrderBy(t => t.timestamp)
//                .ToListAsync();

//            if (transactions == null || transactions.Count == 0)
//                return NotFound($"Transaction {TransactionId} not found.");

//            return Ok(transactions);
//        }

//        // GET BY TRANSFER ID
//        [HttpGet("byTransfer/{transferId}")]
//        [Authorize(Roles = Roles.AdminOrTeller)]
//        public async Task<ActionResult<IEnumerable<Transaction>>> GetByTransferId(Guid transferId)
//        {

//            var transactions = await _context.Transactions
//                .Where(a => a.TransferId == transferId)
//                .OrderBy(t => t.timestamp)
//                .ToListAsync();
//            if (transactions == null || transactions.Count == 0)
//            {
//                return NotFound($"Transaction {transferId} not found.");
//            }
//            return Ok(transactions);
//        }

//        [HttpPost("deposit")]
//        [Authorize(Roles = Roles.AdminOrTeller)]
//        public async Task<ActionResult> Deposit(DepositDto dto)
//        {
//            var account = await _context.Accounts.FindAsync(dto.AccountId);
//            if (account == null) { return NotFound($"Account {dto.AccountId} not found."); }
//            if (dto.Amount <= 0) { return BadRequest("Deposits must be greater than Zero"); }

//            account.Balance += dto.Amount;

//            var transaction = new Transaction
//            {
//                Amount = dto.Amount,
//                AccountId = dto.AccountId,
//                TransactionType = TransactionType.Deposit,
//                TransactionDirection = TransactionDirection.Credit

//            };
//            await _context.Transactions.AddAsync(transaction);
//            await _context.SaveChangesAsync();

//            return Ok(new TransactionResponseDto
//            {
//                message = "Deposit Successful",
//                Txn = TransactionDirection.Credit.ToString(),
//                Amount = dto.Amount.ToString("F2"),
//                Date = transaction.timestamp.ToString("dd MMM yyyy HH:mm"),
//                Balance = account.Balance.ToString("F2")
//            });
//        }

//        [HttpPost("withdraw")]
//        [Authorize(Roles = Roles.AdminOrTeller)]
//        public async Task<ActionResult> Withdraw(WithdrawDto dto)
//        {
//            var account = await _context.Accounts.FindAsync(dto.AccountId);
//            if (account == null) { return NotFound($"Account {dto.AccountId} not found."); }
//            if (dto.Amount <= 0) { return BadRequest("Amount must be greater than zero"); }
//            if (dto.Amount > account.Balance) { return BadRequest("Insufficient Balance"); }

//            account.Balance -= dto.Amount;
//            var transaction = new Transaction
//            {
//                Amount = dto.Amount,
//                AccountId = dto.AccountId,
//                TransactionType = TransactionType.Withdraw,
//                TransactionDirection = TransactionDirection.Debit
//            };

//            await _context.Transactions.AddAsync(transaction);
//            await _context.SaveChangesAsync();

//            return Ok(new TransactionResponseDto
//            {
//                message = "Withdrawal Successful",
//                Txn = TransactionDirection.Debit.ToString(),
//                Amount = dto.Amount.ToString("F2"),
//                Date = transaction.timestamp.ToString("dd MMM yyyy HH:mm"),
//                Balance = account.Balance.ToString("F2")

//            });
//        }

//        [HttpPost("transfer")]
//        [Authorize(Roles = Roles.AllRoles)]
//        public async Task<ActionResult> Transfer(TransferDto dto)
//        {
//            var role = User.FindFirst(ClaimTypes.Role)?.Value;
//            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

//            // CUSTOMER: must own the FromAccount
//            if (role == Roles.Customer)
//            {
//                var fromAccount = await _context.Accounts
//                    .Where(a => a.Id == dto.FromAccountId)
//                    .Select(a => new { a.UserId })
//                    .FirstOrDefaultAsync();

//                if (fromAccount == null || fromAccount.UserId != userId)
//                {
//                    return NotFound("From account not found or does not belong to you.");
//                }
//            }

//            // Fetch accounts for the transfer
//            var FromAccount = await _context.Accounts.FindAsync(dto.FromAccountId);
//            var ToAccount = await _context.Accounts.FindAsync(dto.ToAccountId);

//            if (FromAccount == null || ToAccount == null)
//                return NotFound("Account(s) not found.");

//            if (dto.FromAccountId == dto.ToAccountId)
//                return BadRequest("Cannot transfer to the same account.");

//            if (dto.Amount <= 0)
//                return BadRequest("Amount must be greater than zero.");

//            if (dto.Amount > FromAccount.Balance)
//                return BadRequest("Insufficient Balance.");

//            if (FromAccount.Currency != ToAccount.Currency)
//                return BadRequest("Cannot transfer between different currencies.");

//            // Perform transfer
//            FromAccount.Balance -= dto.Amount;
//            ToAccount.Balance += dto.Amount;

//            var transferId = Guid.NewGuid();

//            var transaction1 = new Transaction
//            {
//                Amount = dto.Amount,
//                AccountId = dto.FromAccountId,
//                TransactionType = TransactionType.Transfer,
//                TransactionDirection = TransactionDirection.Debit,
//                TransferId = transferId
//            };

//            var transaction2 = new Transaction
//            {
//                Amount = dto.Amount,
//                AccountId = dto.ToAccountId,
//                TransactionType = TransactionType.Transfer,
//                TransactionDirection = TransactionDirection.Credit,
//                TransferId = transferId
//            };

//            await _context.Transactions.AddAsync(transaction1);
//            await _context.Transactions.AddAsync(transaction2);
//            await _context.SaveChangesAsync();

//            return Ok(new TransactionResponseDto
//            {
//                message = $"Transfer to Account id {transaction2.AccountId} successful",
//                Txn = TransactionDirection.Debit.ToString(),
//                Amount = dto.Amount.ToString("F2"),
//                Date = transaction1.timestamp.ToString("dd MMM yyyy HH:mm"),
//                Balance = FromAccount.Balance.ToString("F2")
//            });
//        }


//        // GET ALL BY USER ID
//        [HttpGet("byUser/{userId}")]
//        [Authorize(Roles = Roles.AllRoles)]
//        public async Task<ActionResult<IEnumerable<TransactionReadDto>>> GetAllByUserId(int userId)
//        {
//            var role = User.FindFirst(ClaimTypes.Role)?.Value;
//            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

//            // If Customer → can only access their own userId
//            if (role == Roles.Customer && currentUserId != userId)
//            {
//                return NotFound($"No accounts found for User {userId}.");
//            }

//            var accounts = await _context.Accounts
//                .Where(a => a.UserId == userId)
//                .ToListAsync();

//            if (accounts == null || accounts.Count == 0)
//            {
//                return NotFound($"No accounts found for User {userId}.");
//            }

//            var accountIds = accounts.Select(a => a.Id).ToList();
//            var transactions = await _context.Transactions
//    .Where(t => accountIds.Contains(t.AccountId))
//    .OrderBy(t => t.timestamp)
//    .Select(t => new TransactionReadDto
//    {
//        Id = t.Id,
//        Amount = t.Amount,
//        AccountId = t.AccountId,
//        TransactionType = t.TransactionType,
//        TransactionDirection = t.TransactionDirection,
//        TransferId = t.TransferId,
//        Timestamp = t.timestamp
//    })
//    .ToListAsync();

//            return Ok(transactions);



//        }

//        // FILTER ENDPOINT
//        [HttpGet("filter")]
//        [Authorize(Roles = Roles.AdminOrTeller)]
//        public async Task<IActionResult> FilterTransactions(
//    [FromQuery] TransactionType? type,
//    [FromQuery] TransactionDirection? direction,
//    [FromQuery] DateTime? dateFrom,
//    [FromQuery] DateTime? dateTo)
//        {
//            var query = _context.Transactions.AsQueryable();

//            if (type.HasValue)
//                query = query.Where(t => t.TransactionType == type);

//            if (direction.HasValue)
//                query = query.Where(t => t.TransactionDirection == direction);

//            if (dateFrom.HasValue)
//                query = query.Where(t => t.timestamp >= dateFrom);

//            if (dateTo.HasValue)
//                query = query.Where(t => t.timestamp <= dateTo);

//            var transactions = await query
//               // .Include(t => t.Account)
//                .OrderByDescending(t => t.timestamp)
//                .ToListAsync();


//            return Ok(transactions);
//        }

//    }
//}   

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
