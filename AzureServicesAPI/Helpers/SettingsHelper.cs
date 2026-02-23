namespace AzureServicesAPI.Helpers
{
    public class SettingsHelper
    {
        public readonly string KeyVaultUri;


        public SettingsHelper(IConfiguration configuration)
        {
            KeyVaultUri = configuration["KeyVaultUri"] ?? throw new InvalidOperationException("KeyVaultUri is not configured.");
        }
    }
}
