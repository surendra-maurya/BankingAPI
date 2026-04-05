using BankingAPI.Models;
using System.Net;
using System.Text.Json;

namespace BankingAPI.Middleware
{
    /// <summary>
    /// Global exception handling middleware
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>();

            switch (exception)
            {
                case NotSupportedException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResponse(
                        exception.Message,
                        new List<string> { "Unsupported operation" });
                    break;

                case ArgumentException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResponse(
                        exception.Message,
                        new List<string> { "Invalid argument provided" });
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = ApiResponse<object>.ErrorResponse(
                        exception.Message,
                        new List<string> { "Resource not found" });
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response = ApiResponse<object>.ErrorResponse(
                        "An internal error occurred. Please try again later.",
                        new List<string> { exception.Message });
                    break;
            }

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}