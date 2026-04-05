using BankingAPI.Models.Enums;
using BankingAPI.Services.Interfaces;

namespace BankingAPI.Services.Factory
{
    /// <summary>
    /// Factory interface for creating payment services
    /// Factory Pattern: Provides an interface for creating objects without specifying concrete classes
    /// </summary>
    public interface IPaymentFactory
    {
        /// <summary>
        /// Creates appropriate payment service based on payment type
        /// </summary>
        /// <param name="paymentType">The type of payment</param>
        /// <returns>Payment service instance</returns>
        IPaymentService CreatePaymentService(PaymentType paymentType);

        /// <summary>
        /// Gets all available payment types
        /// </summary>
        IEnumerable<PaymentType> GetAvailablePaymentTypes();

        /// <summary>
        /// Checks if a payment type is supported
        /// </summary>
        bool IsPaymentTypeSupported(PaymentType paymentType);
    }
}