using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureServicesAPI.DataModels;
using AzureServicesAPI.Helpers;
using AzureServicesAPI.Interfaces;

namespace AzureServicesAPI.Services
{
    public class KeyVaultService : IKeyVaultService
    {
        private readonly SecretClient _secretClient;

        public KeyVaultService(SettingsHelper settings)
        {
            _secretClient = new SecretClient(
                new Uri(settings.KeyVaultUri),
                new DefaultAzureCredential()
            );
        }

        public async Task<KeyVaultData?> GetSecretAsync(string secretName)
        {
            try
            {
                KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
                return new KeyVaultData
                {
                    SecretName = secret.Name,
                    SecretValue = secret.Value
                };
            }
            catch (Exception ex) when (ex.Message.Contains("SecretNotFound"))
            {
                return null;
            }
        }

        public async Task<KeyVaultData?> SetSecretAsync(KeyVaultData data)
        {
            try
            {
                KeyVaultSecret secret = await _secretClient.SetSecretAsync(
                    data.SecretName,
                    data.SecretValue
                );
                return new KeyVaultData
                {
                    SecretName = secret.Name,
                    SecretValue = secret.Value
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteSecretAsync(string secretName)
        {
            try
            {
                await _secretClient.StartDeleteSecretAsync(secretName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}