namespace BankApi.DTO
{
    public class AccountReadDto
    {
        public int Id { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string AccountType { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;

    }
}
