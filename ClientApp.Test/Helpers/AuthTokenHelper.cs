using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Test.Helpers
{
    internal static class AuthTokenHelper
    {
        internal static async Task<string> AcquireAccessToken(IConfigurationRoot configuration)
        {
            var tenantId = configuration["Entra:TenantId"]!;
            var clientId = configuration["Entra:ClientId"]!;
            var clientSecret = configuration["Entra:ClientSecret"]!;
            var scope = configuration["Entra:Scope"]!;

            // Acquire token
            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                .Build();

            var result = await app.AcquireTokenForClient(new[] { scope }).ExecuteAsync();
            return result.AccessToken;
        }
    }
}
