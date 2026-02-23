using Models.Shared.Models.AzureServices;

namespace AzureServicesAPI.Interfaces
{
    public interface IKeyVaultManager
    {
        Task<KeyVault?> GetSecretAsync(string secretName);
        Task<KeyVault?> SetSecretAsync(KeyVault request);
        Task<KeyVault> DeleteSecretAsync(string secretName);
    }
}