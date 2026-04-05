using BankingAPI.Models;
using BankingAPI.Models.Enums;
using BankingAPI.Repositories.Interfaces;
using BankingAPI.Services.Interfaces;

namespace BankingAPI.Services.Implementations
{
    /// <summary>
    /// Base class for all payment services with common functionality
    /// Template Method Pattern combined with Factory Pattern
    /// </summary>
    public abstract class BasePaymentService : IPaymentService
    {
        protected readonly IAccountRepository _accountRepository;
        protected readonly ITransactionRepository _transactionRepository;
        protected readonly ILogger _logger;

        public abstract PaymentType PaymentType { get; }

        protected BasePaymentService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            ILogger logger)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        public abstract Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
        public abstract Task<(bool IsValid, string ErrorMessage)> ValidateRequestAsync(PaymentRequest request);
        public abstract Dictionary<string, string> GetServiceInfo();

        /// <summary>
        /// Common validation logic for all payment types
        /// </summary>
        protected async Task<(bool IsValid, string ErrorMessage)> ValidateCommonAsync(PaymentRequest request)
        {
            // Check if source account exists
            var sourceAccount = await _accountRepository.GetAccountByIdAsync(request.FromAccountId);
            if (sourceAccount == null)
            {
                return (false, "Source account not found");
            }

            // Check if account is active
            if (!sourceAccount.IsActive)
            {
                return (false, "Source account is inactive");
            }

            // Check balance
            if (sourceAccount.Balance < request.Amount)
            {
                return (false, $"Insufficient balance. Available: {sourceAccount.Balance:C}, Required: {request.Amount:C}");
            }

            // Check minimum amount
            if (request.Amount <= 0)
            {
                return (false, "Amount must be greater than zero");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Common transaction creation logic
        /// </summary>
        protected Transaction CreateBaseTransaction(PaymentRequest request, string toAccountId)
        {
            return new Transaction
            {
                FromAccountId = request.FromAccountId,
                ToAccountId = toAccountId,
                Amount = request.Amount,
                PaymentType = PaymentType,
                Status = TransactionStatus.Processing,
                Description = request.Description ?? $"{PaymentType} Payment",
                TransactionDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Simulates payment processing delay (for demo purposes)
        /// </summary>
        protected async Task SimulateProcessingAsync()
        {
            // Simulate network/processing delay
            await Task.Delay(Random.Shared.Next(100, 500));
        }

        /// <summary>
        /// Generates a unique reference number
        /// </summary>
        protected string GenerateReferenceNumber()
        {
            return $"{PaymentType.ToString().ToUpper()[..3]}{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        }
    }
}