using AzureServicesAPI.DataModels;
using Models.Shared.Models.AzureServices;
namespace AzureServicesAPI.Interfaces
{
    public interface IServiceBusManager
    {
        Task<ServiceBus> SendMessageAsync(ServiceBus request);
        Task<ServiceBusList> PeekMessagesAsync(int count = 1);
        Task<ServiceBus> ReceiveAndCompleteAsync();
        Task<ServiceBusList> PeekDeadLettersAsync();
        Task<ServiceBus> ResendDeadLetterAsync(long sequenceNumber);
    }
}
