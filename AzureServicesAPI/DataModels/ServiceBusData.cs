namespace AzureServicesAPI.DataModels
{
    public class ServiceBusData
    {
        public string? MessageId { get; set; }
        public string? Body { get; set; }
        public string? QueueName { get; set; }
        public long? SequenceNumber { get; set; }
        public DateTimeOffset? EnqueuedTime { get; set; }
        public int? DeliveryCount { get; set; }
        public string? CorrelationId { get; set; }
        public string? Subject { get; set; }
    }
}
