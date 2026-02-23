using AzureServicesAPI.DataModels;

namespace AzureServicesAPI.Interfaces
{
    public interface IKeyVaultService
    {
        Task<KeyVaultData?> GetSecretAsync(string secretName);
        Task<KeyVaultData?> SetSecretAsync(KeyVaultData keyVaultData);
        Task<bool> DeleteSecretAsync(string secretName);
    }
}
