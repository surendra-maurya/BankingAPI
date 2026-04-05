using BankingAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace BankingAPI.Models
{
    /// <summary>
    /// Base payment request model
    /// </summary>
    public class PaymentRequest
    {
        [Required]
        public PaymentType PaymentType { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string FromAccountId { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    /// <summary>
    /// UPI specific payment request
    /// </summary>
    public class UPIPaymentRequest : PaymentRequest
    {
        [Required]
        [RegularExpression(@"^[\w.-]+@[\w]+$", ErrorMessage = "Invalid UPI ID format")]
        public string ReceiverUPIId { get; set; } = string.Empty;

        public string? UPIPin { get; set; } // In real world, this would be handled securely
    }

    /// <summary>
    /// Internet Banking payment request
    /// </summary>
    public class InternetBankingRequest : PaymentRequest
    {
        [Required]
        public string BeneficiaryAccountNumber { get; set; } = string.Empty;

        [Required]
        public string BeneficiaryIFSC { get; set; } = string.Empty;

        [Required]
        public string BeneficiaryName { get; set; } = string.Empty;

        public string TransferType { get; set; } = "NEFT"; // NEFT, RTGS, IMPS
    }

    /// <summary>
    /// Credit Card payment request
    /// </summary>
    public class CreditCardPaymentRequest : PaymentRequest
    {
        [Required]
        [CreditCard]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        public string CardHolderName { get; set; } = string.Empty;

        [Required]
        public string ExpiryMonth { get; set; } = string.Empty;

        [Required]
        public string ExpiryYear { get; set; } = string.Empty;

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string CVV { get; set; } = string.Empty;

        public string MerchantId { get; set; } = string.Empty;
    }

    /// <summary>
    /// NEFT payment request
    /// </summary>
    public class NEFTPaymentRequest : PaymentRequest
    {
        [Required]
        public string BeneficiaryAccountNumber { get; set; } = string.Empty;

        [Required]
        public string BeneficiaryIFSC { get; set; } = string.Empty;

        [Required]
        public string BeneficiaryName { get; set; } = string.Empty;

        public string BeneficiaryBankName { get; set; } = string.Empty;
    }
}