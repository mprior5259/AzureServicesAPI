using AzureServicesAPI.DataModels;

namespace AzureServicesAPI.Interfaces
{
    public interface IBlobStorageService
    {
        Task<BlobStorageData?> UploadAsync(BlobStorageData data);
        Task<BlobStorageData?> DownloadAsync(string containerName, string blobName);
        Task<List<BlobStorageData>> ListBlobsAsync(string containerName, string? prefix);
        Task<bool> DeleteAsync(string containerName, string blobName);
    }
}