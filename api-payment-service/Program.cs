    using api_payment_service.Data;
    using api_payment_service.Repositories;
    using api_payment_service.Services;
    using Microsoft.EntityFrameworkCore;
    using Serilog;

    // ==================== CONFIGURATION ====================
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();

    // ==================== LOGGING ====================
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.File("logs/payment-service-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();

    builder.Host.UseSerilog();

    // ==================== DEPENDENCY INJECTION ====================
    builder.Services.AddDbContext<PaymentDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

    builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
    builder.Services.AddScoped<IPaymentService, PaymentServiceImpl>();

    // ==================== API & CONTROLLERS ====================
    builder.Services.AddControllers();

    // ==================== SWAGGER / OPENAPI ====================
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Payment Service API",
            Version = "v1",
            Description = "Microservice for processing payments and managing transactions",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Your Company",
                Email = "support@example.com"
            }
        });
    });

    // ==================== CORS ====================
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policyBuilder =>
        {
            policyBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    // ==================== HEALTH CHECKS ====================
    builder.Services.AddHealthChecks();

    // ==================== BUILD APP ====================
    var app = builder.Build();

    // ==================== MIDDLEWARE - LOGGING ====================
    app.UseSerilogRequestLogging();

    // ==================== MIDDLEWARE - SWAGGER ====================
    if (app.Environment.IsDevelopment())
    {
        Log.Information("Development environment detected - enabling Swagger");

        app.UseSwagger(options =>
        {
            options.RouteTemplate = "swagger/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service v1");
            options.RoutePrefix = string.Empty;
            options.DocumentTitle = "Payment Service API";
            options.DefaultModelsExpandDepth(2);
            options.DefaultModelExpandDepth(2);
        });
    }
    else
    {
        Log.Information("Production environment detected - Swagger disabled");
    }

    // ==================== MIDDLEWARE - SECURITY & CORS ====================
    app.UseHttpsRedirection();
    app.UseCors("AllowAll");

    // ==================== MIDDLEWARE - ROUTING ====================
    app.UseRouting();
    app.UseAuthorization();

    // ==================== ENDPOINTS ====================
    app.MapHealthChecks("/health");
    app.MapControllers();

    // ==================== DATABASE MIGRATIONS ====================
    Log.Information("Applying database migrations...");

    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
            Log.Information("Database migration starting...");

            await dbContext.Database.MigrateAsync();

            Log.Information("Database migration completed successfully");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during database migration");
        throw;
    }

    // ==================== RUN APPLICATION ====================
    try
    {
        Log.Information("Starting Payment Service");
        Log.Information("Swagger UI will be available at: https://localhost:7006/");
        Log.Information("Health check available at: https://localhost:7006/health");
        Log.Information("API endpoints:");
        Log.Information("  POST   /api/payments - Process payment");
        Log.Information("  GET    /api/payments - Get all payments");
        Log.Information("  GET    /api/payments/{{id}} - Get payment by ID");
        Log.Information("  PUT    /api/payments/{{id}}/complete - Complete payment");
        Log.Information("  PUT    /api/payments/{{id}}/fail - Fail payment");
        Log.Information("  POST   /api/payments/{{id}}/refund - Refund payment");
        Log.Information("  GET    /api/payments/{{id}}/refunds - Get refunds");
        Log.Information("  POST   /api/payment-methods - Add payment method");
        Log.Information("  GET    /api/payment-methods/customer/{{customerId}} - Get payment methods");
        Log.Information("  PUT    /api/payment-methods/{{id}} - Update payment method");
        Log.Information("  DELETE /api/payment-methods/{{id}} - Delete payment method");

        app.Run();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Payment Service terminated unexpectedly");
        throw;
    }
    finally
    {
        Log.CloseAndFlush();
    }