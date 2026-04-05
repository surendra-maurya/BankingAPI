using BankingAPI.Models;
using BankingAPI.Models.Enums;
using BankingAPI.Repositories.Interfaces;

namespace BankingAPI.Services.Implementations
{
    /// <summary>
    /// NEFT (National Electronic Funds Transfer) Payment Service
    /// Dedicated service for NEFT transfers
    /// </summary>
    public class NEFTPaymentService : BasePaymentService
    {
        private const decimal MAX_AMOUNT = 10000000m; // 1 Crore

        public override PaymentType PaymentType => PaymentType.NEFT;

        public NEFTPaymentService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            ILogger<NEFTPaymentService> logger)
            : base(accountRepository, transactionRepository, logger)
        {
        }

        public override async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            _logger.LogInformation("Processing NEFT payment of {Amount}", request.Amount);

            try
            {
                var validation = await ValidateRequestAsync(request);
                if (!validation.IsValid)
                {
                    return PaymentResponse.Failure(validation.ErrorMessage, PaymentType.NEFT);
                }

                var neftRequest = request as NEFTPaymentRequest;
                if (neftRequest == null)
                {
                    return PaymentResponse.Failure("Invalid NEFT request format", PaymentType.NEFT);
                }

                // Create transaction
                var transaction = CreateBaseTransaction(request, $"EXT_{neftRequest.BeneficiaryAccountNumber}");
                transaction.Metadata = new Dictionary<string, string>
                {
                    { "BeneficiaryAccountNumber", neftRequest.BeneficiaryAccountNumber },
                    { "BeneficiaryIFSC", neftRequest.BeneficiaryIFSC },
                    { "BeneficiaryName", neftRequest.BeneficiaryName },
                    { "BeneficiaryBank", neftRequest.BeneficiaryBankName },
                    { "PaymentMode", "NEFT" },
                    { "BatchId", GetNextBatchId() }
                };

                transaction = await _transactionRepository.CreateTransactionAsync(transaction);

                // Simulate NEFT batch processing
                await SimulateNEFTProcessing();

                var sourceAccount = await _accountRepository.GetAccountByIdAsync(request.FromAccountId);
                await _accountRepository.UpdateAccountBalanceAsync(
                    request.FromAccountId,
                    sourceAccount!.Balance - request.Amount);

                await _transactionRepository.UpdateTransactionStatusAsync(
                    transaction.TransactionId,
                    TransactionStatus.Completed);

                var response = PaymentResponse.Success(
                    transaction.TransactionId,
                    transaction.ReferenceNumber,
                    request.Amount,
                    PaymentType.NEFT,
                    $"NEFT transfer of ₹{request.Amount} initiated successfully");

                response.AdditionalInfo = new Dictionary<string, object>
                {
                    { "BeneficiaryName", neftRequest.BeneficiaryName },
                    { "ExpectedSettlement", GetNextNEFTSettlementTime() },
                    { "BatchId", transaction.Metadata["BatchId"] }
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing NEFT payment");
                return PaymentResponse.Failure($"NEFT transfer failed: {ex.Message}", PaymentType.NEFT);
            }
        }

        private string GetNextBatchId()
        {
            // NEFT runs in half-hourly batches
            var now = DateTime.Now;
            int batchNumber = (now.Hour * 2) + (now.Minute >= 30 ? 1 : 0);
            return $"NEFT{now:yyyyMMdd}B{batchNumber:D2}";
        }

        private string GetNextNEFTSettlementTime()
        {
            var now = DateTime.Now;
            // NEFT settles in next half-hourly batch
            var nextBatch = now.AddMinutes(30 - (now.Minute % 30));
            return nextBatch.ToString("hh:mm tt");
        }

        private async Task SimulateNEFTProcessing()
        {
            await Task.Delay(300);
        }

        public override async Task<(bool IsValid, string ErrorMessage)> ValidateRequestAsync(PaymentRequest request)
        {
            var commonValidation = await ValidateCommonAsync(request);
            if (!commonValidation.IsValid)
            {
                return commonValidation;
            }

            if (request is not NEFTPaymentRequest neftRequest)
            {
                return (false, "Invalid request type for NEFT");
            }

            if (string.IsNullOrWhiteSpace(neftRequest.BeneficiaryAccountNumber))
            {
                return (false, "Beneficiary account number is required");
            }

            if (string.IsNullOrWhiteSpace(neftRequest.BeneficiaryIFSC))
            {
                return (false, "IFSC code is required");
            }

            if (neftRequest.BeneficiaryIFSC.Length != 11)
            {
                return (false, "Invalid IFSC code format");
            }

            if (request.Amount > MAX_AMOUNT)
            {
                return (false, $"Amount exceeds NEFT maximum limit of ₹{MAX_AMOUNT:N0}");
            }

            return (true, string.Empty);
        }

        public override Dictionary<string, string> GetServiceInfo()
        {
            return new Dictionary<string, string>
            {
                { "ServiceName", "NEFT Payment Service" },
                { "PaymentType", "NEFT" },
                { "MaxAmount", $"₹{MAX_AMOUNT:N0}" },
                { "ProcessingTime", "30 minutes - 2 hours" },
                { "BatchFrequency", "Every 30 minutes" },
                { "Availability", "24x7 (since December 2019)" },
                { "Charges", "Usually free for savings accounts" }
            };
        }
    }
}