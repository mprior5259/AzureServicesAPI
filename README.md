# AzureServicesAPI

A centralized Azure infrastructure API built with ASP.NET Core that provides secure, unified access to Azure services for consuming applications. Rather than each application implementing its own Azure SDK integrations, they authenticate once and delegate all Azure communication through this API.

## Architecture

```
Consuming App A                           ->   Azure Key Vault
Consuming App B   ->   AzureServicesAPI   ->   Azure Service Bus (coming soon)
Consuming App C                           ->   Azure Blob Storage (coming soon)
                                          
```

Consuming applications authenticate via Entra (Azure AD) using client credentials and receive a JWT token. That token is used to call this API, which handles all Azure service communication internally using its own managed identity.

## Tech Stack

- .NET 8 / ASP.NET Core
- Azure Key Vault
- Azure Service Bus *(coming soon)*
- Azure Blob Storage *(coming soon)*
- Microsoft Identity Web (JWT validation)
- Azure.Identity (DefaultAzureCredential)

## Project Structure

```
AzureServicesAPI/
  Controllers/         HTTP endpoints, request validation, response codes
  Managers/            Business logic, model mapping, error handling
  Services/            Azure SDK calls
  Interfaces/          IKeyVaultManager, IKeyVaultService, etc.
  Helpers/             SettingsHelper (configuration)
  DataModels/          Internal request/transfer models (not exposed publicly)

Models.Shared/
  Base/                ModelBase with Success and Message properties
  KeyVault/            KeyVault response model
  Helpers/             ModelUtility for generic model mapping
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- An Azure subscription
- An Azure Entra (Azure AD) tenant
- Visual Studio 2022 or later


### Configuration

Create `appsettings.Development.json` in the `AzureServicesAPI` project root. This file is git-ignored and will never be committed.

Use the provided `appsettings.Development.example.json` as a template.

---

## Azure Key Vault Setup

### Step 1 - Create a Resource Group

In the Azure portal create a new resource group to contain all resources for this project. Recommended name: `az-serv-api`.

Keep all resources in the same region to avoid cross-region latency.

### Step 2 - Create the Key Vault

1. In the Azure portal search for **Key Vault** and select **Create**
2. Select your resource group from Step 1
3. Give it a globally unique name (e.g. `yourname-kv`)
4. Select the same region as your resource group
5. Leave all other settings as default and hit **Review + Create**

Once created, copy the **Vault URI** from the overview page. It will look like:
```
https://your-keyvault-name.vault.azure.net/
```

### Step 3 - Register the API in Entra

1. In the Azure portal go to **Entra ID** then **App Registrations**
2. Click **New Registration**
3. Name it `AzureServicesAPI`
4. Select **Single tenant**
5. Leave Redirect URI blank
6. Hit **Register**

From the overview page copy and save:
- **Application (client) ID**
- **Directory (tenant) ID**

### Step 4 - Expose an App Role

The API needs an app role so consuming applications can be granted permission to call it.

1. In the `AzureServicesAPI` registration click **Manifest**
2. Find the `appRoles` array and replace it with:

```json
"appRoles": [
    {
        "allowedMemberTypes": [
            "Application"
        ],
        "description": "Allows the application to access AzureServicesAPI",
        "displayName": "Access AzureServicesAPI",
        "id": "YOUR-UNIQUE-GUID",
        "isEnabled": true,
        "origin": "Application",
        "value": "access_as_application"
    }
]
```

Generate a unique GUID for the `id`.

3. Hit **Save**

### Step 5 - Register a Client Application in Entra

This represents any application that will call your Infrastructure API.

1. Go back to **App Registrations** and create a new registration
2. Name it `AzureServicesAPI-Client` (or your consuming app name)
3. Select **Single tenant**
4. Leave Redirect URI blank
5. Hit **Register**

Copy the **Application (client) ID** for this registration.

**Create a client secret:**
1. Click **Certificates and Secrets** then **New client secret**
2. Give it a description and expiry
3. Copy the secret **Value** immediately — it is only shown once

**Grant permission to call the API:**
1. Click **API Permissions** then **Add a permission**
2. Select **My APIs** then find `AzureServicesAPI`
3. Select **Application permissions** and check `access_as_application`
4. Hit **Add permissions**
5. Click **Grant admin consent for your tenant** and confirm

### Step 6 - Grant Key Vault Access

Your account needs permission to read and write secrets during local development.

1. Go to your Key Vault in the Azure portal
2. Click **Access Control (IAM)** then **Add role assignment**
3. Search for and assign **Key Vault Secrets Officer** to your account
4. Hit **Review + assign**

Allow a few minutes for the role assignment to propagate.

### Step 7 - Configure Local Settings

Create `appsettings.Development.json` in the `AzureServicesAPI` project:

```json
{
  "Entra": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "your-directory-tenant-id",
    "ClientId": "your-azureservicesapi-client-id",
    "Audience": "api://your-azureservicesapi-client-id",
    "Authority": "https://login.microsoftonline.com/your-tenant-id"
  },
  "KeyVault": {
    "VaultUri": "https://your-keyvault-name.vault.azure.net/"
  }
}
```

### Step 8 - Add Your Azure Account to Visual Studio

`DefaultAzureCredential` uses your Visual Studio account to authenticate to Key Vault locally.

1. In Visual Studio go to **Tools** then **Options**
2. Find **Azure Service Authentication**
3. Add and sign in with the same account used for your Azure portal

### Step 9 - Run and Test

Run the project. Swagger UI will open at `https://localhost:{port}/swagger`.

**Get a test token** using PowerShell:

```powershell
$body = @{
    grant_type    = "client_credentials"
    client_id     = "your-azureservicesapi-client-id"
    client_secret = "your-client-secret"
    scope         = "api://your-azureservicesapi-application-id/.default"
}

$response = Invoke-RestMethod `
    -Method Post `
    -Uri "https://login.microsoftonline.com/your-tenant-id/oauth2/v2.0/token" `
    -ContentType "application/x-www-form-urlencoded" `
    -Body $body

$response.access_token
```

**Authorize in Swagger:**
1. Click the **Authorize** button in Swagger UI
2. Enter `Bearer your-token-value`
3. Hit **Authorize**

You can now call the Key Vault endpoints.

### Key Vault Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/keyvault/{secretName}` | Retrieve a secret by name |
| POST | `/api/keyvault` | Create or update a secret |
| DELETE | `/api/keyvault/{secretName}` | Delete a secret |

---

## Service Bus Setup

*Coming soon*

---

## Blob Storage Setup

*Coming soon*

---

## Production Deployment

When deployed to Azure App Service, managed identities replace client secrets entirely. No credentials are stored anywhere.

1. Enable a system-assigned managed identity on your App Service
2. Grant the managed identity **Key Vault Secrets Officer** role on your Key Vault
3. Add application settings in App Service Configuration:

```
Entra__Instance     = https://login.microsoftonline.com/
Entra__TenantId     = your-tenant-id
Entra__ClientId     = your-client-id
Entra__Audience     = api://your-client-id
KeyVault__VaultUri  = https://your-keyvault-name.vault.azure.net/
```

The double underscore `__` maps to nested JSON config automatically. No code changes needed between development and production.

---

## Security Notes

- Never commit `appsettings.Development.json` to source control
- Rotate client secrets regularly in development
- In production always use managed identities over client secrets
- Grant only the minimum required roles (Secrets User for read-only, Secrets Officer for read/write)
