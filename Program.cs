using BankingAPI.Middleware;
using BankingAPI.Repositories.Implementations;
using BankingAPI.Repositories.Interfaces;
using BankingAPI.Services.Factory;
using BankingAPI.Services.Implementations;
using BankingAPI.Services.Interfaces;
using Microsoft.OpenApi;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CONFIGURE SERVICES (Dependency Injection)
// ========================================

// Add Controllers
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Banking API",
        Version = "v1",
        Description = "A comprehensive Banking API demonstrating Factory Design Pattern with UPI, Internet Banking, NEFT, and Credit Card payments",
        Contact = new OpenApiContact
        {
            Name = "Banking Team",
            Email = "support@bankingapi.com"
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ========================================
// REGISTER REPOSITORIES (Singleton for JSON file-based)
// ========================================
builder.Services.AddSingleton<IAccountRepository, JsonAccountRepository>();
builder.Services.AddSingleton<ITransactionRepository, JsonTransactionRepository>();

// ========================================
// REGISTER PAYMENT SERVICES
// This is crucial for Factory Pattern - all services implementing IPaymentService
// will be injected into the factory
// ========================================
builder.Services.AddScoped<IPaymentService, UPIPaymentService>();
builder.Services.AddScoped<IPaymentService, InternetBankingPaymentService>();
builder.Services.AddScoped<IPaymentService, CreditCardPaymentService>();
builder.Services.AddScoped<IPaymentService, NEFTPaymentService>();

// ========================================
// REGISTER FACTORY
// The factory receives all IPaymentService implementations via DI
// ========================================
builder.Services.AddScoped<IPaymentFactory, PaymentFactory>();

// Add Health Checks
builder.Services.AddHealthChecks();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ========================================
// CONFIGURE HTTP PIPELINE
// ========================================

// Enable Swagger in all environments for demo
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking API v1");
    options.RoutePrefix = string.Empty; // Serve Swagger at root
});

// Custom Exception Handling Middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Banking API started successfully");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Swagger UI available at: /");

app.Run();