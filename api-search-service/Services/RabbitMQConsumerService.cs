using api_search_service.Models;
using api_search_service.Models.Events;
using Nest;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;
using System.Text.Json;

namespace api_search_service.Services
{
    /// <summary>
    /// Listens to RabbitMQ events and processes them
    /// </summary>
    public class RabbitMQConsumerService : IHostedService
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IChannel _channel;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<RabbitMQConsumerService> _logger;

        private const string ExchangeName = "ecommerce.events";
        private const string QueueName = "product.added.search";
        private const string RoutingKey = "product.added";

        public RabbitMQConsumerService(
            IConfiguration configuration,
            IElasticClient elasticClient,
            ILogger<RabbitMQConsumerService> logger)
        {
            var rabbitMqHost = configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
            _elasticClient = elasticClient;
            _logger = logger;

            // ✅ Create factory but don't connect yet (will connect in StartAsync)
            _connectionFactory = new ConnectionFactory()
            {
                HostName = rabbitMqHost,
                Port = 5672,
                DispatchConsumersAsync = true
            };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("🚀 RabbitMQ Consumer Service starting...");

                // ✅ Create connection asynchronously
                _connection = await _connectionFactory.CreateConnectionAsync();

                _channel = await _connection.CreateChannelAsync();

                // ✅ Declare Exchange (Topic type)
                await _channel.ExchangeDeclareAsync(
                    exchange: ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _logger.LogInformation($"✅ Exchange declared: {ExchangeName}");

                // ✅ Declare Queue
                await _channel.QueueDeclareAsync(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                _logger.LogInformation($"✅ Queue declared: {QueueName}");

                // ✅ Bind Queue to Exchange with Routing Key
                await _channel.QueueBindAsync(
                    queue: QueueName,
                    exchange: ExchangeName,
                    routingKey: RoutingKey);

                _logger.LogInformation($"✅ Queue bound to exchange with routing key: {RoutingKey}");

                // ✅ Set QoS (prefetch count = 1, process one message at a time)
                await _channel.BasicQosAsync(0, 1, false);

                // ✅ Create Consumer
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) => await OnMessageReceived(ea);

                await _channel.BasicConsumeAsync(
                    queue: QueueName,
                    autoAck: false,
                    consumerTag: "SearchServiceConsumer",
                    consumer: consumer);

                _logger.LogInformation("✅ RabbitMQ Consumer started and listening for messages...");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error starting RabbitMQ Consumer: {ex.Message}");
                throw;
            }
        }

        private async Task OnMessageReceived(BasicDeliverEventArgs ea)
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation($"📨 Message received: {message}");

                // Deserialize the event
                var productAddedEvent = JsonSerializer.Deserialize<ProductAddedEvent>(message);

                if (productAddedEvent != null)
                {
                    // ✅ Create SearchDocument from ProductAddedEvent
                    var searchDocument = new SearchDocument
                    {
                        DocumentType = "Product",
                        DocumentId = productAddedEvent.ProductId,
                        Title = productAddedEvent.Name,
                        Description = productAddedEvent.Description,
                        Price = productAddedEvent.Price,
                        Category = productAddedEvent.Category,
                        Status = productAddedEvent.Status,
                        CreatedAt = productAddedEvent.CreatedAt,
                        Content = $"{productAddedEvent.Name} {productAddedEvent.Description} {productAddedEvent.Category}",
                        Metadata = new Dictionary<string, object>
                        {
                            { "sku", productAddedEvent.Sku },
                            { "stock", productAddedEvent.Stock }
                        }
                    };

                    // ✅ Index the document in Elasticsearch
                    var response = await _elasticClient.IndexDocumentAsync(searchDocument);

                    if (response.IsValid)
                    {
                        _logger.LogInformation($"✅ Product indexed successfully: {productAddedEvent.Name} (ID: {productAddedEvent.ProductId})");

                        // ✅ Acknowledge the message
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogError($"❌ Failed to index document in Elasticsearch: {response.ServerError?.Error?.Reason}");
                        // ✅ Nack the message (requeue it)
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ Failed to deserialize ProductAddedEvent");
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error processing message: {ex.Message}");
                // ✅ Nack the message (requeue it for retry)
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 RabbitMQ Consumer Service stopping...");

            if (_channel != null)
                await _channel.CloseAsync();

            if (_connection != null)
                await _connection.CloseAsync();

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}