using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Shared.Models.AzureServices
{
    public class ServiceBus : ModelBase
    {
        public string? MessageId { get; set; }
        public string? Body { get; set; }
        public string? QueueName { get; set; }
        public long? SequenceNumber { get; set; }
        public DateTimeOffset? EnqueuedTime { get; set; }
        public int? DeliveryCount { get; set; }
        public string? CorrelationId { get; set; }
        public string? Subject { get; set; }

        public ServiceBus() : base() { }

        public ServiceBus(string errorMessage) : base(errorMessage) { }

        public ServiceBus(bool success, string message) : base(success, message) { }
    }
}
