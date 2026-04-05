using BankingAPI.Models.Enums;

namespace BankingAPI.Models
{
    /// <summary>
    /// Represents a financial transaction
    /// </summary>
    public class Transaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string FromAccountId { get; set; } = string.Empty;
        public string ToAccountId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }
        public TransactionStatus Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string FailureReason { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}