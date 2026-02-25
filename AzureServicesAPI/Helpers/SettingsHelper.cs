using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace AzureServicesAPI.Helpers
{
    public class SettingsHelper
    {
        public string KeyVaultUri { get; }
        public string ServiceBusConnectionString { get; }
        public string ServiceBusQueueName { get; }
        public string BlobStorageConnectionString { get; }

        private SettingsHelper(
            string keyVaultUri,
            string serviceBusConnectionString,
            string serviceBusQueueName,
            string blobStorageConnectionString)
        {
            KeyVaultUri = keyVaultUri;
            ServiceBusConnectionString = serviceBusConnectionString;
            ServiceBusQueueName = serviceBusQueueName;
            BlobStorageConnectionString = blobStorageConnectionString;
        }

        public static async Task<SettingsHelper> CreateAsync(IConfiguration configuration)
        {
            var keyVaultUri = configuration["KeyVault:VaultUri"]
                ?? throw new InvalidOperationException("KeyVaultUri is not configured.");

            var secretClient = new SecretClient(
                new Uri(keyVaultUri),
                new DefaultAzureCredential()
            );

            var serviceBusKey = configuration["ServiceBus:ConnectionKey"]
                ?? throw new InvalidOperationException("ServiceBus connection key is not configured.");
            
            var serviceBusQueueName = configuration["ServiceBus:QueueName"]
                ?? throw new InvalidOperationException("ServiceBusQueueName is not configured.");

            var blobStorageKey = configuration["BlobStorage:ConnectionKey"]
                ?? throw new InvalidOperationException("BlobStorage connection key is not configured.");

            // Fetch all secrets in parallel
            var serviceBusConnectionStringTask = secretClient.GetSecretAsync(serviceBusKey);
            var blobStorageConnectionStringTask = secretClient.GetSecretAsync(blobStorageKey);

            await Task.WhenAll(
                serviceBusConnectionStringTask,
                blobStorageConnectionStringTask
            );

            return new SettingsHelper(
                keyVaultUri,
                serviceBusConnectionStringTask.Result.Value.Value
                    ?? throw new InvalidOperationException("ServiceBus ConnectionString not found in Key Vault."),
                serviceBusQueueName,
                blobStorageConnectionStringTask.Result.Value.Value
                    ?? throw new InvalidOperationException("BlobStorage ConnectionString not found in Key Vault.")
            );
        }
    }
}
