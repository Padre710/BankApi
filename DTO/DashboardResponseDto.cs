namespace BankApi.DTO
{
    public class DashboardResponseDto
    {
        public bool Cached { get; set; }
        public DashboardSummaryDto Data { get; set; } = new();
    }
}
