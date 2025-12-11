using System.ComponentModel.DataAnnotations;

namespace BankApi.DTO
{
    public class DepositDto
    {
        [Required]
        public int AccountId { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public double Amount { get; set; }
    }

}
