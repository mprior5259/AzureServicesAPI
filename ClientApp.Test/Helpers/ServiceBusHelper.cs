using Models.Shared.Models.AzureServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Test.Helpers
{
    internal static class ServiceBusHelper
    {
        internal static async Task SendMessage(AzureServicesApiProxy apiClient)
        {
            Console.Clear();
            Console.WriteLine("=== Send Message ===\n");

            Console.Write("Enter message body: ");
            var body = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(body))
            {
                Console.WriteLine("Message body cannot be empty.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter subject (optional, press Enter to skip): ");
            var subject = Console.ReadLine();

            Console.Write("Enter correlation ID (optional, press Enter to skip): ");
            var correlationId = Console.ReadLine();

            var result = await apiClient.SendMessageAsync(
                body,
                string.IsNullOrWhiteSpace(subject) ? null : subject,
                string.IsNullOrWhiteSpace(correlationId) ? null : correlationId
            );

            PrintResult(result);
        }

        internal static async Task PeekMessages(AzureServicesApiProxy apiClient)
        {
            Console.Clear();
            Console.WriteLine("=== Peek Messages ===\n");

            Console.Write("How many messages to peek (default 1): ");
            var input = Console.ReadLine();
            var count = string.IsNullOrWhiteSpace(input) ? 1 : int.TryParse(input, out var parsed) ? parsed : 1;

            var result = await apiClient.PeekMessagesAsync(count);
            PrintListResult(result);
        }

        internal static async Task ReceiveMessage(AzureServicesApiProxy apiClient)
        {
            Console.Clear();
            Console.WriteLine("=== Receive and Complete ===\n");

            var result = await apiClient.ReceiveMessageAsync();
            PrintResult(result);
        }

        internal static async Task PeekDeadLetters(AzureServicesApiProxy apiClient)
        {
            Console.Clear();
            Console.WriteLine("=== Peek Dead Letter Queue ===\n");

            var result = await apiClient.PeekDeadLettersAsync();
            PrintListResult(result);
        }

        internal static async Task ResendDeadLetter(AzureServicesApiProxy apiClient)
        {
            Console.Clear();
            Console.WriteLine("=== Resend Dead Letter Message ===\n");

            Console.Write("Enter sequence number: ");
            var input = Console.ReadLine();

            if (!long.TryParse(input, out var sequenceNumber))
            {
                Console.WriteLine("Invalid sequence number.");
                Console.ReadKey();
                return;
            }

            var result = await apiClient.ResendDeadLetterAsync(sequenceNumber);
            PrintResult(result);
        }

        private static void PrintResult(ServiceBus? result)
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
            if (!string.IsNullOrWhiteSpace(result.MessageId))
                Console.WriteLine($"Message ID: {result.MessageId}");
            if (!string.IsNullOrWhiteSpace(result.Body))
                Console.WriteLine($"Body: {result.Body}");
            if (!string.IsNullOrWhiteSpace(result.Subject))
                Console.WriteLine($"Subject: {result.Subject}");
            if (!string.IsNullOrWhiteSpace(result.CorrelationId))
                Console.WriteLine($"Correlation ID: {result.CorrelationId}");
            if (result.SequenceNumber.HasValue)
                Console.WriteLine($"Sequence Number: {result.SequenceNumber}");
            if (result.EnqueuedTime.HasValue)
                Console.WriteLine($"Enqueued Time: {result.EnqueuedTime}");
            if (result.DeliveryCount.HasValue)
                Console.WriteLine($"Delivery Count: {result.DeliveryCount}");

            Console.ReadKey();
        }

        private static void PrintListResult(ServiceBusList? result)
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

            if (result.Messages != null && result.Messages.Any())
            {
                Console.WriteLine($"Count: {result.Count}\n");
                foreach (var message in result.Messages)
                {
                    Console.WriteLine("---");
                    if (!string.IsNullOrWhiteSpace(message.MessageId))
                        Console.WriteLine($"  Message ID: {message.MessageId}");
                    if (!string.IsNullOrWhiteSpace(message.Body))
                        Console.WriteLine($"  Body: {message.Body}");
                    if (!string.IsNullOrWhiteSpace(message.Subject))
                        Console.WriteLine($"  Subject: {message.Subject}");
                    if (message.SequenceNumber.HasValue)
                        Console.WriteLine($"  Sequence Number: {message.SequenceNumber}");
                    if (message.EnqueuedTime.HasValue)
                        Console.WriteLine($"  Enqueued Time: {message.EnqueuedTime}");
                    if (message.DeliveryCount.HasValue)
                        Console.WriteLine($"  Delivery Count: {message.DeliveryCount}");
                }
            }

            Console.ReadKey();
        }
    }
}
