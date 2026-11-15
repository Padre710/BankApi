namespace BankApi.DTO
{
    public class StatementLineDto
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public Guid? TransferId { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class StatementDto
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal ClosingBalance { get; set; }
        public int TransactionCount { get; set; }
        public List<StatementLineDto> Lines { get; set; } = new();
    }
}