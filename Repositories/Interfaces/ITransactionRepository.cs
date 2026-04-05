using BankingAPI.Models;
using BankingAPI.Models.Enums;

namespace BankingAPI.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for transaction operations
    /// </summary>
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task<Transaction?> GetTransactionByIdAsync(string transactionId);
        Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(string accountId);
        Task<IEnumerable<Transaction>> GetTransactionsByStatusAsync(TransactionStatus status);
        Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<bool> UpdateTransactionStatusAsync(string transactionId, TransactionStatus status, string? failureReason = null);
    }
}