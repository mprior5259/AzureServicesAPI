using AzureServicesAPI.DataModels;
using Models.Shared.Models.AzureServices;
namespace AzureServicesAPI.Interfaces
{
    public interface IServiceBusManager
    {
        Task<ServiceBus> SendMessageAsync(ServiceBus request);
        Task<ServiceBus> PeekMessageAsync();
        Task<ServiceBus> ReceiveAndCompleteAsync();
        Task<ServiceBusList> PeekDeadLettersAsync();
        Task<ServiceBus> ResendDeadLetterAsync(long sequenceNumber);
    }
}
