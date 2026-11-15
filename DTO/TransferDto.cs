using System.ComponentModel.DataAnnotations;

namespace BankApi.DTO
{
    public class TransferDto
    {
        [Required]
        public int FromAccountId { get; set; }

        [Required]
        public int ToAccountId { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }

}
