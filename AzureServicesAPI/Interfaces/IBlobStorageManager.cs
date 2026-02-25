using Models.Shared.Models.AzureServices;

namespace AzureServicesAPI.Interfaces
{
    public interface IBlobStorageManager
    {
        Task<BlobStorage> UploadAsync(BlobStorage request);
        Task<BlobStorage> DownloadAsync(string containerName, string blobName);
        Task<BlobStorageList> ListBlobsAsync(string containerName, string? prefix);
        Task<BlobStorage> DeleteAsync(string containerName, string blobName);
    }
}