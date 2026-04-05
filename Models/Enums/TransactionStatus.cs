namespace BankingAPI.Models.Enums
{
    /// <summary>
    /// Defines the status of a transaction
    /// </summary>
    public enum TransactionStatus
    {
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5,
        Refunded = 6
    }
}