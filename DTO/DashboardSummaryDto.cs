namespace BankApi.DTO
{
    public class DashboardSummaryDto
    {
        public int TotalUsers { get; set; }
        public int TotalAccounts { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalBalance { get; set; }
    }
}
