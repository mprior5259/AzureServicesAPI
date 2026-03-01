using AzureServicesAPI.Helpers;
using AzureServicesAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Shared.Models.AzureServices;

namespace AzureServicesAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BlobStorageController : ControllerBase
    {
        private readonly IBlobStorageManager _blobStorageManager;

        public BlobStorageController(IBlobStorageManager blobStorageManager)
        {
            _blobStorageManager = blobStorageManager;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAsync(
            [FromForm] string containerName,
            [FromForm] string blobName,
            [FromForm] string? contentType,
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File cannot be empty.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var request = new BlobStorage
            {
                ContainerName = containerName,
                BlobName = blobName,
                ContentType = ContentTypeHelper.SanitizeContentType(contentType ?? file.ContentType),
                Content = memoryStream.ToArray()
            };

            var result = await _blobStorageManager.UploadAsync(request);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{containerName}/{blobName}")]
        public async Task<IActionResult> DownloadAsync(string containerName, string blobName)
        {
            var result = await _blobStorageManager.DownloadAsync(containerName, blobName);

            if (!result.Success)
                return NotFound(result);

            if (result.Content == null || result.Content.Length == 0)
                return Ok(result);

            return new FileContentResult(result.Content, result.ContentType ?? "application/octet-stream")
            {
                FileDownloadName = blobName
            };
        }

        [HttpGet("{containerName}")]
        public async Task<IActionResult> ListBlobsAsync(string containerName, [FromQuery] string? prefix = null)
        {
            var result = await _blobStorageManager.ListBlobsAsync(containerName, prefix);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{containerName}/{blobName}")]
        public async Task<IActionResult> DeleteAsync(string containerName, string blobName)
        {
            var result = await _blobStorageManager.DeleteAsync(containerName, blobName);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}