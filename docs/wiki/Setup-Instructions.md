# Setup Instructions

## Complete Setup Guide

Detailed step-by-step setup for D365 WhatsApp integration.

---

## Phase 1: WhatsApp Setup

### 1.1 Create Meta Business Account

1. Visit [Meta Business Platform](https://business.facebook.com)
2. Click **Create Account**
3. Enter business information
4. Verify email

### 1.2 Register Phone Number

1. Navigate to **WhatsApp** → **Phone Numbers**
2. Click **Add Phone Number**
3. Enter business phone
4. Complete SMS verification
5. Copy **Phone Number ID**

### 1.3 Generate Access Token

1. Go to **Settings** → **System User**
2. Create new system user
3. Navigate to **Apps and Websites**
4. Generate long-lived **Access Token**
5. Store securely in Azure Key Vault

### 1.4 Set Up Webhook (Optional)

1. Go to **WhatsApp** → **Configuration**
2. Set webhook URL: `https://your-domain.com/api/whatsapp/webhook`
3. Generate webhook token
4. Verify webhook

---

## Phase 2: D365 Configuration

### 2.1 Add Vendor Fields

1. Open **Accounts Payable** → **Vendors**
2. Click form designer
3. Add field: **WhatsAppPhoneNumber**
   - Type: Text
   - Format: Phone
   - Length: 20
4. Publish

### 2.2 Create Message Log Table

1. Create new table: **WhatsAppMessageLog**
2. Add fields:
   - MessageId (Text)
   - PurchaseOrderNumber (Text)
   - RecipientPhoneNumber (Text)
   - VendorName (Text)
   - MessageType (Text)
   - Status (Choice: Pending/Sent/Delivered/Failed)
   - CreatedDate (DateTime)
   - SentDate (DateTime)
   - ErrorMessage (Text)

### 2.3 Register Plugin

1. Use Plugin Registration Tool
2. Deploy **POConfirmationPlugin.dll**
3. Register on **PurchaseOrderTable** entity
4. Set:
   - Event: Create/Update
   - Stage: Post Operation
   - Mode: Asynchronous

---

## Phase 3: Code Deployment

### 3.1 Clone Repository

```bash
git clone https://github.com/hmburhan-AI/D365-WhatsApp.git
cd D365-WhatsApp
```

### 3.2 Configure Settings

```bash
cp config/appsettings.json config/appsettings.Development.json
```

Edit `appsettings.Development.json`:

```json
{
  "WhatsApp": {
    "PhoneNumberId": "YOUR_PHONE_ID",
    "BusinessAccountId": "YOUR_ACCOUNT_ID",
    "AccessToken": "YOUR_TOKEN",
    "ApiVersion": "v18.0"
  },
  "D365": {
    "InstanceUrl": "https://your-instance.crm.dynamics.com"
  }
}
```

### 3.3 Build Solution

```bash
dotnet restore
dotnet build --configuration Release
```

### 3.4 Deploy Plugin

1. Use Plugin Registration Tool
2. Connect to D365
3. Upload plugin DLL from `bin/Release/`
4. Register plugin steps
5. Verify registration

---

## Phase 4: Testing

### 4.1 Unit Tests

```bash
dotnet test
```

### 4.2 Manual Testing

1. Create test PO in D365
2. Add vendor with test WhatsApp number
3. Confirm PO
4. Wait 5-10 seconds
5. Check vendor WhatsApp
6. Verify message log in D365

### 4.3 Verify Setup

- [ ] WhatsApp message received
- [ ] Message log status: "Sent"
- [ ] Message ID stored
- [ ] No errors in plugin trace log

---

## Configuration Files

### appsettings.json (Production)

```json
{
  "WhatsApp": {
    "PhoneNumberId": "PRODUCTION_PHONE_ID",
    "BusinessAccountId": "PRODUCTION_ACCOUNT_ID",
    "AccessToken": "PRODUCTION_TOKEN",
    "ApiVersion": "v18.0",
    "TimeoutSeconds": 30
  },
  "D365": {
    "InstanceUrl": "https://prod-instance.crm.dynamics.com"
  }
}
```

### Using Azure Key Vault

```csharp
var keyVaultUrl = new Uri("https://your-keyvault.vault.azure.net/");
var credential = new DefaultAzureCredential();

var config = new ConfigurationBuilder()
    .AddAzureKeyVault(keyVaultUrl, credential)
    .Build();
```

---

## Troubleshooting

### Plugin Not Triggering

- [ ] Plugin is registered and active
- [ ] Entity name matches: `PurchaseOrderTable`
- [ ] Event is Post Operation
- [ ] Check plugin trace logs

### Messages Not Sending

- [ ] WhatsApp credentials are correct
- [ ] Access token hasn't expired
- [ ] Vendor phone in E.164 format
- [ ] Network connectivity verified

### Configuration Errors

- [ ] All required keys present
- [ ] No hardcoded secrets
- [ ] Environment variables set

---

## Next Steps

1. [[Configuration|Configuration]] - Advanced configuration
2. [[D365 Customization|D365-Customization]] - D365 specific settings
3. [[Testing Guide|Testing-Guide]] - Comprehensive testing
4. [[Monitoring|Monitoring]] - Set up monitoring

