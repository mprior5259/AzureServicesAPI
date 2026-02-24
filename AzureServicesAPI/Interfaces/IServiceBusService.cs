using AzureServicesAPI.DataModels;

namespace AzureServicesAPI.Interfaces
{
    public interface IServiceBusService
    {
        Task<ServiceBusData?> SendMessageAsync(ServiceBusData data);
        Task<ServiceBusData?> PeekMessageAsync();
        Task<ServiceBusData?> ReceiveAndCompleteAsync();
        Task<List<ServiceBusData>> PeekDeadLettersAsync();
        Task<ServiceBusData?> ResendDeadLetterAsync(long sequenceNumber);
    }
}
