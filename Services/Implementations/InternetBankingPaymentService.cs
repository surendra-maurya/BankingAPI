using BankingAPI.Models;
using BankingAPI.Models.Enums;
using BankingAPI.Repositories.Interfaces;

namespace BankingAPI.Services.Implementations
{
    /// <summary>
    /// Internet Banking (NEFT/RTGS/IMPS) Payment Service Implementation
    /// 
    /// Features:
    /// - NEFT: Batch processing, settled in batches
    /// - RTGS: Real-time for large amounts (>2 Lakhs)
    /// - IMPS: Instant, 24/7, lower limits
    /// </summary>
    public class InternetBankingPaymentService : BasePaymentService
    {
        private const decimal NEFT_MIN_AMOUNT = 1m;
        private const decimal RTGS_MIN_AMOUNT = 200000m; // 2 Lakhs minimum for RTGS
        private const decimal IMPS_MAX_AMOUNT = 500000m; // 5 Lakhs max for IMPS

        public override PaymentType PaymentType => PaymentType.InternetBanking;

        public InternetBankingPaymentService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            ILogger<InternetBankingPaymentService> logger)
            : base(accountRepository, transactionRepository, logger)
        {
        }

        public override async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            _logger.LogInformation(
                "Processing Internet Banking payment of {Amount} from {FromAccount}",
                request.Amount, request.FromAccountId);

            try
            {
                // Validate
                var validation = await ValidateRequestAsync(request);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Internet Banking validation failed: {Error}", validation.ErrorMessage);
                    return PaymentResponse.Failure(validation.ErrorMessage, PaymentType.InternetBanking);
                }

                var ibRequest = request as InternetBankingRequest;
                if (ibRequest == null)
                {
                    return PaymentResponse.Failure("Invalid Internet Banking request format", PaymentType.InternetBanking);
                }

                // Find or simulate beneficiary account
                var beneficiaryAccount = await _accountRepository.GetAccountByNumberAsync(ibRequest.BeneficiaryAccountNumber);
                string beneficiaryId = beneficiaryAccount?.AccountId ?? $"EXT_{ibRequest.BeneficiaryAccountNumber}";

                // Determine transfer type
                string transferType = DetermineTransferType(request.Amount, ibRequest.TransferType);

                // Create transaction
                var transaction = CreateBaseTransaction(request, beneficiaryId);
                transaction.Metadata = new Dictionary<string, string>
                {
                    { "BeneficiaryAccountNumber", ibRequest.BeneficiaryAccountNumber },
                    { "BeneficiaryIFSC", ibRequest.BeneficiaryIFSC },
                    { "BeneficiaryName", ibRequest.BeneficiaryName },
                    { "TransferType", transferType },
                    { "PaymentMode", "InternetBanking" }
                };

                // Save transaction
                transaction = await _transactionRepository.CreateTransactionAsync(transaction);

                // Simulate processing based on transfer type
                await SimulateTransferProcessing(transferType);

                // Process transfer
                var sourceAccount = await _accountRepository.GetAccountByIdAsync(request.FromAccountId);

                // Debit source
                await _accountRepository.UpdateAccountBalanceAsync(
                    request.FromAccountId,
                    sourceAccount!.Balance - request.Amount);

                // Credit destination (if internal)
                if (beneficiaryAccount != null)
                {
                    await _accountRepository.UpdateAccountBalanceAsync(
                        beneficiaryAccount.AccountId,
                        beneficiaryAccount.Balance + request.Amount);
                }

                // Update transaction
                await _transactionRepository.UpdateTransactionStatusAsync(
                    transaction.TransactionId,
                    TransactionStatus.Completed);

                var processingTime = GetProcessingTime(transferType);
                var response = PaymentResponse.Success(
                    transaction.TransactionId,
                    transaction.ReferenceNumber,
                    request.Amount,
                    PaymentType.InternetBanking,
                    $"Successfully initiated {transferType} transfer of ₹{request.Amount} to {ibRequest.BeneficiaryName}");

