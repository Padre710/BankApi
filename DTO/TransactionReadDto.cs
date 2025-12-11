using BankApi.Models;
using System.Text.Json.Serialization;

namespace BankApi.DTO
{
    public class TransactionReadDto
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public TransactionType TransactionType { get; set; } 
        public double Amount { get; set; }
        public TransactionDirection TransactionDirection { get; set; } 
        [JsonIgnore]
        public DateTime Timestamp { get; set; } 
        public Guid? TransferId { get; set; }
        public string FormattedTimestamp
        {
            get
            {
                return Timestamp.ToString("yyyy-MM-dd HH:mm");
            }
        }
    }

    

}
