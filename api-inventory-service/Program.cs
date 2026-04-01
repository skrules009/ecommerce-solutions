    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using api_inventory_service.Data;
    using api_inventory_service.Repositories;
    using api_inventory_service.Services;

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
        .WriteTo.File("logs/inventory-service-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();

    builder.Host.UseSerilog();

    // ==================== DEPENDENCY INJECTION ====================
    builder.Services.AddDbContext<InventoryDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

    builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
    builder.Services.AddScoped<IInventoryService, InventoryServiceImpl>();

    // ==================== API & CONTROLLERS ====================
    builder.Services.AddControllers();

    // ==================== SWAGGER / OPENAPI ====================
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Inventory Service API",
            Version = "v1",
            Description = "Microservice for managing product inventory and stock levels"
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

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Service v1");
            options.RoutePrefix = string.Empty;
        });
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
            var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
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
        Log.Information("Starting Inventory Service");
        Log.Information("Swagger UI will be available at: https://localhost:7005/");
        Log.Information("Health check available at: https://localhost:7005/health");

        app.Run();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Inventory Service terminated unexpectedly");
        throw;
    }
    finally
    {
        Log.CloseAndFlush();
    }