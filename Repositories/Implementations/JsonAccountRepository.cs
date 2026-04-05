using BankingAPI.Models;
using BankingAPI.Repositories.Interfaces;
using Newtonsoft.Json;

namespace BankingAPI.Repositories.Implementations
{
    /// <summary>
    /// JSON file based implementation of account repository
    /// </summary>
    public class JsonAccountRepository : IAccountRepository
    {
        private readonly string _filePath;
        private readonly ILogger<JsonAccountRepository> _logger;
        private List<Account> _accounts;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public JsonAccountRepository(IConfiguration configuration, ILogger<JsonAccountRepository> logger)
        {
            _logger = logger;
            _filePath = configuration["DataFiles:AccountsPath"] ?? "Data/accounts.json";
            _accounts = LoadAccounts();
        }

        private List<Account> LoadAccounts()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    return JsonConvert.DeserializeObject<List<Account>>(json) ?? new List<Account>();
                }
                _logger.LogWarning("Accounts file not found at {Path}", _filePath);
                return new List<Account>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading accounts from JSON file");
                return new List<Account>();
            }
        }

        private async Task SaveAccountsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                var json = JsonConvert.SerializeObject(_accounts, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            return Task.FromResult(_accounts.AsEnumerable());
        }

        public Task<Account?> GetAccountByIdAsync(string accountId)
        {
            var account = _accounts.FirstOrDefault(a =>
                a.AccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(account);
        }

        public Task<Account?> GetAccountByNumberAsync(string accountNumber)
        {
            var account = _accounts.FirstOrDefault(a =>
                a.AccountNumber.Equals(accountNumber, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(account);
        }

        public Task<Account?> GetAccountByUPIIdAsync(string upiId)
        {
            var account = _accounts.FirstOrDefault(a =>
                a.UPIId.Equals(upiId, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(account);
        }

        public async Task<bool> UpdateAccountBalanceAsync(string accountId, decimal newBalance)
        {
            var account = _accounts.FirstOrDefault(a =>
                a.AccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase));

            if (account == null) return false;

            account.Balance = newBalance;
            account.LastTransactionDate = DateTime.UtcNow;

            await SaveAccountsAsync();
            _logger.LogInformation("Updated balance for account {AccountId} to {Balance}",
                accountId, newBalance);

            return true;
        }

        public Task<bool> AccountExistsAsync(string accountId)
        {
            var exists = _accounts.Any(a =>
                a.AccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(exists);
        }

        public Task<decimal> GetBalanceAsync(string accountId)
        {
            var account = _accounts.FirstOrDefault(a =>
                a.AccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(account?.Balance ?? 0);
        }
    }
}