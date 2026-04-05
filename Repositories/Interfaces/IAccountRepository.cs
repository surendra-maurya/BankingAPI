using BankingAPI.Models;

namespace BankingAPI.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for account operations
    /// </summary>
    public interface IAccountRepository
    {
        Task<IEnumerable<Account>> GetAllAccountsAsync();
        Task<Account?> GetAccountByIdAsync(string accountId);
        Task<Account?> GetAccountByNumberAsync(string accountNumber);
        Task<Account?> GetAccountByUPIIdAsync(string upiId);
        Task<bool> UpdateAccountBalanceAsync(string accountId, decimal newBalance);
        Task<bool> AccountExistsAsync(string accountId);
        Task<decimal> GetBalanceAsync(string accountId);
    }
}