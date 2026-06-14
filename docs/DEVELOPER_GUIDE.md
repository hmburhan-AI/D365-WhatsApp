# Developer Guide

## Table of Contents
1. [Project Structure](#project-structure)
2. [Development Environment](#development-environment)
3. [Building & Testing](#building--testing)
4. [Code Standards](#code-standards)
5. [Contributing](#contributing)
6. [Troubleshooting](#troubleshooting)

## Project Structure

```
D365-WhatsApp/
├── src/
│   ├── D365/
│   │   ├── PurchaseOrder/
│   │   │   ├── Models/
│   │   │   │   ├── PurchaseOrderModel.cs
│   │   │   │   └── POConfirmationRequest.cs
│   │   │   ├── Services/
│   │   │   │   └── POConfirmationService.cs
│   │   │   └── D365.WhatsApp.PurchaseOrder.csproj
│   │   ├── Plugins/
│   │   │   └── POConfirmationPlugin.cs
│   │   └── D365.WhatsApp.D365.csproj
│   ├── WhatsApp/
│   │   ├── Models/
│   │   │   ├── WhatsAppMessage.cs
│   │   │   └── WhatsAppResponse.cs
│   │   ├── Services/
│   │   │   └── WhatsAppService.cs
│   │   └── D365.WhatsApp.Services.csproj
│   ├── Configuration/
│   │   └── WhatsAppConfig.cs
│   └── D365.WhatsApp.sln
├── tests/
│   ├── WhatsApp.Tests/
│   └── D365.Tests/
├── templates/
│   └── PO_Confirmation.template
├── config/
│   ├── appsettings.json
│   └── appsettings.Development.json
├── docs/
├── .gitignore
├── README.md
└── LICENSE
```

## Development Environment

### Prerequisites

- **Visual Studio 2019+** or **Visual Studio Code**
- **.NET Framework 4.7.2+**
- **NuGet Package Manager**
- **Git**
- **D365 Developer Toolkit**

### Setup Steps

1. **Clone Repository**:
```bash
git clone https://github.com/hmburhan-AI/D365-WhatsApp.git
cd D365-WhatsApp
```

2. **Restore NuGet Packages**:
```bash
dotnet restore
```

3. **Open Solution**:
```bash
start D365.WhatsApp.sln
```

4. **Configure Local Settings**:
```bash
cp config/appsettings.json config/appsettings.Development.json
# Edit appsettings.Development.json with your test credentials
```

5. **Build Solution**:
```bash
dotnet build --configuration Debug
```

## Building & Testing

### Build Solution

```bash
# Debug build
dotnet build --configuration Debug

# Release build
dotnet build --configuration Release
```

### Run Unit Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/WhatsApp.Tests/WhatsApp.Tests.csproj

# Run with code coverage
dotnet test /p:CollectCoverage=true
```

### Test Examples

#### Test 1: WhatsApp Service - Send Message

```csharp
[TestFixture]
public class WhatsAppServiceTests
{
    private IWhatsAppService _whatsAppService;
    private ILogger<WhatsAppService> _logger;

    [SetUp]
    public void Setup()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<WhatsAppService>();
        
        var httpClient = new HttpClient();
        _whatsAppService = new WhatsAppService(
            httpClient,
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
            Text = new TextMessage
            {
                Body = "Test message",
                PreviewUrl = true
            }
        };

        // Act
        var result = await _whatsAppService.SendMessageAsync(message);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Messages);
        Assert.IsNotEmpty(result.Messages);
        Assert.IsFalse(string.IsNullOrEmpty(result.Messages[0].MessageId));
    }

    [Test]
    public async Task ValidatePhoneNumber_WithValidFormat_ReturnsTrue()
    {
        // Act
        var result = await _whatsAppService.ValidatePhoneNumberAsync("+11234567890");

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task ValidatePhoneNumber_WithInvalidFormat_ReturnsFalse()
    {
        // Act
        var result = await _whatsAppService.ValidatePhoneNumberAsync("1-123-456-7890");

        // Assert
        Assert.IsFalse(result);
    }
}
```

#### Test 2: PO Confirmation Service

```csharp
[TestFixture]
public class POConfirmationServiceTests
{
    private IPOConfirmationService _service;
    private Mock<IWhatsAppService> _whatsAppServiceMock;
    private ILogger<POConfirmationService> _logger;

    [SetUp]
    public void Setup()
    {
        _whatsAppServiceMock = new Mock<IWhatsAppService>();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<POConfirmationService>();

        _service = new POConfirmationService(_whatsAppServiceMock.Object, _logger);
    }

    [Test]
    public async Task SendPOConfirmation_WithValidPO_ReturnsTrue()
    {
        // Arrange
        var po = new PurchaseOrderModel
        {
            PurchaseOrderId = "PO-001",
            VendorName = "Test Vendor",
            VendorPhoneNumber = "+11234567890",
            TotalAmount = 1000m,
            CurrencyCode = "USD"
        };

        var request = new POConfirmationRequest { PurchaseOrder = po };

        _whatsAppServiceMock
            .Setup(x => x.ValidatePhoneNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        _whatsAppServiceMock
            .Setup(x => x.SendMessageAsync(It.IsAny<WhatsAppMessage>()))
            .ReturnsAsync(new WhatsAppResponse
            {
                Messages = new List<MessageStatus>
                {
                    new MessageStatus { MessageId = "msg-123", Status = "accepted" }
                }
            });

        // Act
        var result = await _service.SendPOConfirmationAsync(request);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task GenerateMessage_WithPOData_ReturnsFormattedMessage()
    {
        // Arrange
        var po = new PurchaseOrderModel
        {
            PurchaseOrderId = "PO-001",
            VendorName = "ABC Supplies",
            PurchaseOrderDate = new DateTime(2026, 06, 14),
            TotalAmount = 5000m,
            CurrencyCode = "USD",
            DeliveryAddress = "123 Main St"
        };

        // Act
        var message = await _service.GenerateMessageAsync(po);

        // Assert
        Assert.IsNotNull(message);
        Assert.Contains("PO-001", message);
        Assert.Contains("ABC Supplies", message);
        Assert.Contains("5000", message);
    }
}
```

## Code Standards

### Naming Conventions

- **Classes**: PascalCase (e.g., `WhatsAppService`)
- **Methods**: PascalCase (e.g., `SendMessageAsync`)
- **Properties**: PascalCase (e.g., `RecipientPhoneNumber`)
- **Parameters**: camelCase (e.g., `phoneNumber`)
- **Private fields**: _camelCase (e.g., `_httpClient`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `MAX_RETRIES`)

### Code Style

```csharp
// Good: Clear variable names
public async Task<bool> SendPOConfirmationAsync(POConfirmationRequest request)
{
    if (request == null)
        throw new ArgumentNullException(nameof(request));

    var purchaseOrder = request.PurchaseOrder;
    var vendorPhoneNumber = purchaseOrder.VendorPhoneNumber;

    var isValidPhone = await ValidatePhoneNumberAsync(vendorPhoneNumber);
    if (!isValidPhone)
        return false;

    return await SendAsync(purchaseOrder);
}

// Good: Proper error handling
try
{
    await SendMessageAsync(message);
}
catch (ArgumentNullException ex)
{
    _logger.LogError($"Argument null: {ex.Message}");
    throw;
}
catch (Exception ex)
{
    _logger.LogError($"Unexpected error: {ex.Message}", ex);
    throw;
}

// Good: XML documentation
/// <summary>
/// Sends a WhatsApp message to the specified recipient
/// </summary>
/// <param name="message">Message to send</param>
/// <returns>Response with message ID and status</returns>
/// <exception cref="ArgumentNullException">If message is null</exception>
public async Task<WhatsAppResponse> SendMessageAsync(WhatsAppMessage message)
```

### Logging Best Practices

```csharp
// Log levels
_logger.LogDebug("Entering SendMessageAsync");          // Detailed diagnostic
_logger.LogInformation("Message sent to vendor");       // General info
_logger.LogWarning("Slow response from API");          // Warning
_logger.LogError("Failed to send message", ex);        // Error with exception
_logger.LogCritical("Service unavailable");            // Critical issue

// Avoid logging sensitive data
var maskedPhone = "***-***-" + phoneNumber.Substring(phoneNumber.Length - 4);
_logger.LogInformation($"Sending to: {maskedPhone}");
```

## Contributing

### Branch Naming

- `feature/po-confirmation` - New feature
- `bugfix/issue-description` - Bug fix
- `docs/update-readme` - Documentation
- `refactor/improve-service` - Code refactoring

### Commit Messages

```
<type>: <subject>

<body>

<footer>
```

Types:
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation
- `style:` - Code style
- `refactor:` - Code refactoring
- `test:` - Tests
- `chore:` - Build/tooling

Example:
```
feat: Add PO confirmation via WhatsApp

- Implemented WhatsAppService for sending messages
- Added POConfirmationService for PO workflow
- Added D365 plugin for event handling

Closes #123
```

### Pull Request Process

1. Create feature branch: `git checkout -b feature/your-feature`
2. Make changes and commit
3. Push to remote: `git push origin feature/your-feature`
4. Create Pull Request
5. Ensure all tests pass
6. Request review
7. Merge when approved

## Troubleshooting

### Build Issues

**Error**: `The type 'System.Net.Http.HttpClient' already exists in this project`

**Solution**:
```xml
<!-- Remove from .csproj if duplicate -->
<ItemGroup>
    <Reference Include="System.Net.Http" />
</ItemGroup>
```

**Error**: `NuGet package restore failed`

**Solution**:
```bash
dotnet nuget locals all --clear
dotnet restore
```

### Testing Issues

**Error**: `Tests run but don't execute`

**Solution**: Check test framework:
```bash
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
```

### Plugin Compilation

**Error**: `Cannot compile D365 plugin`

**Check**:
- Target framework: .NET Framework 4.7.2
- D365 SDK references are correct
- No async/await in plugins (D365 limitation)

## IDE Setup

### Visual Studio

1. Install extensions:
   - Code Cleanup
   - Resharper (optional)
   - NUnit Test Adapter

2. Configure code style:
   - Tools → Options → C# → Code Style
   - Set formatting rules

### Visual Studio Code

1. Install extensions:
   - C#
   - .NET Extension Pack
   - REST Client

2. Create `.vscode/settings.json`:
```json
{
    "omnisharp.enableRoslynAnalyzers": true,
    "omnisharp.enableEditorConfigSupport": true,
    "files.exclude": {
        "**/bin": true,
        "**/obj": true
    }
}
```
