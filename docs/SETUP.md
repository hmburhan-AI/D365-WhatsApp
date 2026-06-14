# Setup Guide - D365 WhatsApp Integration

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [WhatsApp Setup](#whatsapp-setup)
3. [D365 Configuration](#d365-configuration)
4. [Code Deployment](#code-deployment)
5. [Testing](#testing)
6. [Troubleshooting](#troubleshooting)

## Prerequisites

Before you begin, ensure you have:

- **Dynamics 365 Finance & Operations** (version 10.0+)
- **Meta WhatsApp Business Account** with approval
- **WhatsApp Business Phone Number**
- **Meta Business Platform Account**
- **Azure DevOps** or similar for source control
- **.NET Framework 4.7.2+** (for D365 plugins)
- **Visual Studio** 2019 or later (for development)
- **Administrator access** to D365 and Meta platforms

## WhatsApp Setup

### Step 1: Create Meta Business Account

1. Go to [Meta Business Platform](https://business.facebook.com)
2. Click on **Settings** → **Business Settings**
3. Note your **Business Account ID**

### Step 2: Set Up WhatsApp Business Phone Number

1. Navigate to **WhatsApp** → **Phone Numbers**
2. Click **Add Phone Number**
3. Verify your business phone number (SMS or call verification)
4. Note the **Phone Number ID** and **Display Name**

### Step 3: Generate Access Token

1. Go to **Settings** → **System User**
2. Create a new System User for WhatsApp integration
3. Navigate to **Apps and Websites**
4. Generate a long-lived **Access Token** (valid for 60 days)
5. Copy and securely store the token

**Important**: Do NOT share this token. Store it securely in Azure Key Vault or secure configuration.

### Step 4: Register Webhook (Optional)

For receiving delivery confirmations and webhooks:

1. Go to **WhatsApp** → **Configuration**
2. Set up webhook URL: `https://your-domain.com/api/whatsapp/webhook`
3. Generate and verify webhook token

## D365 Configuration

### Step 1: Add Vendor Fields

You need to add a custom field to store the WhatsApp phone number:

1. Open **Accounts Payable** → **Vendors** → **All Vendors**
2. Click on **Vendor** form
3. Open form designer
4. Add new field: **WhatsAppPhoneNumber** (Text, E.164 format)
5. Publish customizations
6. Save and close

### Step 2: Create WhatsApp Message Log Table

For tracking message delivery status:

1. Create a new table: **WhatsAppMessageLog**
2. Add fields:
   - **MessageId** (Text)
   - **PurchaseOrderNumber** (Text)
   - **RecipientPhoneNumber** (Text)
   - **VendorName** (Text)
   - **MessageType** (Text)
   - **Status** (Choice: Pending, Sent, Delivered, Failed)
   - **ResponseJson** (Text)
   - **CreatedDate** (DateTime)
   - **SentDate** (DateTime)
   - **ErrorMessage** (Text)

### Step 3: Register Plugin

1. Deploy the **POConfirmationPlugin.dll** to D365
2. Register the plugin on **PurchaseOrderTable** entity
3. Set execution stage: **Post Operation**
4. Set execution mode: **Asynchronous**
5. Create step configuration

## Code Deployment

### Option 1: Manual Deployment

```bash
# Clone the repository
git clone https://github.com/hmburhan-AI/D365-WhatsApp.git
cd D365-WhatsApp

# Restore NuGet packages
restores

# Build the solution
dotnet build src/D365/PurchaseOrder/D365.WhatsApp.PurchaseOrder.csproj

# Deploy plugin to D365
# Use Plugin Registration Tool (PRT)
PluginRegistrationTool.exe
```

### Option 2: Using Azure DevOps Pipeline

```yaml
trigger:
  - main

pool:
  vmImage: 'windows-latest'

steps:
- task: DotNetCoreCLI@2
  inputs:
    command: 'build'
    arguments: '--configuration Release'

- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    arguments: '--configuration Release'

- task: CopyFiles@2
  inputs:
    SourceFolder: '$(Build.ArtifactStagingDirectory)'
    Contents: '**/*.dll'
    TargetFolder: '$(Build.ArtifactStagingDirectory)'

- task: PublishBuildArtifacts@1
  inputs:
    PathtoPublish: '$(Build.ArtifactStagingDirectory)'
    ArtifactName: 'drop'
```

## Configuration Setup

### Step 1: Update Configuration Files

**appsettings.json**:
```json
{
  "WhatsApp": {
    "PhoneNumberId": "YOUR_PHONE_NUMBER_ID",
    "BusinessAccountId": "YOUR_BUSINESS_ACCOUNT_ID",
    "AccessToken": "YOUR_ACCESS_TOKEN"
  },
  "D365": {
    "InstanceUrl": "https://your-instance.crm.dynamics.com"
  }
}
```

### Step 2: Secure Configuration (Production)

Use Azure Key Vault:

```csharp
var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());

var config = builder.Build();
```

## Testing

### Test 1: Validate Configuration

```csharp
var config = WhatsAppConfig.Load(configuration);
assert.NotNull(config.PhoneNumberId);
assert.NotNull(config.AccessToken);
```

### Test 2: Test Phone Number Validation

```csharp
var isValid = await whatsAppService.ValidatePhoneNumberAsync("+11234567890");
assert.True(isValid);
```

### Test 3: Send Test Message

```csharp
var po = new PurchaseOrderModel
{
    PurchaseOrderId = "PO-001",
    VendorName = "Test Vendor",
    VendorPhoneNumber = "+11234567890",
    TotalAmount = 1000,
    CurrencyCode = "USD"
};

var request = new POConfirmationRequest { PurchaseOrder = po };
var result = await poConfirmationService.SendPOConfirmationAsync(request);
assert.True(result);
```

### Test 4: Manual Test in D365

1. Create a new Purchase Order in D365
2. Add vendor with WhatsApp phone number
3. Confirm the PO
4. Check WhatsAppMessageLog for status
5. Verify message received on vendor's WhatsApp

## Troubleshooting

### Issue: "Invalid Phone Number"

**Solution**: Ensure phone number is in E.164 format (+country_codephonenumber)

```
Valid: +11234567890
Invalid: 1-123-456-7890
```

### Issue: "Authentication Failed"

**Solution**: Verify access token

```bash
curl -X GET "https://graph.instagram.com/v18.0/me?access_token=YOUR_TOKEN"
```

### Issue: "Message Not Sent"

**Check**:
1. Phone number ID is correct
2. Phone number is verified in Meta platform
3. Vendor has WhatsApp account
4. Rate limits not exceeded

### Issue: "Plugin Not Triggering"

**Check**:
1. Plugin is registered and active
2. Entity name matches (PurchaseOrderTable)
3. Check plugin trace logs in D365

## Next Steps

- [API Reference](API_REFERENCE.md)
- [Developer Guide](DEVELOPER_GUIDE.md)
- [Configuration Guide](CONFIGURATION.md)
