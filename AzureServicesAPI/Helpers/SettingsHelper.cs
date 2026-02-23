namespace AzureServicesAPI.Helpers
{
    public class SettingsHelper
    {
        public readonly string KeyVaultUri;


        public SettingsHelper(IConfiguration configuration)
        {
            KeyVaultUri = configuration["KeyVault:VaultUri"] ?? throw new InvalidOperationException("KeyVaultUri is not configured.");
        }
    }
}
