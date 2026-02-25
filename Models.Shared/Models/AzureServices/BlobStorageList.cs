using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Shared.Models.AzureServices
{
    public class BlobStorageList : ModelBase
    {
        public List<BlobStorage> Blobs { get; set; } = new();
        public int Count => Blobs.Count;

        public BlobStorageList() : base() { }

        public BlobStorageList(bool success, string message) : base(success, message) { }

        public BlobStorageList(string errorMessage) : base(errorMessage) { }
    }
}
