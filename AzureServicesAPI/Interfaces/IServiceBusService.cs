using AzureServicesAPI.DataModels;

namespace AzureServicesAPI.Interfaces
{
    public interface IServiceBusService
    {
        Task<ServiceBusData?> SendMessageAsync(ServiceBusData data);
        Task<List<ServiceBusData>> PeekMessagesAsync(int count = 1);
        Task<ServiceBusData?> ReceiveAndCompleteAsync();
        Task<List<ServiceBusData>> PeekDeadLettersAsync();
        Task<ServiceBusData?> ResendDeadLetterAsync(long sequenceNumber);
    }
}
