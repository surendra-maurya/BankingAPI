using BankingAPI.Models;
using BankingAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankingAPI.Controllers
{
    /// <summary>
    /// Account Controller - Manages bank account operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountRepository accountRepository, ILogger<AccountController> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all accounts
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Account>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAccounts()
        {
            var accounts = await _accountRepository.GetAllAccountsAsync();
            return Ok(ApiResponse<IEnumerable<Account>>.SuccessResponse(accounts, "Accounts retrieved successfully"));
        }

        /// <summary>
        /// Get account by ID
        /// </summary>
        [HttpGet("{accountId}")]
        [ProducesResponseType(typeof(ApiResponse<Account>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccountById(string accountId)
        {
            var account = await _accountRepository.GetAccountByIdAsync(accountId);

            if (account == null)
            {
                return NotFound(ApiResponse<Account>.ErrorResponse($"Account with ID '{accountId}' not found"));
            }

            return Ok(ApiResponse<Account>.SuccessResponse(account));
        }

        /// <summary>
        /// Get account balance
        /// </summary>
        [HttpGet("{accountId}/balance")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBalance(string accountId)
        {
            var account = await _accountRepository.GetAccountByIdAsync(accountId);

            if (account == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Account with ID '{accountId}' not found"));
            }

            var balanceInfo = new
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                AccountHolderName = account.AccountHolderName,
                Balance = account.Balance,
                Currency = "INR",
                LastUpdated = account.LastTransactionDate ?? account.CreatedDate
            };

            return Ok(ApiResponse<object>.SuccessResponse(balanceInfo, "Balance retrieved successfully"));
        }

        /// <summary>
        /// Get account by UPI ID
        /// </summary>
        [HttpGet("upi/{upiId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccountByUPI(string upiId)
        {
            var account = await _accountRepository.GetAccountByUPIIdAsync(upiId);

            if (account == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Account with UPI ID '{upiId}' not found"));
            }

            // Return limited info for UPI lookup (privacy)
            var upiInfo = new
            {
                Name = account.AccountHolderName,
                UPIId = account.UPIId,
                BankName = account.IFSC[..4], // First 4 chars indicate bank
                IsVerified = account.IsActive
            };

            return Ok(ApiResponse<object>.SuccessResponse(upiInfo, "UPI ID verified"));
        }
    }
}