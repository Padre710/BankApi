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
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public double Amount { get; set; }
    }

}
