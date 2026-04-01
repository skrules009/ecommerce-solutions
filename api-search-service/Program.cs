using api_search_service.Services;
using Microsoft.AspNetCore.Connections;
using Nest;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using IConnectionFactory = RabbitMQ.Client.IConnectionFactory;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/searchservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("🚀 Search Service starting...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // ==================== ELASTICSEARCH ====================
    var elasticsearchUrl = builder.Configuration.GetValue<string>("Elasticsearch:Url")
        ?? "http://localhost:9200";

    Log.Information("Connecting to Elasticsearch: {ElasticsearchUrl}", elasticsearchUrl);

    var elasticsearchSettings = new ConnectionSettings(new Uri(elasticsearchUrl))
        .DefaultIndex("search-documents");

    var elasticClient = new ElasticClient(elasticsearchSettings);

    // ✅ TEST CONNECTION
    Log.Information("Testing Elasticsearch connection...");
    var pingResponse = elasticClient.Ping();
    if (pingResponse.IsValid)
    {
        Log.Information("✅ Elasticsearch connection successful!");
    }
    else
    {
        Log.Error("❌ Elasticsearch connection failed: {Error}", pingResponse.ServerError?.Error?.Reason);
    }

    builder.Services.AddSingleton<IElasticClient>(elasticClient);

    // ==================== RABBITMQ ====================
    var rabbitMqHost = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
    Log.Information("Configuring RabbitMQ: {RabbitMqHost}:5672", rabbitMqHost);

    // ✅ Register RabbitMQ Consumer Service
    builder.Services.AddHostedService<RabbitMQConsumerService>();
    Log.Information("✅ RabbitMQ configured successfully");

    // ==================== SERVICES ====================
    builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();

    // ==================== API ====================
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
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

    // ==================== MIDDLEWARE ====================
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthorization();
    app.MapControllers();

    // ==================== HEALTH CHECK ====================
    app.MapGet("/health", () => new { status = "Healthy" });

    Log.Information("✅ Search Service started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Search Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}