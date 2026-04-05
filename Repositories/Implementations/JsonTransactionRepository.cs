using BankingAPI.Models;
using BankingAPI.Models.Enums;
using BankingAPI.Repositories.Interfaces;
using Newtonsoft.Json;

namespace BankingAPI.Repositories.Implementations
{
    /// <summary>
    /// JSON file based implementation of transaction repository
    /// </summary>
    public class JsonTransactionRepository : ITransactionRepository
    {
        private readonly string _filePath;
        private readonly ILogger<JsonTransactionRepository> _logger;
        private List<Transaction> _transactions;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public JsonTransactionRepository(IConfiguration configuration, ILogger<JsonTransactionRepository> logger)
        {
            _logger = logger;
            _filePath = configuration["DataFiles:TransactionsPath"] ?? "Data/transactions.json";
            _transactions = LoadTransactions();
        }

        private List<Transaction> LoadTransactions()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    return JsonConvert.DeserializeObject<List<Transaction>>(json) ?? new List<Transaction>();
                }
                _logger.LogWarning("Transactions file not found at {Path}", _filePath);
                return new List<Transaction>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transactions from JSON file");
                return new List<Transaction>();
            }
        }

        private async Task SaveTransactionsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                var json = JsonConvert.SerializeObject(_transactions, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return Task.FromResult(_transactions.AsEnumerable());
        }

        public Task<Transaction?> GetTransactionByIdAsync(string transactionId)
        {
            var transaction = _transactions.FirstOrDefault(t =>
                t.TransactionId.Equals(transactionId, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(transaction);
        }

        public Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(string accountId)
        {
            var transactions = _transactions.Where(t =>
                t.FromAccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase) ||
                t.ToAccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(transactions);
        }

        public Task<IEnumerable<Transaction>> GetTransactionsByStatusAsync(TransactionStatus status)
        {
            var transactions = _transactions.Where(t => t.Status == status);
            return Task.FromResult(transactions);
        }

        public Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = _transactions.Where(t =>
                t.TransactionDate >= startDate && t.TransactionDate <= endDate);
            return Task.FromResult(transactions);
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            transaction.TransactionId = $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
            transaction.ReferenceNumber = $"REF{DateTime.UtcNow:yyyyMMdd}{new Random().Next(100000, 999999)}";
            transaction.TransactionDate = DateTime.UtcNow;

            _transactions.Add(transaction);
            await SaveTransactionsAsync();

            _logger.LogInformation("Created transaction {TransactionId}", transaction.TransactionId);
            return transaction;
        }

        public async Task<bool> UpdateTransactionStatusAsync(
            string transactionId,
            TransactionStatus status,
            string? failureReason = null)
        {
            var transaction = _transactions.FirstOrDefault(t =>
                t.TransactionId.Equals(transactionId, StringComparison.OrdinalIgnoreCase));

            if (transaction == null) return false;

            transaction.Status = status;
            if (status == TransactionStatus.Completed)
            {
                transaction.CompletedDate = DateTime.UtcNow;
            }
            if (!string.IsNullOrEmpty(failureReason))
            {
                transaction.FailureReason = failureReason;
            }

            await SaveTransactionsAsync();
            return true;
        }
    }
}