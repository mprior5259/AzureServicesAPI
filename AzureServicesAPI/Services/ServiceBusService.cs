using Azure.Messaging.ServiceBus;
using AzureServicesAPI.DataModels;
using AzureServicesAPI.Helpers;
using AzureServicesAPI.Interfaces;

namespace AzureServicesAPI.Services
{
    public class ServiceBusService : IServiceBusService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ServiceBusReceiver _receiver;
        private readonly ServiceBusReceiver _deadLetterReceiver;
        private readonly string _queueName;

        public ServiceBusService(SettingsHelper settings)
        {
            _client = new ServiceBusClient(settings.ServiceBusConnectionString);
            _queueName = settings.ServiceBusQueueName;
            _sender = _client.CreateSender(_queueName);
            _receiver = _client.CreateReceiver(_queueName);
            _deadLetterReceiver = _client.CreateReceiver(
                _queueName,
                new ServiceBusReceiverOptions
                {
                    SubQueue = SubQueue.DeadLetter
                }
            );
        }

        public async Task<ServiceBusData?> SendMessageAsync(ServiceBusData data)
        {
            try
            {
                var message = new ServiceBusMessage(data.Body)
                {
                    MessageId = data.MessageId ?? Guid.NewGuid().ToString(),
                    Subject = data.Subject,
                    CorrelationId = data.CorrelationId
                };

                await _sender.SendMessageAsync(message);

                return new ServiceBusData
                {
                    MessageId = message.MessageId,
                    Body = data.Body,
                    Subject = data.Subject,
                    CorrelationId = data.CorrelationId,
                    QueueName = _queueName
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ServiceBusData>> PeekMessagesAsync(int count = 1)
        {
            var messages = await _receiver.PeekMessagesAsync(maxMessages: count);
            return messages.Select(m => MapToData(m)).ToList();
        }

        public async Task<ServiceBusData?> ReceiveAndCompleteAsync()
        {
            var message = await _receiver.ReceiveMessageAsync(
                maxWaitTime: TimeSpan.FromSeconds(5)
            );
            if (message == null)
                return null;

            await _receiver.CompleteMessageAsync(message);
            return MapToData(message);
        }

        public async Task<List<ServiceBusData>> PeekDeadLettersAsync()
        {
            var results = new List<ServiceBusData>();
            // Contrain to 100 messages for performance
            var messages = await _deadLetterReceiver.PeekMessagesAsync(maxMessages: 100);

            foreach (var message in messages)
            {
                results.Add(MapToData(message));
            }

            return results;
        }

        public async Task<ServiceBusData?> ResendDeadLetterAsync(long sequenceNumber)
        {
            // Browse dead letter queue to find the specific message
            var messages = await _deadLetterReceiver.PeekMessagesAsync(maxMessages: 100);
            var target = messages.FirstOrDefault(m => m.SequenceNumber == sequenceNumber);

            if (target == null)
                return null;

            // Receive it by sequence number to get a locked instance
            var lockedMessage = await _deadLetterReceiver.ReceiveMessageAsync(
                maxWaitTime: TimeSpan.FromSeconds(5)
            );

            if (lockedMessage == null || lockedMessage.SequenceNumber != sequenceNumber)
            {
                if (lockedMessage != null)
                    await _deadLetterReceiver.AbandonMessageAsync(lockedMessage);
                return null;
            }

            var resentMessage = new ServiceBusMessage(lockedMessage.Body)
            {
                MessageId = Guid.NewGuid().ToString(),
                Subject = lockedMessage.Subject,
                CorrelationId = lockedMessage.CorrelationId
            };

            await _sender.SendMessageAsync(resentMessage);
            await _deadLetterReceiver.CompleteMessageAsync(lockedMessage);

            return MapToData(lockedMessage);
        }

        private ServiceBusData MapToData(ServiceBusReceivedMessage message)
        {
            return new ServiceBusData
            {
                MessageId = message.MessageId,
                Body = message.Body.ToString(),
                Subject = message.Subject,
                CorrelationId = message.CorrelationId,
                SequenceNumber = message.SequenceNumber,
                EnqueuedTime = message.EnqueuedTime,
                DeliveryCount = message.DeliveryCount,
                QueueName = _queueName
            };
        }
    }
}