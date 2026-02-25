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
        public async Task<IActionResult> UploadAsync([FromBody] BlobStorage request)
        {
            if (request == null)
                return BadRequest("Request cannot be empty.");

            // Ensure content type being passed in is valid, otherwise default to application/octet-stream
            request.ContentType = ContentTypeHelper.SanitizeContentType(request.ContentType);

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

            return Ok(result);
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