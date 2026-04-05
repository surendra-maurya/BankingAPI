using BankingAPI.Models;
using BankingAPI.Models.Enums;

namespace BankingAPI.Services.Interfaces
{
    /// <summary>
    /// Interface for payment services - All payment types must implement this
    /// This is the core of Factory Pattern - defining a common contract
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Gets the payment type this service handles
        /// </summary>
        PaymentType PaymentType { get; }

        /// <summary>
        /// Process a payment request
        /// </summary>
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);

        /// <summary>
        /// Validate the payment request before processing
        /// </summary>
        Task<(bool IsValid, string ErrorMessage)> ValidateRequestAsync(PaymentRequest request);

        /// <summary>
        /// Get service specific information
        /// </summary>
        Dictionary<string, string> GetServiceInfo();
    }
}