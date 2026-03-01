# AzureServicesAPI

A centralized Azure infrastructure API built with ASP.NET Core that provides secure, unified access to Azure services for consuming applications. Rather than each application implementing its own Azure SDK integrations, they authenticate once and delegate all Azure communication through this API.

## Architecture

```
Consuming App A							  ->   Azure Key Vault
Consuming App B   ->   AzureServicesAPI   ->   Azure Service Bus
Consuming App C                           ->   Azure Blob Storage
```

Consuming applications authenticate via Entra (Azure AD) using client credentials and receive a JWT token. That token is used to call this API, which handles all Azure service communication internally using its own managed identity.

## Tech Stack

- .NET 8 / ASP.NET Core
- Azure Key Vault
- Azure Service Bus
- Azure Blob Storage
- Microsoft Identity Web (JWT validation)
- Azure.Identity (DefaultAzureCredential)

## Project Structure

```
AzureServicesAPI/
  Controllers/         HTTP endpoints, request validation, response codes
  Managers/            Business logic, model mapping, error handling
  Services/            Azure SDK calls
  Interfaces/          IKeyVaultManager, IKeyVaultService, etc.
  Helpers/             SettingsHelper, ContentTypeHelper
  DataModels/          Internal request/transfer models (not exposed publicly)

Models.Shared/
  Base/                ModelBase with Success and Message properties
  KeyVault/            KeyVault response model
  ServiceBus/          ServiceBus and ServiceBusList response models
  BlobStorage/         BlobStorage and BlobStorageList response models
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

Generate a unique GUID for the `id` field using PowerShell:
```powershell
New-Guid
```

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

### Key Vault Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/keyvault/{secretName}` | Retrieve a secret by name |
| POST | `/api/keyvault` | Create or update a secret |
| DELETE | `/api/keyvault/{secretName}` | Delete a secret |

---

## Azure Service Bus Setup

### Step 1 - Create a Service Bus Namespace

1. In the Azure portal search for **Service Bus** and select **Create**
2. Select your existing resource group
3. Give the namespace a globally unique name (e.g. `yourname-sb`)
4. Select the same region as your other resources
5. Select **Basic** pricing tier for queue support
6. Hit **Review + Create**

> Note: Basic tier supports queues only. To use topics upgrade to Standard tier in the namespace settings. No resources need to be recreated.

### Step 2 - Create a Queue

1. Go to your Service Bus namespace
2. Click **Queues** in the left menu then **+ Queue**
3. Give it a name (e.g. `main-queue`)
4. Leave all other settings as default
5. Hit **Create**

### Step 3 - Get the Connection String

1. In your Service Bus namespace click **Shared Access Policies**
2. Click **RootManageSharedAccessKey**
3. Copy the **Primary Connection String**

### Step 4 - Store Connection String in Key Vault

Rather than storing the Service Bus connection string directly in config, it is stored as a secret in Key Vault. Your config only stores the secret namemaking it safe to commit.

1. Go to your Key Vault and click **Secrets**
2. Click **Generate/Import**
3. Name it `servicebus-connection-string`
4. Paste the Primary Connection String as the value
5. Hit **Create**

### Step 5 - Grant Service Bus Access

Your account needs permission to send and receive messages during local development.

1. Go to your Service Bus namespace in the Azure portal
2. Click **Access Control (IAM)** then **Add role assignment**
3. Search for and assign **Azure Service Bus Data Owner** to your account
4. Hit **Review + assign**

Allow a few minutes for the role assignment to propagate.

### Step 6 - Update Local Settings

Confirm your `appsettings.Development.json` has the Service Bus section:

```json
{
  "ServiceBus": {
    "ConnectionKey": "servicebus-connection-string",
    "QueueName": "main-queue"
  }
}
```

`ConnectionKey` must match the secret name you created in Key Vault in Step 4.

### Service Bus Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/servicebus/send` | Send a message to the queue |
| GET | `/api/servicebus/peek/?{count}` | Peek at the next message without consuming it. Count defaulted to 1 but can peek up to 100 messages |
| GET | `/api/servicebus/receive` | Receive and complete a message atomically |
| GET | `/api/servicebus/deadletter` | Peek all messages in the dead letter queue |
| POST | `/api/servicebus/deadletter/resend/{sequenceNumber}` | Resend a specific dead letter message by sequence number |

### Service Bus Design Notes

**Receive and Complete** — receiving and completing a message happens in a single atomic operation. This is intentional. Since the API is stateless it cannot hold a message lock between requests. If your consuming application needs to process a message before acknowledging it, implement your own Service Bus listener using the connection string retrieved from Key Vault.

