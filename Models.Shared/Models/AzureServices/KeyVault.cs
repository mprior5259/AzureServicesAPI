namespace Models.Shared.Models.AzureServices
{
    public class KeyVault : ModelBase
    {
        public string? SecretName { get; set; }
        public string? SecretValue { get; set; }

        public KeyVault() : base()
        {
        }

        public KeyVault(string errorMessage) : base(errorMessage)
        {
        }
    }
}
