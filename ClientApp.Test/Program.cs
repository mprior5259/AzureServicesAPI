using ClientApp.Test;
using ClientApp.Test.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json", optional: false)
                .Build();
var baseUrl = configuration["Api:BaseUrl"]!;
var token = await AuthTokenHelper.AcquireAccessToken(configuration);

// Http client with token
var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

var apiClient = new AzureServicesApiProxy(httpClient);

// Main menu
while (true)
{
    Console.Clear();
    Console.WriteLine("=== AzureServicesAPI Client Demo ===\n");
    Console.WriteLine("1. Key Vault");
    Console.WriteLine("2. Service Bus");
    Console.WriteLine("3. Blob Storage");
    Console.WriteLine("0. Exit\n");
    Console.Write("Select an option: ");

    switch (Console.ReadLine())
    {
        case "1":
            await KeyVaultMenu(apiClient);
            break;
        case "2":
            await ServiceBusMenu(apiClient);
            break;
        case "3":
            await BlobStorageMenu(apiClient);
            break;
        case "0":
            return;
        default:
            Console.WriteLine("Invalid option.");
            Console.ReadKey();
            break;
    }
}

static async Task KeyVaultMenu(AzureServicesApiProxy apiClient)
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("=== Key Vault ===\n");
        Console.WriteLine("1. Get secret");
        Console.WriteLine("2. Set secret");
        Console.WriteLine("3. Delete secret");
        Console.WriteLine("0. Back\n");
        Console.Write("Select an option: ");

        switch (Console.ReadLine())
        {
            case "1":
                await KeyVaultHelper.GetSecret(apiClient);
                break;
            case "2":
                await KeyVaultHelper.SetSecret(apiClient);
                break;
            case "3":
                await KeyVaultHelper.DeleteSecret(apiClient);
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid option.");
                Console.ReadKey();
                break;
        }
    }
}

static async Task ServiceBusMenu(AzureServicesApiProxy apiClient)
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("=== Service Bus ===\n");
        Console.WriteLine("1. Send message");
        Console.WriteLine("2. Peek messages");
        Console.WriteLine("3. Receive and complete");
        Console.WriteLine("4. Peek dead letter queue");
        Console.WriteLine("5. Resend dead letter message");
        Console.WriteLine("0. Back\n");
        Console.Write("Select an option: ");

        switch (Console.ReadLine())
        {
            case "1":
                await ServiceBusHelper.SendMessage(apiClient);
                break;
            case "2":
                await ServiceBusHelper.PeekMessages(apiClient);
                break;
            case "3":
                await ServiceBusHelper.ReceiveMessage(apiClient);
                break;
            case "4":
                await ServiceBusHelper.PeekDeadLetters(apiClient);
                break;
            case "5":
                await ServiceBusHelper.ResendDeadLetter(apiClient);
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid option.");
                Console.ReadKey();
                break;
        }
    }
}

static async Task BlobStorageMenu(AzureServicesApiProxy apiClient)
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("=== Blob Storage ===\n");
        Console.WriteLine("1. Upload file");
        Console.WriteLine("2. Download file");
        Console.WriteLine("3. List blobs");
        Console.WriteLine("4. Delete blob");
        Console.WriteLine("0. Back\n");
        Console.Write("Select an option: ");

        switch (Console.ReadLine())
        {
            case "1":
                await BlobStorageHelper.UploadBlob(apiClient);
                break;
            case "2":
                await BlobStorageHelper.DownloadBlob(apiClient);
                break;
            case "3":
                await BlobStorageHelper.ListBlobs(apiClient);
                break;
            case "4":
                await BlobStorageHelper.DeleteBlob(apiClient);
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid option.");
                Console.ReadKey();
                break;
        }
    }
}
