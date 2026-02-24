using Models.Shared.Models;
using Models.Shared.Models.AzureServices;

public class ServiceBusList : ModelBase
{
    public List<ServiceBus> Messages { get; set; } = new();
    public int Count => Messages.Count;

    public ServiceBusList() : base() { }

    public ServiceBusList(bool success, string message) : base(success, message) { }

    public ServiceBusList(string errorMessage) : base(errorMessage) { }
}