using BankingAPI.Models;
using BankingAPI.Models.Enums;
using BankingAPI.Repositories.Interfaces;

namespace BankingAPI.Services.Implementations
{
    /// <summary>
    /// Credit Card Payment Service Implementation
    /// Used for merchant payments, bill payments, etc.
    /// </summary>
    public class CreditCardPaymentService : BasePaymentService
    {
        private const decimal MAX_SINGLE_TRANSACTION = 500000m;
        private const decimal PROCESSING_FEE_PERCENTAGE = 2.5m;

        public override PaymentType PaymentType => PaymentType.CreditCard;

        public CreditCardPaymentService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            ILogger<CreditCardPaymentService> logger)
            : base(accountRepository, transactionRepository, logger)
        {
        }

        public override async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            _logger.LogInformation(
                "Processing Credit Card payment of {Amount} from {FromAccount}",
                request.Amount, request.FromAccountId);

            try
            {
                var validation = await ValidateRequestAsync(request);
                if (!validation.IsValid)
                {
                    return PaymentResponse.Failure(validation.ErrorMessage, PaymentType.CreditCard);
                }

                var ccRequest = request as CreditCardPaymentRequest;
                if (ccRequest == null)
                {
                    return PaymentResponse.Failure("Invalid Credit Card request format", PaymentType.CreditCard);
                }

                // Create transaction
                var transaction = CreateBaseTransaction(request, ccRequest.MerchantId);
                transaction.Metadata = new Dictionary<string, string>
                {
                    { "CardNumber", MaskCardNumber(ccRequest.CardNumber) },
                    { "CardHolderName", ccRequest.CardHolderName },
                    { "MerchantId", ccRequest.MerchantId },
                    { "PaymentMode", "CreditCard" }
                };

                transaction = await _transactionRepository.CreateTransactionAsync(transaction);

                // Simulate card processing
                await SimulateCardProcessing();

                // Calculate processing fee
                decimal processingFee = request.Amount * (PROCESSING_FEE_PERCENTAGE / 100);
                decimal totalAmount = request.Amount + processingFee;

                // In real scenario, this would hit card network (Visa/Master)
                // For demo, we debit from linked account
                var sourceAccount = await _accountRepository.GetAccountByIdAsync(request.FromAccountId);

                await _accountRepository.UpdateAccountBalanceAsync(
                    request.FromAccountId,
                    sourceAccount!.Balance - totalAmount);

                await _transactionRepository.UpdateTransactionStatusAsync(
                    transaction.TransactionId,
                    TransactionStatus.Completed);

                var response = PaymentResponse.Success(
                    transaction.TransactionId,
                    transaction.ReferenceNumber,
                    request.Amount,
                    PaymentType.CreditCard,
                    $"Credit Card payment of ₹{request.Amount} successful");

                response.AdditionalInfo = new Dictionary<string, object>
                {
                    { "CardNumber", MaskCardNumber(ccRequest.CardNumber) },
                    { "ProcessingFee", processingFee },
                    { "TotalDebited", totalAmount },
                    { "AuthorizationCode", GenerateAuthCode() }
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Credit Card payment");
                return PaymentResponse.Failure($"Payment failed: {ex.Message}", PaymentType.CreditCard);
            }
        }

        private async Task SimulateCardProcessing()
        {
            // Simulate card network authorization
            await Task.Delay(Random.Shared.Next(200, 800));
        }

        private string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return "****";
            return $"****-****-****-{cardNumber[^4..]}";
        }

        private string GenerateAuthCode()
        {
            return $"AUTH{Random.Shared.Next(100000, 999999)}";
        }

        public override async Task<(bool IsValid, string ErrorMessage)> ValidateRequestAsync(PaymentRequest request)
        {
            var commonValidation = await ValidateCommonAsync(request);
            if (!commonValidation.IsValid)
            {
                return commonValidation;
            }

            if (request is not CreditCardPaymentRequest ccRequest)
            {
                return (false, "Invalid request type for Credit Card payment");
            }

            if (string.IsNullOrWhiteSpace(ccRequest.CardNumber))
            {
                return (false, "Card number is required");
            }

            // Basic Luhn validation could be added here
            if (ccRequest.CardNumber.Length < 13 || ccRequest.CardNumber.Length > 19)
            {
                return (false, "Invalid card number length");
            }

            if (string.IsNullOrWhiteSpace(ccRequest.CVV) || ccRequest.CVV.Length != 3)
            {
                return (false, "Invalid CVV");
            }

            if (request.Amount > MAX_SINGLE_TRANSACTION)
            {
                return (false, $"Amount exceeds maximum limit of ₹{MAX_SINGLE_TRANSACTION:N0}");
            }

            // Validate expiry
            if (!ValidateExpiry(ccRequest.ExpiryMonth, ccRequest.ExpiryYear))
            {
                return (false, "Card has expired or invalid expiry date");
            }

            return (true, string.Empty);
        }

        private bool ValidateExpiry(string month, string year)
        {
            if (!int.TryParse(month, out int m) || !int.TryParse(year, out int y))
                return false;

            if (m < 1 || m > 12) return false;

            // Convert 2-digit year to 4-digit
            if (y < 100) y += 2000;

            var expiryDate = new DateTime(y, m, DateTime.DaysInMonth(y, m));
            return expiryDate >= DateTime.Now;
        }

        public override Dictionary<string, string> GetServiceInfo()
        {
            return new Dictionary<string, string>
            {
                { "ServiceName", "Credit Card Payment Service" },
                { "PaymentType", "CreditCard" },
                { "MaxTransactionLimit", $"₹{MAX_SINGLE_TRANSACTION:N0}" },
                { "ProcessingFee", $"{PROCESSING_FEE_PERCENTAGE}%" },
                { "SupportedNetworks", "Visa, MasterCard, RuPay" },
                { "ProcessingTime", "Instant" },
                { "Features", "EMI options, Rewards points, Secure 3D authentication" }
            };
        }
    }
}