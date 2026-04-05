using BankingAPI.Models.Enums;
using BankingAPI.Services.Interfaces;

namespace BankingAPI.Services.Factory
{
    /// <summary>
    /// Payment Factory Implementation
    /// 
    /// FACTORY DESIGN PATTERN EXPLANATION:
    /// =====================================
    /// The Factory Pattern is a creational design pattern that provides an interface
    /// for creating objects in a superclass, but allows subclasses to alter the type
    /// of objects that will be created.
    /// 
    /// Benefits:
    /// 1. Decoupling: Client code doesn't need to know concrete classes
    /// 2. Single Responsibility: Object creation logic is centralized
    /// 3. Open/Closed Principle: Easy to add new payment types without modifying existing code
    /// 4. Testability: Easy to mock and test
    /// 
    /// In this banking context:
    /// - Client (Controller) asks Factory for a payment service
    /// - Factory creates appropriate service (UPI, NEFT, etc.) based on payment type
    /// - Client uses the service through common interface (IPaymentService)
    /// </summary>
    public class PaymentFactory : IPaymentFactory
    {
        // Dictionary to store all registered payment services
        private readonly Dictionary<PaymentType, IPaymentService> _paymentServices;
        private readonly ILogger<PaymentFactory> _logger;

        public PaymentFactory(
            IEnumerable<IPaymentService> paymentServices,
            ILogger<PaymentFactory> logger)
        {
            _logger = logger;

            // Register all payment services by their type
            // This is done via Dependency Injection - all IPaymentService implementations
            // are injected here automatically
            _paymentServices = paymentServices.ToDictionary(s => s.PaymentType);

            _logger.LogInformation(
                "PaymentFactory initialized with {Count} payment services: {Types}",
                _paymentServices.Count,
                string.Join(", ", _paymentServices.Keys));
        }

        /// <summary>
        /// Creates and returns the appropriate payment service
        /// This is the core factory method
        /// </summary>
        public IPaymentService CreatePaymentService(PaymentType paymentType)
        {
            _logger.LogDebug("Creating payment service for type: {PaymentType}", paymentType);

            if (_paymentServices.TryGetValue(paymentType, out var service))
            {
                _logger.LogInformation(
                    "Payment service created: {ServiceType} for {PaymentType}",
                    service.GetType().Name, paymentType);
                return service;
            }

            _logger.LogError("Unsupported payment type requested: {PaymentType}", paymentType);
            throw new NotSupportedException(
                $"Payment type '{paymentType}' is not supported. " +
                $"Available types: {string.Join(", ", _paymentServices.Keys)}");
        }

        public IEnumerable<PaymentType> GetAvailablePaymentTypes()
        {
            return _paymentServices.Keys;
        }

        public bool IsPaymentTypeSupported(PaymentType paymentType)
        {
            return _paymentServices.ContainsKey(paymentType);
        }
    }
}