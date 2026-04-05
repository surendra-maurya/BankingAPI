using BankingAPI.Models;
using BankingAPI.Models.Enums;
using BankingAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankingAPI.Controllers
{
    /// <summary>
    /// Transaction Controller - Manages transaction history and details
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            ITransactionRepository transactionRepository,
            ILogger<TransactionController> logger)
        {
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all transactions
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Transaction>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTransactions()
        {
            var transactions = await _transactionRepository.GetAllTransactionsAsync();
            return Ok(ApiResponse<IEnumerable<Transaction>>.SuccessResponse(
                transactions,
                $"Retrieved {transactions.Count()} transactions"));
        }

        /// <summary>
        /// Get transaction by ID
        /// </summary>
        [HttpGet("{transactionId}")]
        [ProducesResponseType(typeof(ApiResponse<Transaction>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransactionById(string transactionId)
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId);

            if (transaction == null)
            {
                return NotFound(ApiResponse<Transaction>.ErrorResponse(
                    $"Transaction with ID '{transactionId}' not found"));
            }

            return Ok(ApiResponse<Transaction>.SuccessResponse(transaction));
        }

        /// <summary>
        /// Get transactions for an account
        /// </summary>
        [HttpGet("account/{accountId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Transaction>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactionsByAccount(string accountId)
        {
            var transactions = await _transactionRepository.GetTransactionsByAccountIdAsync(accountId);
            return Ok(ApiResponse<IEnumerable<Transaction>>.SuccessResponse(
                transactions,
                $"Retrieved {transactions.Count()} transactions for account {accountId}"));
        }

        /// <summary>
        /// Get transactions by status
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Transaction>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactionsByStatus(TransactionStatus status)
        {
            var transactions = await _transactionRepository.GetTransactionsByStatusAsync(status);
            return Ok(ApiResponse<IEnumerable<Transaction>>.SuccessResponse(
                transactions,
                $"Retrieved transactions with status '{status}'"));
        }

        /// <summary>
        /// Get transactions by date range
        /// </summary>
        [HttpGet("date-range")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Transaction>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactionsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var transactions = await _transactionRepository.GetTransactionsByDateRangeAsync(startDate, endDate);
            return Ok(ApiResponse<IEnumerable<Transaction>>.SuccessResponse(
                transactions,
                $"Retrieved transactions from {startDate:d} to {endDate:d}"));
        }

        /// <summary>
        /// Get transaction summary for an account
        /// </summary>
        [HttpGet("account/{accountId}/summary")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactionSummary(string accountId)
        {
            var transactions = await _transactionRepository.GetTransactionsByAccountIdAsync(accountId);
            var transactionList = transactions.ToList();

            var summary = new
            {
                AccountId = accountId,
                TotalTransactions = transactionList.Count,
                TotalDebits = transactionList.Where(t => t.FromAccountId == accountId).Sum(t => t.Amount),
                TotalCredits = transactionList.Where(t => t.ToAccountId == accountId).Sum(t => t.Amount),
                SuccessfulTransactions = transactionList.Count(t => t.Status == TransactionStatus.Completed),
                FailedTransactions = transactionList.Count(t => t.Status == TransactionStatus.Failed),
                PendingTransactions = transactionList.Count(t => t.Status == TransactionStatus.Pending),
                TransactionsByType = transactionList
                    .GroupBy(t => t.PaymentType)
                    .Select(g => new { PaymentType = g.Key.ToString(), Count = g.Count(), TotalAmount = g.Sum(t => t.Amount) })
            };

            return Ok(ApiResponse<object>.SuccessResponse(summary, "Transaction summary generated"));
        }
    }
}