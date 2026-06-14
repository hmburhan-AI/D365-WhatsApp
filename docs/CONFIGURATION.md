# Configuration Guide

## Environment Setup

### Local Development

1. **Create `appsettings.Development.json`**:

```json
{
  "WhatsApp": {
    "PhoneNumberId": "your_test_phone_number_id",
    "BusinessAccountId": "your_test_business_account_id",
    "AccessToken": "your_test_access_token",
    "ApiVersion": "v18.0",
    "TimeoutSeconds": 30,
    "EnableLogging": true,
    "LogFilePath": "logs/whatsapp-dev.log"
  },
  "D365": {
    "InstanceUrl": "https://your-dev-instance.crm.dynamics.com",
    "ClientId": "your_client_id",
    "ClientSecret": "your_client_secret",
    "TenantId": "your_tenant_id"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

### Production Setup

1. **Use Azure Key Vault**:

```csharp
var keyVaultUrl = new Uri("https://your-keyvault.vault.azure.net/");
var credential = new DefaultAzureCredential();

var config = new ConfigurationBuilder()
    .AddAzureKeyVault(keyVaultUrl, credential)
    .Build();
```

2. **Store Secrets in Key Vault**:

- `WhatsApp--PhoneNumberId`
- `WhatsApp--BusinessAccountId`
- `WhatsApp--AccessToken`
- `D365--ClientSecret`

## WhatsApp Configuration

### Parameter Reference

| Parameter | Value | Notes |
|-----------|-------|-------|
| PhoneNumberId | 12345678901234 | Found in Meta Business Platform |
| BusinessAccountId | 9876543210 | Found in Meta Business Settings |
| AccessToken | EAAB...abc | Generate from System User |
| ApiVersion | v18.0 | Latest stable version |

## D365 Configuration

### Step 1: Add Custom Fields to Vendor

1. Open **Accounts Payable** → **Vendors**
2. Select a vendor record
3. Click on the form designer
4. Add new field: `WhatsAppPhoneNumber`
   - Type: Text
   - Format: Phone
   - Length: 20
5. Publish

### Step 2: Create Message Log Table

Create a new table with these fields:

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
    ResponseJson NVARCHAR(MAX),
    CreatedDate DATETIME,
    SentDate DATETIME,
    ErrorMessage NVARCHAR(MAX)
)
```

## Dependency Injection Setup

### C# Configuration Example

```csharp
public void ConfigureServices(IServiceCollection services)
{
    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    var whatsAppConfig = WhatsAppConfig.Load(config);

    // Register WhatsApp Service
    services.AddHttpClient<IWhatsAppService, WhatsAppService>()
        .ConfigureHttpClient(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(whatsAppConfig.TimeoutSeconds);
        })
        .AddTransientHttpErrorPolicy(p =>
            p.WaitAndRetryAsync(retryCount: 3,
                sleepDurationProvider: _ => TimeSpan.FromSeconds(2)));

    // Register PO Confirmation Service
    services.AddScoped<IPOConfirmationService, POConfirmationService>();

    // Register Logging
    services.AddLogging(config =>
    {
        config.AddConsole();
        config.AddDebug();
        if (whatsAppConfig.EnableLogging)
        {
            config.AddFile(whatsAppConfig.LogFilePath);
        }
    });
}
```

## Logging Configuration

### Log Levels

- **Debug**: Detailed diagnostic information
- **Information**: General informational messages
- **Warning**: Warning messages for potential issues
- **Error**: Error messages
- **Critical**: Critical errors

### Example Log Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "D365.WhatsApp": "Debug",
      "Microsoft": "Warning",
      "System": "Warning"
    },
    "Console": {
      "IncludeScopes": true
    }
  }
}
```

## Testing Configuration

### Test Environment Setup

```csharp
public class WhatsAppServiceTests
{
    private IWhatsAppService _service;
    private Mock<HttpClient> _httpClientMock;
    private ILogger<WhatsAppService> _logger;

    [SetUp]
    public void Setup()
    {
        _httpClientMock = new Mock<HttpClient>();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<WhatsAppService>();

        _service = new WhatsAppService(
            _httpClientMock.Object,
            "test_phone_id",
            "test_token",
            "v18.0",
            _logger);
    }

    [Test]
    public async Task SendMessage_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var message = new WhatsAppMessage
        {
            RecipientPhoneNumber = "+11234567890",
            MessageType = "text",
            Text = new TextMessage { Body = "Test" }
        };

        // Act
        var result = await _service.SendMessageAsync(message);

        // Assert
        Assert.NotNull(result);
        Assert.IsNotEmpty(result.Messages);
    }
}
```

## Security Considerations

### 1. Access Token Management

- Never hardcode tokens
- Use environment variables or Key Vault
- Rotate tokens regularly (every 60 days)
- Monitor token usage

### 2. Phone Number Privacy

- Encrypt phone numbers at rest
- Use HTTPS for all communications
- Log phone numbers as masked (***-****-7890)
- Comply with GDPR/privacy regulations

### 3. API Rate Limiting

```csharp
public class RateLimitingPolicy
{
    private const int MaxRequestsPerMinute = 60;
    private DateTime _lastResetTime = DateTime.UtcNow;
    private int _requestCount = 0;

    public bool IsAllowed()
    {
        if (DateTime.UtcNow - _lastResetTime > TimeSpan.FromMinutes(1))
        {
            _requestCount = 0;
            _lastResetTime = DateTime.UtcNow;
        }

        if (_requestCount < MaxRequestsPerMinute)
        {
            _requestCount++;
            return true;
        }

        return false;
    }
}
```

## Monitoring & Alerts

### Key Metrics to Monitor

1. **Message Success Rate**
   ```sql
   SELECT COUNT(*) as Total,
          SUM(CASE WHEN Status = 'Sent' THEN 1 ELSE 0 END) as Sent,
          (SUM(CASE WHEN Status = 'Sent' THEN 1 ELSE 0 END) * 100.0 / COUNT(*)) as SuccessRate
   FROM WhatsAppMessageLog
   WHERE CreatedDate > DATEADD(day, -1, GETDATE())
   ```

2. **Average Response Time**
   ```sql
   SELECT AVG(DATEDIFF(SECOND, CreatedDate, SentDate)) as AvgResponseTime
   FROM WhatsAppMessageLog
   WHERE Status = 'Sent'
   ```

3. **Error Rate**
   ```sql
   SELECT COUNT(*) as ErrorCount,
          COUNT(*) * 100.0 / (SELECT COUNT(*) FROM WhatsAppMessageLog) as ErrorPercentage
   FROM WhatsAppMessageLog
   WHERE Status = 'Failed'
   ```
