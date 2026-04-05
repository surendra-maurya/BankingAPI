using BankingAPI.Models.Enums;

namespace BankingAPI.Models
{
    /// <summary>
    /// Response model for payment operations
    /// </summary>
    public class PaymentResponse
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }
        public DateTime TransactionDate { get; set; }
        public Dictionary<string, object> AdditionalInfo { get; set; } = new();

        public static PaymentResponse Success(
            string transactionId,
            string referenceNumber,
            decimal amount,
            PaymentType paymentType,
            string message = "Payment successful")
        {
            return new PaymentResponse
            {
                IsSuccess = true,
                TransactionId = transactionId,
                ReferenceNumber = referenceNumber,
                Status = TransactionStatus.Completed,
                Message = message,
                Amount = amount,
                PaymentType = paymentType,
                TransactionDate = DateTime.UtcNow
            };
        }

        public static PaymentResponse Failure(string message, PaymentType paymentType)
        {
            return new PaymentResponse
            {
                IsSuccess = false,
                Status = TransactionStatus.Failed,
                Message = message,
                PaymentType = paymentType,
                TransactionDate = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Generic API response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}