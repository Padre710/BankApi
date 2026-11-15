namespace BankApi.Exceptions
{
    public class InsufficientBalanceException : BadRequestException
    {
        public InsufficientBalanceException(string message = "Insufficient balance.") : base(message) { }
    }
}
