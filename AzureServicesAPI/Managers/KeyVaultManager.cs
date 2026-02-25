using AzureServicesAPI.Interfaces;
using AzureServicesAPI.DataModels;
using Models.Shared.Models.AzureServices;
using Models.Shared.Helpers;

namespace AzureServicesAPI.Managers
{
    public class KeyVaultManager : IKeyVaultManager
    {
        private readonly IKeyVaultService _keyVaultService;

        public KeyVaultManager(IKeyVaultService keyVaultService)
        {
            _keyVaultService = keyVaultService;
        }

        public async Task<KeyVault?> GetSecretAsync(string secretName)
        {
            try
            {
                var data = await _keyVaultService.GetSecretAsync(secretName);
                if (data == null)
                    return new KeyVault("Secret not found.");

                KeyVault? keyVault = ModelUtility.TryParseModel<KeyVaultData, KeyVault>(data);
                if (keyVault == null)
                    return new KeyVault("Failed to parse result.");

                return keyVault;
            }
            catch (Exception ex)
            {
                return new KeyVault(ex.Message);
            }
        }

        public async Task<KeyVault?> SetSecretAsync(KeyVault request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.SecretName))
                    return new KeyVault("SecretName cannon be empty.");

                if (string.IsNullOrWhiteSpace(request.SecretValue))
                    return new KeyVault("SecretValue cannot be empty.");

                var data = ModelUtility.TryParseModel<KeyVault, KeyVaultData>(request);
                if (data == null)
                    return new KeyVault("Failed to parse request.");

                var result = await _keyVaultService.SetSecretAsync(data);
                if (result == null)
                    return new KeyVault("Failed to set secret.");

                KeyVault? keyVault = ModelUtility.TryParseModel<KeyVaultData, KeyVault>(result);
                if (keyVault == null)
                    return new KeyVault("Failed to parse result.");

                return keyVault;
            }
            catch (Exception ex)
            {
                return new KeyVault(ex.Message);
            }
        }

        public async Task<KeyVault> DeleteSecretAsync(string secretName)
        {
            try
            {
                var success = await _keyVaultService.DeleteSecretAsync(secretName);
                if (!success)
                    return new KeyVault("Failed to delete secret.");

                return new KeyVault { Success = true, Message = "Secret deleted successfully." };
            }
            catch (Exception ex)
            {
                return new KeyVault(ex.Message);
            }
        }
    }
}