using AzureServicesAPI.DataModels;
using AzureServicesAPI.Interfaces;
using Models.Shared.Helpers;
using Models.Shared.Models.AzureServices;

namespace AzureServicesAPI.Managers
{
    public class ServiceBusManager : IServiceBusManager
    {
        private readonly IServiceBusService _serviceBusService;

        public ServiceBusManager(IServiceBusService serviceBusService)
        {
            _serviceBusService = serviceBusService;
        }

        public async Task<ServiceBus> SendMessageAsync(ServiceBus request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Body))
                    return new ServiceBus("Message body cannot be empty.");

                // Auto-generate MessageId if not provided
                if (string.IsNullOrWhiteSpace(request.MessageId))
                    request.MessageId = Guid.NewGuid().ToString();

                // Clear optional fields if empty
                if (string.IsNullOrWhiteSpace(request.CorrelationId))
                    request.CorrelationId = null;

                if (string.IsNullOrWhiteSpace(request.Subject))
                    request.Subject = null;

                var data = ModelUtility.TryParseModel<ServiceBus, ServiceBusData>(request);
                if (data == null)
                    return new ServiceBus("Failed to parse request.");

                var result = await _serviceBusService.SendMessageAsync(data);
                if (result == null)
                    return new ServiceBus("Failed to send message.");

                var response = ModelUtility.TryParseModel<ServiceBusData, ServiceBus>(result);
                return response ?? new ServiceBus("Failed to parse response.");
            }
            catch (Exception ex)
            {
                return new ServiceBus(ex.Message);
            }
        }

        public async Task<ServiceBusList> PeekMessagesAsync(int count = 1)
        {
            try
            {
                var results = await _serviceBusService.PeekMessagesAsync(count);

                if (!results.Any())
                    return new ServiceBusList(true, "Queue is empty.");

                var response = new ServiceBusList();
                response.Messages = ModelUtility.TryParseModelList<ServiceBusData, ServiceBus>(results);
                return response;
            }
            catch (Exception ex)
            {
                return new ServiceBusList(ex.Message);
            }
        }

        public async Task<ServiceBus> ReceiveAndCompleteAsync()
        {
            try
            {
                var result = await _serviceBusService.ReceiveAndCompleteAsync();
                if (result == null)
                    return new ServiceBus(true, "Queue is empty.");

                var response = ModelUtility.TryParseModel<ServiceBusData, ServiceBus>(result);
                return response ?? new ServiceBus("Failed to parse response.");
            }
            catch (Exception ex)
            {
                return new ServiceBus(ex.Message);
            }
        }

        public async Task<ServiceBusList> PeekDeadLettersAsync()
        {
            try
            {
                var results = await _serviceBusService.PeekDeadLettersAsync();

                if (!results.Any())
                    return new ServiceBusList(true, "Dead letter queue is empty.");

                var response = ModelUtility.TryParseModelList<ServiceBusData, ServiceBus>(results);
                
                return response != null && response.Any() 
                    ? new ServiceBusList { Messages = response } 
                    : new ServiceBusList("Failed to parse response.");
            }
            catch (Exception ex)
            {
                return new ServiceBusList(ex.Message);
            }
        }

        public async Task<ServiceBus> ResendDeadLetterAsync(long sequenceNumber)
        {
            try
            {
                var result = await _serviceBusService.ResendDeadLetterAsync(sequenceNumber);
                if (result == null)
                    return new ServiceBus($"No message found with sequence number {sequenceNumber}.");

                var response = ModelUtility.TryParseModel<ServiceBusData, ServiceBus>(result);
                return response ?? new ServiceBus("Failed to parse response.");
            }
            catch (Exception ex)
            {
                return new ServiceBus(ex.Message);
            }
        }
    }
}