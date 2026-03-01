using Models.Shared.Models.AzureServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Test.Helpers
{
    internal static class KeyVaultHelper
    {
        internal static async Task GetSecret(AzureServicesApiProxy apiClient)
        {
            Console.Clear();
            Console.WriteLine("=== Get Secret ===\n");
            Console.Write("Enter secret name: ");
            var secretName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(secretName))
            {
                Console.WriteLine("Secret name cannot be empty.");
                Console.ReadKey();
                return;
            }

            var result = await apiClient.GetSecretAsync(secretName);
            PrintResult(result);
        }

        internal static async Task SetSecret(AzureServicesApiProxy apiClient)
        {
            Console.Clear();
            Console.WriteLine("=== Set Secret ===\n");
            Console.Write("Enter secret name: ");
            var secretName = Console.ReadLine();

            Console.Write("Enter secret value: ");
            var secretValue = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(secretName) || string.IsNullOrWhiteSpace(secretValue))
            {
                Console.WriteLine("Secret name and value cannot be empty.");
                Console.ReadKey();
                return;
            }

            var result = await apiClient.SetSecretAsync(secretName, secretValue);
            PrintResult(result);
        }

        internal static async Task DeleteSecret(AzureServicesApiProxy apiClient)
        {
            Console.Clear();
            Console.WriteLine("=== Delete Secret ===\n");
            Console.Write("Enter secret name: ");
            var secretName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(secretName))
            {
                Console.WriteLine("Secret name cannot be empty.");
                Console.ReadKey();
                return;
            }

            Console.Write($"Are you sure you want to delete '{secretName}'? (y/n): ");
            if (Console.ReadLine()?.ToLower() != "y")
            {
                Console.WriteLine("Cancelled.");
                Console.ReadKey();
                return;
            }

            var result = await apiClient.DeleteSecretAsync(secretName);
            PrintResult(result);
        }

        private static void PrintResult(KeyVault? result)
        {
            if (result == null)
            {
                Console.WriteLine("\nNo response received.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nSuccess: {result.Success}");
            if (!string.IsNullOrWhiteSpace(result.Message))
                Console.WriteLine($"Message: {result.Message}");
            if (!string.IsNullOrWhiteSpace(result.SecretName))
                Console.WriteLine($"Secret Name: {result.SecretName}");
            if (!string.IsNullOrWhiteSpace(result.SecretValue))
                Console.WriteLine($"Value: {result.SecretValue}");

            Console.ReadKey();
        }
    }
}