                response.AdditionalInfo = new Dictionary<string, object>
                {
                    { "TransferType", transferType },
                    { "BeneficiaryName", ibRequest.BeneficiaryName },
                    { "BeneficiaryAccount", MaskAccountNumber(ibRequest.BeneficiaryAccountNumber) },
                    { "ExpectedProcessingTime", processingTime }
                };

                _logger.LogInformation(
                    "Internet Banking {TransferType} payment successful. TransactionId: {TransactionId}",
                    transferType, transaction.TransactionId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Internet Banking payment");
                return PaymentResponse.Failure($"Payment failed: {ex.Message}", PaymentType.InternetBanking);
            }
        }

        private string DetermineTransferType(decimal amount, string requestedType)
        {
            // Auto-select based on amount if not specified correctly
            if (amount >= RTGS_MIN_AMOUNT)
            {
                return "RTGS";
            }
            else if (requestedType.Equals("IMPS", StringComparison.OrdinalIgnoreCase) && amount <= IMPS_MAX_AMOUNT)
            {
                return "IMPS";
            }
            return "NEFT";
        }

        private async Task SimulateTransferProcessing(string transferType)
        {
            // Simulate different processing times
            var delay = transferType switch
            {
                "IMPS" => 200,  // Nearly instant
                "RTGS" => 300,  // Real-time but slightly longer
                "NEFT" => 500,  // Batch processing
                _ => 400
            };
            await Task.Delay(delay);
        }

        private string GetProcessingTime(string transferType)
        {
            return transferType switch
            {
                "IMPS" => "Instant (within seconds)",
                "RTGS" => "Within 30 minutes",
                "NEFT" => "Within 2 hours (batch processing)",
                _ => "1-2 business days"
            };
        }

        private string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 4)
                return "****";
            return $"****{accountNumber[^4..]}";
        }

        public override async Task<(bool IsValid, string ErrorMessage)> ValidateRequestAsync(PaymentRequest request)
        {
            var commonValidation = await ValidateCommonAsync(request);
            if (!commonValidation.IsValid)
            {
                return commonValidation;
            }

            if (request is not InternetBankingRequest ibRequest)
            {
                return (false, "Invalid request type for Internet Banking");
            }

            if (string.IsNullOrWhiteSpace(ibRequest.BeneficiaryAccountNumber))
            {
                return (false, "Beneficiary account number is required");
            }

            if (string.IsNullOrWhiteSpace(ibRequest.BeneficiaryIFSC))
            {
                return (false, "Beneficiary IFSC code is required");
            }

            // Basic IFSC validation (11 characters, first 4 letters, 5th is 0)
            if (ibRequest.BeneficiaryIFSC.Length != 11)
            {
                return (false, "Invalid IFSC code format");
            }

            if (string.IsNullOrWhiteSpace(ibRequest.BeneficiaryName))
            {
                return (false, "Beneficiary name is required");
            }

            // Validate amount for RTGS
            if (ibRequest.TransferType.Equals("RTGS", StringComparison.OrdinalIgnoreCase)
                && request.Amount < RTGS_MIN_AMOUNT)
            {
                return (false, $"Minimum amount for RTGS is ₹{RTGS_MIN_AMOUNT:N0}");
            }

            return (true, string.Empty);
        }

        public override Dictionary<string, string> GetServiceInfo()
        {
            return new Dictionary<string, string>
            {
                { "ServiceName", "Internet Banking Service" },
                { "PaymentType", "InternetBanking" },
                { "SupportedModes", "NEFT, RTGS, IMPS" },
                { "NEFTProcessingTime", "2-4 hours (batch)" },
                { "RTGSMinAmount", $"₹{RTGS_MIN_AMOUNT:N0}" },
                { "RTGSProcessingTime", "30 minutes" },
                { "IMPSMaxAmount", $"₹{IMPS_MAX_AMOUNT:N0}" },
                { "IMPSProcessingTime", "Instant" },
                { "Availability", "NEFT/RTGS: Banking hours, IMPS: 24x7" }
            };
        }
    }
}