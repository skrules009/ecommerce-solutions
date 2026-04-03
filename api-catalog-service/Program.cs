using api_catalog_service.Data;
using api_catalog_service.Publishers;
using api_catalog_service.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/catalogservice-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("🚀 CatalogService starting...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // ==================== DATABASE ====================
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Log.Information("Configuring Database: {ConnectionString}",
        connectionString?.Split("Password=")[0] + "Password=***");

    builder.Services.AddDbContext<CatalogDbContext>(options =>
        options.UseSqlServer(connectionString)
    );

    // ==================== RABBITMQ ====================
    var rabbitMqHost = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";

    var connectionFactory = new ConnectionFactory()
    {
        HostName = rabbitMqHost,
        Port = 5672
    };

    var connection = await connectionFactory.CreateConnectionAsync();  // ✅ Await async call

    builder.Services.AddSingleton(connection);
    builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

    // Initialize publisher on startup
    using (var scope = builder.Services.BuildServiceProvider().CreateScope())
    {
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        await publisher.InitializeAsync();  // ✅ Await async call
    }

    Log.Information("✅ RabbitMQ configured successfully");

    // ==================== SERVICES ====================
    builder.Services.AddScoped<IProductService, ProductService>();

    // ==================== API ====================
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Catalog Service API",
            Version = "v1",
            Description = "API for managing products in the catalog"
        });
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // ==================== DATABASE MIGRATION ====================
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            Log.Information("Applying database migrations...");
            dbContext.Database.Migrate();
            Log.Information("✅ Database migrations completed successfully");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "⚠️ Failed to apply database migrations. Make sure the database connection is correct.");
        }
    }

    // ==================== INITIALIZE RABBITMQ ON STARTUP ====================
    using (var scope = builder.Services.BuildServiceProvider().CreateScope())
    {
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        await publisher.InitializeAsync();  // ✅ Call the async method
    }
    Log.Information("✅ RabbitMQ Publisher initialized successfully");

    // ==================== MIDDLEWARE ====================
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog Service API v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthorization();
    app.MapControllers();

    // ==================== HEALTH CHECK ====================
    app.MapGet("/health", () => new { status = "Healthy", service = "CatalogService" });

    Log.Information("✅ CatalogService started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ CatalogService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}