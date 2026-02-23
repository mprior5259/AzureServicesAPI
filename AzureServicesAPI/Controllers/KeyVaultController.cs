using AzureServicesAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Shared.Models.AzureServices;

namespace AzureServicesAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class KeyVaultController : ControllerBase
    {
        private readonly IKeyVaultManager _keyVaultManager;

        public KeyVaultController(IKeyVaultManager keyVaultManager)
        {
            _keyVaultManager = keyVaultManager;
        }

        [HttpGet("{secretName}")]
        public async Task<IActionResult> GetSecretAsync(string secretName)
        {
            if (string.IsNullOrWhiteSpace(secretName))
                return BadRequest("Secret name cannot be empty.");

            var result = await _keyVaultManager.GetSecretAsync(secretName);
            if (result == null || !result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> SetSecretAsync([FromBody] KeyVault request)
        {
            if (string.IsNullOrWhiteSpace(request.SecretName) ||
                string.IsNullOrWhiteSpace(request.SecretValue))
                return BadRequest("Secret name and value are required.");

            var result = await _keyVaultManager.SetSecretAsync(request);
            if (result == null || !result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{secretName}")]
        public async Task<IActionResult> DeleteSecretAsync(string secretName)
        {
            if (string.IsNullOrWhiteSpace(secretName))
                return BadRequest("Secret name cannot be empty.");

            var result = await _keyVaultManager.DeleteSecretAsync(secretName);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}