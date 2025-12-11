using BankApi.Models;
using System.Globalization;
using System.Text.Json.Serialization;

namespace BankApi.DTO
{
    public class TransactionResponseDto
    {
        public string message { get; set; } = string.Empty;
        public string Txn { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string Balance { get; set; } = string.Empty;
        public String Date { get; set; } = String.Empty;

    }
}
