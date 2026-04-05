using BankingAPI.Models;
using BankingAPI.Models.Enums;
using BankingAPI.Repositories.Interfaces;

namespace BankingAPI.Services.Implementations
{
    /// <summary>
    /// UPI Payment Service Implementation
    /// 
    /// UPI (Unified Payments Interface) is India's real-time payment system
    /// Features: Instant transfers, 24/7 availability, UPI ID based transfers
    /// </summary>
    public class UPIPaymentService : BasePaymentService
    {
        private const decimal MAX_TRANSACTION_LIMIT = 100000m; // 1 Lakh per transaction
        private const decimal DAILY_LIMIT = 500000m; // 5 Lakhs per day

        public override PaymentType PaymentType => PaymentType.UPI;

        public UPIPaymentService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            ILogger<UPIPaymentService> logger)
            : base(accountRepository, transactionRepository, logger)
        {
        }

        public override async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            _logger.LogInformation("Processing UPI payment of {Amount} from {FromAccount}",
                request.Amount, request.FromAccountId);

            try
            {
                // Validate request
                var validation = await ValidateRequestAsync(request);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("UPI validation failed: {Error}", validation.ErrorMessage);
                    return PaymentResponse.Failure(validation.ErrorMessage, PaymentType.UPI);
                }

                var upiRequest = request as UPIPaymentRequest;
                if (upiRequest == null)
                {
                    return PaymentResponse.Failure("Invalid UPI request format", PaymentType.UPI);
                }

                // Find receiver account by UPI ID
                var receiverAccount = await _accountRepository.GetAccountByUPIIdAsync(upiRequest.ReceiverUPIId);
                if (receiverAccount == null)
                {
                    return PaymentResponse.Failure($"UPI ID '{upiRequest.ReceiverUPIId}' not found", PaymentType.UPI);
                }

                // Create transaction
                var transaction = CreateBaseTransaction(request, receiverAccount.AccountId);
                transaction.Metadata = new Dictionary<string, string>
                {
                    { "ReceiverUPIId", upiRequest.ReceiverUPIId },
                    { "ReceiverName", receiverAccount.AccountHolderName },
                    { "PaymentMode", "UPI" }
                };

                // Save initial transaction
                transaction = await _transactionRepository.CreateTransactionAsync(transaction);

                // Simulate processing
                await SimulateProcessingAsync();

                // Process the actual transfer
                var sourceAccount = await _accountRepository.GetAccountByIdAsync(request.FromAccountId);

                // Debit source account
                await _accountRepository.UpdateAccountBalanceAsync(
                    request.FromAccountId,
                    sourceAccount!.Balance - request.Amount);

                // Credit destination account
                await _accountRepository.UpdateAccountBalanceAsync(
                    receiverAccount.AccountId,
                    receiverAccount.Balance + request.Amount);

                // Update transaction status
                await _transactionRepository.UpdateTransactionStatusAsync(
                    transaction.TransactionId,
                    TransactionStatus.Completed);

                _logger.LogInformation(
                    "UPI payment successful. TransactionId: {TransactionId}, Amount: {Amount}",
                    transaction.TransactionId, request.Amount);

                var response = PaymentResponse.Success(
                    transaction.TransactionId,
                    transaction.ReferenceNumber,
                    request.Amount,
                    PaymentType.UPI,
                    $"Successfully transferred ₹{request.Amount} to {upiRequest.ReceiverUPIId}");

                response.AdditionalInfo = new Dictionary<string, object>
                {
                    { "ReceiverName", receiverAccount.AccountHolderName },
                    { "ReceiverUPIId", upiRequest.ReceiverUPIId },
                    { "ProcessingTime", "Instant" }
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing UPI payment");
                return PaymentResponse.Failure($"Payment failed: {ex.Message}", PaymentType.UPI);
            }
        }

        public override async Task<(bool IsValid, string ErrorMessage)> ValidateRequestAsync(PaymentRequest request)
        {
            // Common validation
            var commonValidation = await ValidateCommonAsync(request);
            if (!commonValidation.IsValid)
            {
                return commonValidation;
            }

            // UPI specific validation
            if (request is not UPIPaymentRequest upiRequest)
            {
                return (false, "Invalid request type for UPI payment");
            }

            if (string.IsNullOrWhiteSpace(upiRequest.ReceiverUPIId))
            {
                return (false, "Receiver UPI ID is required");
            }

            // Validate UPI ID format (basic check)
            if (!upiRequest.ReceiverUPIId.Contains('@'))
            {
                return (false, "Invalid UPI ID format. Expected format: username@bankname");
            }

            // Check transaction limit
            if (request.Amount > MAX_TRANSACTION_LIMIT)
            {
                return (false, $"Amount exceeds UPI transaction limit of ₹{MAX_TRANSACTION_LIMIT:N0}");
            }

            // Self-transfer check
            var sourceAccount = await _accountRepository.GetAccountByIdAsync(request.FromAccountId);
            if (sourceAccount?.UPIId.Equals(upiRequest.ReceiverUPIId, StringComparison.OrdinalIgnoreCase) == true)
            {
                return (false, "Cannot transfer to same account");
            }

            return (true, string.Empty);
        }

        public override Dictionary<string, string> GetServiceInfo()
        {
            return new Dictionary<string, string>
            {
                { "ServiceName", "UPI Payment Service" },
                { "PaymentType", "UPI" },
                { "MaxTransactionLimit", $"₹{MAX_TRANSACTION_LIMIT:N0}" },
                { "DailyLimit", $"₹{DAILY_LIMIT:N0}" },
                { "ProcessingTime", "Instant" },
                { "Availability", "24x7" },
                { "Features", "Instant transfer, UPI ID based, QR Code support" }
            };
        }
    }
}