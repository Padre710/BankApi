using System.ComponentModel.DataAnnotations;

namespace BankApi.DTO
{
    public class DepositDto
    {
        [Required]
        public int AccountId { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }

}
