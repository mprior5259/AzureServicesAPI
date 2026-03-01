using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Shared.Models.AzureServices
{
    public class BlobStorage : ModelBase
    {
        public string? ContainerName { get; set; }
        public string? BlobName { get; set; }
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public byte[]? Content { get; set; }

        public BlobStorage() : base() { }

        public BlobStorage(bool success, string message) : base(success, message) { }

        public BlobStorage(string errorMessage) : base(errorMessage) { }
    }
}
