# Configuration

## Configuration Management

---

## Environment Setup

### Development

```json
{
  "WhatsApp": {
    "PhoneNumberId": "test_phone_id",
    "BusinessAccountId": "test_account_id",
    "AccessToken": "test_token",
    "ApiVersion": "v18.0",
    "EnableLogging": true
  }
}
```

### Production

Use Azure Key Vault:

```csharp
var keyVaultUrl = new Uri("https://your-keyvault.vault.azure.net/");
var credential = new DefaultAzureCredential();

var config = new ConfigurationBuilder()
    .AddAzureKeyVault(keyVaultUrl, credential)
    .Build();
```

---

## Configuration Parameters

| Parameter | Required | Default | Notes |
|-----------|----------|---------|-------|
| PhoneNumberId | Yes | - | From WhatsApp setup |
| BusinessAccountId | Yes | - | From Meta platform |
| AccessToken | Yes | - | Long-lived token |
| ApiVersion | No | v18.0 | WhatsApp API version |
| TimeoutSeconds | No | 30 | API call timeout |
| EnableLogging | No | true | Enable detailed logging |

---

## Security

### Best Practices

1. **Never hardcode secrets**
   ```csharp
   // ❌ Bad
   var token = "eaab...secret...token";
   
   // ✅ Good
   var token = configuration["WhatsApp:AccessToken"];
   ```

2. **Use Azure Key Vault for production**
   ```csharp
   services.AddAzureKeyVault();
   ```

3. **Rotate tokens regularly** (every 60 days)

4. **Use environment variables**
   ```bash
   export WhatsApp__AccessToken=your_token
   ```

---

## Logging Configuration

### Log Levels

- **Debug**: Detailed diagnostic info
- **Information**: General informational
- **Warning**: Potential issues
- **Error**: Error occurred
- **Critical**: Critical errors

### Example Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "D365.WhatsApp": "Debug",
      "Microsoft": "Warning"
    }
  }
}
```

---

## Connection Management

### Retry Policy

```csharp
services.AddHttpClient<IWhatsAppService, WhatsAppService>()
    .AddTransientHttpErrorPolicy(p =>
        p.WaitAndRetryAsync(retryCount: 3,
            sleepDurationProvider: _ => TimeSpan.FromSeconds(2)));
```

### Timeout Configuration

```csharp
_httpClient.Timeout = TimeSpan.FromSeconds(30);
```

---

## Database Configuration

D365 message log table configuration:

```sql
CREATE TABLE WhatsAppMessageLog
(
    MessageLogId UNIQUEIDENTIFIER PRIMARY KEY,
    PurchaseOrderNumber NVARCHAR(50),
    RecipientPhoneNumber NVARCHAR(20),
    VendorName NVARCHAR(100),
    MessageType NVARCHAR(50),
    Status NVARCHAR(20),
    MessageId NVARCHAR(100),
    CreatedDate DATETIME,
    SentDate DATETIME,
    ErrorMessage NVARCHAR(MAX)
)
```

---

## Related Pages

- [[Setup Instructions|Setup-Instructions]] - Setup guide
- [[Security Model|Security-Model]] - Security details
- [[Monitoring|Monitoring]] - Monitoring setup
- [[Environment|Environment]] - Environment configuration

