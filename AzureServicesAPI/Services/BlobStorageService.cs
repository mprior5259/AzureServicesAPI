using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureServicesAPI.DataModels;
using AzureServicesAPI.Helpers;
using AzureServicesAPI.Interfaces;

namespace AzureServicesAPI.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly string _connectionString;

        public BlobStorageService(SettingsHelper settings)
        {
            _connectionString = settings.BlobStorageConnectionString;
        }

        public async Task<BlobStorageData?> UploadAsync(BlobStorageData data)
        {
            try
            {
                var containerClient = new BlobContainerClient(
                    _connectionString,
                    data.ContainerName
                );

                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient(data.BlobName);

                using var stream = new MemoryStream(data.Content!);
                await blobClient.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = data.ContentType
                });

                var properties = await blobClient.GetPropertiesAsync();

                return new BlobStorageData
                {
                    ContainerName = data.ContainerName,
                    BlobName = data.BlobName,
                    ContentType = properties.Value.ContentType,
                    FileSize = properties.Value.ContentLength,
                    LastModified = properties.Value.LastModified
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<BlobStorageData?> DownloadAsync(string containerName, string blobName)
        {
            var containerClient = new BlobContainerClient(
                _connectionString,
                containerName
            );

            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
                return null;

            var response = await blobClient.DownloadContentAsync();

            if (response?.Value == null)
                return null;

            return new BlobStorageData
            {
                ContainerName = containerName,
                BlobName = blobName,
                ContentType = response.Value.Details.ContentType,
                FileSize = response.Value.Details.ContentLength,
                LastModified = response.Value.Details.LastModified,
                Content = response.Value.Content?.ToArray() ?? Array.Empty<byte>()
            };
        }
        public async Task<List<BlobStorageData>> ListBlobsAsync(string containerName, string? prefix = null)
        {
            var results = new List<BlobStorageData>();
            var containerClient = new BlobContainerClient(_connectionString, containerName);

            if (!await containerClient.ExistsAsync())
                return results;

            await foreach (var blobItem in containerClient.GetBlobsAsync(
                traits: BlobTraits.None,
                states: BlobStates.None,
                prefix: prefix,
                cancellationToken: CancellationToken.None))
            {
                results.Add(new BlobStorageData
                {
                    ContainerName = containerName,
                    BlobName = blobItem.Name,
                    ContentType = blobItem.Properties.ContentType,
                    FileSize = blobItem.Properties.ContentLength,
                    LastModified = blobItem.Properties.LastModified
                });
            }

            return results;
        }

        public async Task<bool> DeleteAsync(string containerName, string blobName)
        {
            var containerClient = new BlobContainerClient(
                _connectionString,
                containerName
            );

            var blobClient = containerClient.GetBlobClient(blobName);
            var response = await blobClient.DeleteIfExistsAsync();
            return response.Value;
        }
    }
}