**Dead Letter Queue** — messages that fail processing repeatedly are automatically moved to the dead letter queue by Service Bus after exceeding the maximum delivery count (default 10). Use the dead letter endpoints to inspect and selectively resend failed messages by sequence number.

---

## Azure Blob Storage Setup

### Step 1 - Create a Storage Account

1. In the Azure portal search for **Storage Account** and select **Create**
2. Select your existing resource group
3. Give the storage account a globally unique name, lowercase letters and numbers only (e.g. `yournameinfrast`)
4. Select the same region as your other resources
5. Set **Performance** to Standard
6. Set **Redundancy** to Locally Redundant Storage (LRS)
7. Hit **Review + Create**

### Step 2 - Create a Container

1. Go to your Storage Account and click **Containers** in the left menu
2. Click **+ Container**
3. Give it a name (e.g. `general`)
4. Set public access level to **Private**
5. Hit **Create**

### Step 3 - Get the Connection String

1. Go to your Storage Account and click **Access keys** in the left menu
2. Click **Show** next to key1
3. Copy the **Connection string**

### Step 4 - Store Connection String in Key Vault

1. Go to your Key Vault and click **Secrets**
2. Click **Generate/Import**
3. Name it `blobstorage-connection-string`
4. Paste the connection string as the value
5. Hit **Create**

### Step 5 - Grant Blob Storage Access

Your account needs permission to read and write blobs during local development.

1. Go to your Storage Account in the Azure portal
2. Click **Access Control (IAM)** then **Add role assignment**
3. Search for and assign **Storage Blob Data Contributor** to your account
4. Hit **Review + assign**

Allow a few minutes for the role assignment to propagate.

### Step 6 - Update Local Settings

Add the Blob Storage section to your `appsettings.Development.json`:

```json
{
  "BlobStorage": {
    "ConnectionStringKey": "blobstorage-connection-string"
  }
}
```

`ConnectionStringKey` must match the secret name you created in Key Vault in Step 4.

### Blob Storage Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/blobstorage/upload` | Upload a file via multipart/form-data |
| GET | `/api/blobstorage/{containerName}/{blobName}` | Download a file as raw bytes |
| GET | `/api/blobstorage/{containerName}?prefix={prefix}` | List blobs in a container, optionally filtered by prefix |
| DELETE | `/api/blobstorage/{containerName}/{blobName}` | Delete a blob |

### Blob Storage Design Notes

**File transfer** — upload uses `multipart/form-data` and download returns raw bytes with the appropriate content type header.

**Upload fields** — the upload endpoint accepts `containerName`, `blobName`, `contentType` (optional), and `file` as form fields.

**Content types** — the API validates content types against a list of supported MIME types. If an invalid or missing content type is provided it defaults to `application/octet-stream`. Supported types include common image, document, text, audio, video, and archive formats.

**List vs Download** — the list endpoint returns metadata only (name, content type, size, last modified). Content is not included in list responses. Use the download endpoint to retrieve the content of a specific blob.

**Prefix filtering** — the list endpoint supports an optional `prefix` query parameter to filter blobs by name prefix. For example `?prefix=images/` returns only blobs whose names start with `images/`.

---

## Production Deployment

When deployed to Azure App Service, managed identities replace client secrets entirely. No credentials are stored anywhere.

1. Enable a system-assigned managed identity on your App Service
2. Grant the managed identity the following roles:
   - **Key Vault Secrets Officer** on your Key Vault
   - **Azure Service Bus Data Owner** on your Service Bus namespace
   - **Storage Blob Data Contributor** on your Storage Account
3. Add application settings in App Service Configuration:

```
Entra__Instance                    = https://login.microsoftonline.com/
Entra__TenantId                    = your-tenant-id
Entra__ClientId                    = your-client-id
Entra__Audience                    = api://your-client-id
KeyVault__VaultUri                 = https://your-keyvault-name.vault.azure.net/
ServiceBus__ConnectionStringKey    = servicebus-connection-string
ServiceBus__QueueName              = your-queue-name
BlobStorage__ConnectionStringKey   = blobstorage-connection-string
```

The double underscore `__` maps to nested JSON config automatically. No code changes needed between development and production.

---

## Security Notes

- Never commit `appsettings.Development.json` to source control
- Rotate client secrets regularly in development
- In production always use managed identities over client secrets
- Grant only the minimum required roles (Secrets User for read-only, Secrets Officer for read/write)
- All sensitive values including connection strings are stored in Key Vault — config files only hold secret names
