using api_catalog_service.Events;

namespace api_catalog_service.Publishers
{
    public interface IEventPublisher
    {
        Task InitializeAsync(); 
        Task PublishProductAddedEventAsync(ProductAddedEvent @event);  
    }
}