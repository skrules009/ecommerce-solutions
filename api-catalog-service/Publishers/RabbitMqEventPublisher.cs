using api_catalog_service.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Serilog;

namespace api_catalog_service.Publishers
{
    /// <summary>
    /// RabbitMQ implementation of event publisher
    /// </summary>
    public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private RabbitMQ.Client.IChannel _channel;  // ✅ Fully qualified name
        private readonly string _exchangeName = "ecommerce.events";
        private bool _isInitialized = false;

        public RabbitMqEventPublisher(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <summary>
        /// Initialize the channel - call once on startup
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                _channel = await _connection.CreateChannelAsync();

                // Declare exchange
                await _channel.ExchangeDeclareAsync(
                    exchange: _exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false
                );

                _isInitialized = true;
                Log.Information("✅ RabbitMQ Publisher initialized - Exchange: {Exchange}", _exchangeName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to initialize RabbitMQ Publisher");
                throw;
            }
        }

        public async Task PublishProductAddedEventAsync(ProductAddedEvent @event)
        {
            if (!_isInitialized)
            {
                Log.Warning("⚠️ Publisher not initialized, initializing now...");
                await InitializeAsync();
            }

            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            try
            {
                const string routingKey = "product.added";

                // Serialize event to JSON
                var json = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(json);

                // Create basic properties
                var properties = _channel.CreateBasicProperties();  // ✅ This is synchronous
                properties.ContentType = "application/json";
                properties.ContentEncoding = "utf-8";
                properties.DeliveryMode = 2;  // Persistent delivery
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                // Publish message
                await _channel.BasicPublishAsync(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    mandatory: false,
                    basicProperties: properties,
                    body: body
                );

                Log.Information("✅ ProductAddedEvent published: Product {ProductId} - {ProductName}",
                    @event.ProductId, @event.Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to publish ProductAddedEvent: {ProductId}", @event.ProductId);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_channel != null && _channel.IsOpen)
                {
                    await _channel.CloseAsync();
                }
                _channel?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error closing RabbitMQ channel");
            }
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }
    }
}