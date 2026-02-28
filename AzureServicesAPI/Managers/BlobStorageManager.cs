using AzureServicesAPI.DataModels;
using AzureServicesAPI.Interfaces;
using Models.Shared.Helpers;
using Models.Shared.Models.AzureServices;

namespace AzureServicesAPI.Managers
{
    public class BlobStorageManager : IBlobStorageManager
    {
        private readonly IBlobStorageService _blobStorageService;

        public BlobStorageManager(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public async Task<BlobStorage> UploadAsync(BlobStorage request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ContainerName))
                    return new BlobStorage("Container name cannot be empty.");

                if (string.IsNullOrWhiteSpace(request.BlobName))
                    return new BlobStorage("Blob name cannot be empty.");

                if (request.Content == null || request.Content.Length == 0)
                    return new BlobStorage("File content cannot be empty.");

                var data = ModelUtility.TryParseModel<BlobStorage, BlobStorageData>(request);
                if (data == null)
                    return new BlobStorage("Failed to parse request.");

                var result = await _blobStorageService.UploadAsync(data);
                if (result == null)
                    return new BlobStorage("Failed to upload file.");

                var response = ModelUtility.TryParseModel<BlobStorageData, BlobStorage>(result);
                if (response == null)
                    return new BlobStorage("Failed to parse response.");

                return response;
            }
            catch (Exception ex)
            {
                return new BlobStorage(ex.Message);
            }
        }

        public async Task<BlobStorage> DownloadAsync(string containerName, string blobName)
        {
            try
            {
                var result = await _blobStorageService.DownloadAsync(containerName, blobName);
                if (result == null)
                    return new BlobStorage($"Blob '{blobName}' not found in container '{containerName}'.");

                var response = ModelUtility.TryParseModel<BlobStorageData, BlobStorage>(result);
                if (response == null)
                    return new BlobStorage("Failed to parse response.");

                return response;
            }
            catch (Exception ex)
            {
                return new BlobStorage(ex.Message);
            }
        }

        public async Task<BlobStorageList> ListBlobsAsync(string containerName, string? prefix = null)
        {
            try
            {
                var results = await _blobStorageService.ListBlobsAsync(containerName, prefix);

                if (!results.Any())
                    return prefix == null
                        ? new BlobStorageList($"No blobs found in container '{containerName}'.")
                        : new BlobStorageList($"No blobs found in container '{containerName}' with prefix '{prefix}'.");

                var response = new BlobStorageList();
                response.Blobs = ModelUtility.TryParseModelList<BlobStorageData, BlobStorage>(results);
                return response;
            }
            catch (Exception ex)
            {
                return new BlobStorageList(ex.Message);
            }
        }

        public async Task<BlobStorage> DeleteAsync(string containerName, string blobName)
        {
            try
            {
                var success = await _blobStorageService.DeleteAsync(containerName, blobName);
                if (!success)
                    return new BlobStorage($"Blob '{blobName}' not found or already deleted.");

                return new BlobStorage(true, $"Blob '{blobName}' deleted successfully.");
            }
            catch (Exception ex)
            {
                return new BlobStorage(ex.Message);
            }
        }
    }
}