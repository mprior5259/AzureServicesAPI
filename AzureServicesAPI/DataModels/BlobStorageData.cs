namespace AzureServicesAPI.DataModels
{
    public class BlobStorageData
    {
        public string? ContainerName { get; set; }
        public string? BlobName { get; set; }
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public byte[]? Content { get; set; }
    }
}
