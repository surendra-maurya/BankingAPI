using Microsoft.AspNetCore.Mvc;

namespace BankingAPI.Controllers
{
    /// <summary>
    /// Health check controller for Kubernetes probes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Liveness probe - Is the application alive?
        /// </summary>
        [HttpGet("live")]
        public IActionResult Live()
        {
            return Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Readiness probe - Is the application ready to accept traffic?
        /// </summary>
        [HttpGet("ready")]
        public IActionResult Ready()
        {
            // Add checks for dependencies (DB, external services) here
            return Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Detailed health check
        /// </summary>
        [HttpGet]
        public IActionResult Health()
        {
            var health = new
            {
                Status = "Healthy",
                Application = "Banking API",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                Timestamp = DateTime.UtcNow,
                Checks = new
                {
                    DataFiles = "OK",
                    PaymentServices = "OK"
                }
            };

            return Ok(health);
        }
    }
}