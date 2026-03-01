using Models.Shared.Models.AzureServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Test.Helpers
{
    internal static class BlobStorageHelper
    {
        internal static async Task UploadBlob(AzureServicesApiProxy apiClient)
        {
            Console.Clear();
            Console.WriteLine("=== Upload Blob ===\n");

            Console.Write("Enter container name: ");
            var containerName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(containerName))
            {
                Console.WriteLine("Container name cannot be empty.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter full file path of the file you wish to upload: ");
            var filePath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter blob name: ");
            var blobName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(blobName))
            {
                Console.WriteLine("Blob name cannot be empty.");
                Console.ReadKey();
                return;
            }

            // Append extension from file if blob name has none
            if (string.IsNullOrWhiteSpace(Path.GetExtension(blobName)))
            {
                var extension = Path.GetExtension(filePath);
                if (!string.IsNullOrWhiteSpace(extension))
                {
                    blobName += extension;
                    Console.WriteLine($"Extension appended. Blob name: {blobName}");
                }
            }

            Console.Write("Enter content type (e.g. image/png, press Enter for auto-detect): ");
            var contentType = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(contentType))
                contentType = GetContentTypeFromExtension(Path.GetExtension(filePath));

            var content = await File.ReadAllBytesAsync(filePath);
            var result = await apiClient.UploadBlobAsync(containerName, blobName, contentType, content);
            PrintResult(result);
        }

        internal static async Task DownloadBlob(AzureServicesApiProxy apiClient)
        {
            Console.Clear();
            Console.WriteLine("=== Download Blob ===\n");

            Console.Write("Enter container name: ");
            var containerName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(containerName))
            {
                Console.WriteLine("Container name cannot be empty.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter blob name: ");
            var blobName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(blobName))
            {
                Console.WriteLine("Blob name cannot be empty.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter path to save file (press Enter to skip saving): ");
            var savePath = Console.ReadLine();

            var (metadata, content) = await apiClient.DownloadBlobAsync(containerName, blobName);

            if (content != null && !string.IsNullOrWhiteSpace(savePath))
            {
                var fileName = blobName;

                // Append extension from content type if blob name has none
                if (string.IsNullOrWhiteSpace(Path.GetExtension(blobName)) && !string.IsNullOrWhiteSpace(metadata?.ContentType))
                    fileName = blobName + GetExtensionFromContentType(metadata.ContentType);

                var fullPath = Path.Combine(savePath, blobName);
                await File.WriteAllBytesAsync(fullPath, content);
                Console.WriteLine($"\nFile saved to: {fullPath}");
            }

            PrintResult(metadata);
        }
        internal static async Task ListBlobs(AzureServicesApiProxy apiClient)
        {
            Console.Clear();
            Console.WriteLine("=== List Blobs ===\n");

            Console.Write("Enter container name: ");
            var containerName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(containerName))
            {
                Console.WriteLine("Container name cannot be empty.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter prefix to filter (optional, press Enter to skip): ");
            var prefix = Console.ReadLine();

            var result = await apiClient.ListBlobsAsync(
                containerName,
                string.IsNullOrWhiteSpace(prefix) ? null : prefix
            );

            PrintListResult(result);
        }

        internal static async Task DeleteBlob(AzureServicesApiProxy apiClient)
        {
            Console.Clear();
            Console.WriteLine("=== Delete Blob ===\n");

            Console.Write("Enter container name: ");
            var containerName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(containerName))
            {
                Console.WriteLine("Container name cannot be empty.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter blob name: ");
            var blobName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(blobName))
            {
                Console.WriteLine("Blob name cannot be empty.");
                Console.ReadKey();
                return;
            }

            Console.Write($"Are you sure you want to delete '{blobName}'? (y/n): ");
            if (Console.ReadLine()?.ToLower() != "y")
            {
                Console.WriteLine("Cancelled.");
                Console.ReadKey();
                return;
            }

            var result = await apiClient.DeleteBlobAsync(containerName, blobName);
            PrintResult(result);
        }

        private static void PrintResult(BlobStorage? result)
        {
            if (result == null)
            {
                Console.WriteLine("\nNo response received.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nSuccess: {result.Success}");
            if (!string.IsNullOrWhiteSpace(result.Message))
                Console.WriteLine($"Message: {result.Message}");
            if (!string.IsNullOrWhiteSpace(result.ContainerName))
                Console.WriteLine($"Container: {result.ContainerName}");
            if (!string.IsNullOrWhiteSpace(result.BlobName))
                Console.WriteLine($"Blob Name: {result.BlobName}");
            if (!string.IsNullOrWhiteSpace(result.ContentType))
                Console.WriteLine($"Content Type: {result.ContentType}");
            if (result.FileSize.HasValue)
                Console.WriteLine($"File Size: {result.FileSize} bytes");
            if (result.LastModified.HasValue)
                Console.WriteLine($"Last Modified: {result.LastModified}");

            Console.ReadKey();
        }

        private static void PrintListResult(BlobStorageList? result)
        {
            if (result == null)
            {
                Console.WriteLine("\nNo response received.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nSuccess: {result.Success}");
            if (!string.IsNullOrWhiteSpace(result.Message))
                Console.WriteLine($"Message: {result.Message}");

            if (result.Blobs != null && result.Blobs.Any())
            {
                Console.WriteLine($"Count: {result.Count}\n");
                foreach (var blob in result.Blobs)
                {
                    Console.WriteLine("---");
                    if (!string.IsNullOrWhiteSpace(blob.BlobName))
                        Console.WriteLine($"  Name: {blob.BlobName}");
                    if (!string.IsNullOrWhiteSpace(blob.ContentType))
                        Console.WriteLine($"  Content Type: {blob.ContentType}");
                    if (blob.FileSize.HasValue)
                        Console.WriteLine($"  Size: {blob.FileSize} bytes");
                    if (blob.LastModified.HasValue)
                        Console.WriteLine($"  Last Modified: {blob.LastModified}");
                }
            }

            Console.ReadKey();
        }

        private static string GetContentTypeFromExtension(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".bmp" => "image/bmp",
                ".tiff" => "image/tiff",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".html" => "text/html",
                ".css" => "text/css",
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".mp4" => "video/mp4",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }

        private static string GetExtensionFromContentType(string contentType)
        {
            return contentType.ToLower() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "image/svg+xml" => ".svg",
                "image/bmp" => ".bmp",
                "image/tiff" => ".tiff",
                "application/pdf" => ".pdf",
                "application/msword" => ".doc",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                "application/vnd.ms-excel" => ".xls",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
                "text/plain" => ".txt",
                "text/html" => ".html",
                "text/css" => ".css",
                "text/csv" => ".csv",
                "application/json" => ".json",
                "application/xml" => ".xml",
                "audio/mpeg" => ".mp3",
                "audio/wav" => ".wav",
                "video/mp4" => ".mp4",
                "application/zip" => ".zip",
                _ => ""
            };
        }
    }
}
