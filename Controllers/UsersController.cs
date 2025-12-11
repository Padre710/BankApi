using BankApi.Constants;
using BankApi.Models;
using BankApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAllUsers()
        {            
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var users = await _userService.GetAllUsersAsync(role!);
                return Ok(users);  
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> GetUserById(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // If Customer → can only access their own user record
            if (role == Roles.Customer && userId != id)
            {
                return NotFound($"User with ID {id} not found.");
            }

            // Admin or verified Customer
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(user);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var message = await _userService.UpdateUserAsync(id, updatedUser, role!);
                return Ok(new { message });     
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteUser(int id)
        {   
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var message = await _userService.DeleteUserAsync(id, role!);
                return Ok(new { message });
        }

        [HttpGet("customers")]
        [Authorize(Roles = Roles.AdminOrTeller)]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _userService.GetCustomersAsync();
            return Ok(customers);
        }

        [HttpGet("tellers")]
        [Authorize(Roles = Roles.AdminOrTeller)]
        public async Task<IActionResult> GetTellers()
        {
            var tellers = await _userService.GetTellersAsync();
            return Ok(tellers);
        }
    }
}
