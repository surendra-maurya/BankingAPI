using BankingAPI.Models;
using BankingAPI.Models.Enums;
using BankingAPI.Services.Factory;
using Microsoft.AspNetCore.Mvc;

namespace BankingAPI.Controllers
{
    /// <summary>
    /// Payment Controller - Main entry point for all payment operations
    /// Demonstrates Factory Pattern usage
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentFactory _paymentFactory;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentFactory paymentFactory, ILogger<PaymentController> logger)
        {
            _paymentFactory = paymentFactory;
            _logger = logger;
        }

        /// <summary>
        /// Get all available payment types
        /// </summary>
        [HttpGet("payment-types")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), StatusCodes.Status200OK)]
        public IActionResult GetPaymentTypes()
        {
            var paymentTypes = _paymentFactory.GetAvailablePaymentTypes()
                .Select(pt => new
                {
                    Type = pt,
                    TypeId = (int)pt,
                    Name = pt.ToString(),
                    Info = _paymentFactory.CreatePaymentService(pt).GetServiceInfo()
                });

            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(
                paymentTypes,
                "Available payment types retrieved"));
        }

        /// <summary>
        /// Process a UPI payment
        /// </summary>
        [HttpPost("upi")]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessUPIPayment([FromBody] UPIPaymentRequest request)
        {
            _logger.LogInformation("UPI payment request received for amount: {Amount}", request.Amount);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<PaymentResponse>.ErrorResponse(
                    "Invalid request",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            request.PaymentType = PaymentType.UPI;

            // Factory Pattern in action - getting UPI service
            var paymentService = _paymentFactory.CreatePaymentService(PaymentType.UPI);
            var result = await paymentService.ProcessPaymentAsync(request);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<PaymentResponse>.SuccessResponse(result, "UPI payment successful"));
            }

            return BadRequest(ApiResponse<PaymentResponse>.ErrorResponse(result.Message));
        }

        /// <summary>
        /// Process an Internet Banking payment (NEFT/RTGS/IMPS)
        /// </summary>
        [HttpPost("internet-banking")]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessInternetBankingPayment([FromBody] InternetBankingRequest request)
        {
            _logger.LogInformation(
                "Internet Banking payment request received. Amount: {Amount}, Type: {TransferType}",
                request.Amount, request.TransferType);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<PaymentResponse>.ErrorResponse(
                    "Invalid request",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            request.PaymentType = PaymentType.InternetBanking;

            var paymentService = _paymentFactory.CreatePaymentService(PaymentType.InternetBanking);
            var result = await paymentService.ProcessPaymentAsync(request);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<PaymentResponse>.SuccessResponse(result, "Transfer initiated successfully"));
            }

            return BadRequest(ApiResponse<PaymentResponse>.ErrorResponse(result.Message));
        }

        /// <summary>
        /// Process a Credit Card payment
        /// </summary>
        [HttpPost("credit-card")]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessCreditCardPayment([FromBody] CreditCardPaymentRequest request)
        {
            _logger.LogInformation("Credit Card payment request received for amount: {Amount}", request.Amount);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<PaymentResponse>.ErrorResponse(
                    "Invalid request",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            request.PaymentType = PaymentType.CreditCard;

            var paymentService = _paymentFactory.CreatePaymentService(PaymentType.CreditCard);
            var result = await paymentService.ProcessPaymentAsync(request);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<PaymentResponse>.SuccessResponse(result, "Credit Card payment successful"));
            }

            return BadRequest(ApiResponse<PaymentResponse>.ErrorResponse(result.Message));
        }

        /// <summary>
        /// Process a NEFT payment
        /// </summary>
        [HttpPost("neft")]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessNEFTPayment([FromBody] NEFTPaymentRequest request)
        {
            _logger.LogInformation("NEFT payment request received for amount: {Amount}", request.Amount);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<PaymentResponse>.ErrorResponse(
                    "Invalid request",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            request.PaymentType = PaymentType.NEFT;

            var paymentService = _paymentFactory.CreatePaymentService(PaymentType.NEFT);
            var result = await paymentService.ProcessPaymentAsync(request);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<PaymentResponse>.SuccessResponse(result, "NEFT transfer initiated"));
            }

            return BadRequest(ApiResponse<PaymentResponse>.ErrorResponse(result.Message));
        }

        /// <summary>
        /// Generic payment endpoint - Factory determines the service
        /// </summary>
        [HttpPost("process")]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            _logger.LogInformation(
                "Generic payment request received. Type: {PaymentType}, Amount: {Amount}",
                request.PaymentType, request.Amount);

            if (!_paymentFactory.IsPaymentTypeSupported(request.PaymentType))
            {
                return BadRequest(ApiResponse<PaymentResponse>.ErrorResponse(
                    $"Payment type '{request.PaymentType}' is not supported"));
            }

            var paymentService = _paymentFactory.CreatePaymentService(request.PaymentType);
            var result = await paymentService.ProcessPaymentAsync(request);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<PaymentResponse>.SuccessResponse(result));
            }

            return BadRequest(ApiResponse<PaymentResponse>.ErrorResponse(result.Message));
        }

        /// <summary>
        /// Get service information for a payment type
        /// </summary>
        [HttpGet("service-info/{paymentType}")]
        [ProducesResponseType(typeof(ApiResponse<Dictionary<string, string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetServiceInfo(PaymentType paymentType)
        {
            if (!_paymentFactory.IsPaymentTypeSupported(paymentType))
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    $"Payment type '{paymentType}' is not supported"));
            }

            var service = _paymentFactory.CreatePaymentService(paymentType);
            var info = service.GetServiceInfo();

            return Ok(ApiResponse<Dictionary<string, string>>.SuccessResponse(info));
        }
    }
}