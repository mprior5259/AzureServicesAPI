using System.Text;
using System.Text.Json;
using Models.Shared.Models.AzureServices;

namespace ClientApp.Test
{
    public class AzureServicesApiProxy
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public AzureServicesApiProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Key Vault
        public async Task<KeyVault?> GetSecretAsync(string secretName)
        {
            var response = await _httpClient.GetAsync($"api/keyvault/{secretName}");
            return await DeserializeAsync<KeyVault>(response);
        }

        public async Task<KeyVault?> SetSecretAsync(string secretName, string value)
        {
            var body = new KeyVault() { SecretName = secretName, SecretValue = value };
            var response = await _httpClient.PostAsync("api/keyvault", Serialize(body));
            return await DeserializeAsync<KeyVault>(response);
        }

        public async Task<KeyVault?> DeleteSecretAsync(string secretName)
        {
            var response = await _httpClient.DeleteAsync($"api/keyvault/{secretName}");
            return await DeserializeAsync<KeyVault>(response);
        }

        // Service Bus
        public async Task<ServiceBus?> SendMessageAsync(string body, string? subject = null, string? correlationId = null)
        {
            var message = new ServiceBus() { Body = body, Subject = subject, CorrelationId = correlationId };
            var response = await _httpClient.PostAsync("api/servicebus/send", Serialize(message));
            return await DeserializeAsync<ServiceBus>(response);
        }

        public async Task<ServiceBusList?> PeekMessagesAsync(int count = 1)
        {
            var response = await _httpClient.GetAsync($"api/servicebus/peek?count={count}");
            return await DeserializeAsync<ServiceBusList>(response);
        }

        public async Task<ServiceBus?> ReceiveMessageAsync()
        {
            var response = await _httpClient.GetAsync("api/servicebus/receive");
            return await DeserializeAsync<ServiceBus>(response);
        }

        public async Task<ServiceBusList?> PeekDeadLettersAsync()
        {
            var response = await _httpClient.GetAsync("api/servicebus/deadletter");
            return await DeserializeAsync<ServiceBusList>(response);
        }

        public async Task<ServiceBus?> ResendDeadLetterAsync(long sequenceNumber)
        {
            var response = await _httpClient.PostAsync($"api/servicebus/deadletter/resend/{sequenceNumber}", null);
            return await DeserializeAsync<ServiceBus>(response);
        }

        // Blob Storage
        public async Task<BlobStorage?> UploadBlobAsync(string containerName, string blobName, string contentType, byte[] content)
        {
            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(containerName), "containerName");
            formData.Add(new StringContent(blobName), "blobName");
            formData.Add(new StringContent(contentType), "contentType");
            formData.Add(new ByteArrayContent(content), "file", blobName);

            var response = await _httpClient.PostAsync("api/blobstorage/upload", formData);
            return await DeserializeAsync<BlobStorage>(response);
        }

        public async Task<(BlobStorage? Metadata, byte[]? Content)> DownloadBlobAsync(string containerName, string blobName)
        {
            var response = await _httpClient.GetAsync($"api/blobstorage/{containerName}/{blobName}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await DeserializeAsync<BlobStorage>(response);
                return (error, null);
            }

            var contentType = response.Content.Headers.ContentType?.MediaType;
            var bytes = await response.Content.ReadAsByteArrayAsync();

            var metadata = new BlobStorage
            {
                BlobName = blobName,
                ContainerName = containerName,
                ContentType = contentType,
                FileSize = bytes.Length,
                Success = true
            };

            return (metadata, bytes);
        }

        public async Task<BlobStorageList?> ListBlobsAsync(string containerName, string? prefix = null)
        {
            var url = string.IsNullOrWhiteSpace(prefix)
                ? $"api/blobstorage/{containerName}"
                : $"api/blobstorage/{containerName}?prefix={prefix}";

            var response = await _httpClient.GetAsync(url);
            return await DeserializeAsync<BlobStorageList>(response);
        }

        public async Task<BlobStorage?> DeleteBlobAsync(string containerName, string blobName)
        {
            var response = await _httpClient.DeleteAsync($"api/blobstorage/{containerName}/{blobName}");
            return await DeserializeAsync<BlobStorage>(response);
        }

        // Helpers
        private StringContent Serialize(object obj)
        {
            return new StringContent(
                JsonSerializer.Serialize(obj, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                Encoding.UTF8,
                "application/json"
            );
        }

        private async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
    }
}