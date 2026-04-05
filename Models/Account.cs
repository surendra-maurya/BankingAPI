using BankingAPI.Models.Enums;

namespace BankingAPI.Models
{
    /// <summary>
    /// Represents a bank account
    /// </summary>
    public class Account
    {
        public string AccountId { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public string IFSC { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string UPIId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastTransactionDate { get; set; }
    }
}