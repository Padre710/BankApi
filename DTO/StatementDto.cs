namespace BankApi.DTO
{
    public class StatementLineDto
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public double Amount { get; set; }
        public double BalanceAfter { get; set; }
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
        public double OpeningBalance { get; set; }
        public double TotalCredits { get; set; }
        public double TotalDebits { get; set; }
        public double ClosingBalance { get; set; }
        public int TransactionCount { get; set; }
        public List<StatementLineDto> Lines { get; set; } = new();
    }
}