# Developer Guide

## Development Setup

---

## Prerequisites

- Visual Studio 2019+ or Visual Studio Code
- .NET Framework 4.7.2+
- NuGet Package Manager
- Git
- D365 Developer Toolkit

---

## Environment Setup

### 1. Clone Repository

```bash
git clone https://github.com/hmburhan-AI/D365-WhatsApp.git
cd D365-WhatsApp
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Open in IDE

```bash
start D365.WhatsApp.sln  # Visual Studio
# or
code .  # Visual Studio Code
```

### 4. Configure Local Settings

```bash
cp config/appsettings.json config/appsettings.Development.json
# Edit with your test credentials
```

---

## Building

### Debug Build

```bash
dotnet build --configuration Debug
```

### Release Build

```bash
dotnet build --configuration Release
```

---

## Testing

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Project

```bash
dotnet test tests/WhatsApp.Tests/WhatsApp.Tests.csproj
```

### With Code Coverage

```bash
dotnet test /p:CollectCoverage=true
```

---

## Code Standards

### Naming Conventions

- **Classes**: PascalCase → `WhatsAppService`
- **Methods**: PascalCase → `SendMessageAsync`
- **Properties**: PascalCase → `RecipientPhoneNumber`
- **Parameters**: camelCase → `phoneNumber`
- **Private Fields**: _camelCase → `_httpClient`
- **Constants**: UPPER_SNAKE_CASE → `MAX_RETRIES`

### Documentation

All public methods must have XML documentation:

```csharp
/// <summary>
/// Sends a WhatsApp message
/// </summary>
/// <param name="message">Message to send</param>
/// <returns>Response with message ID</returns>
public async Task<WhatsAppResponse> SendMessageAsync(WhatsAppMessage message)
```

---

## Project Structure

```
src/
├── D365/
│   ├── PurchaseOrder/
│   │   ├── Models/
│   │   └── Services/
│   ├── Plugins/
│   └── D365.WhatsApp.D365.csproj
├── WhatsApp/
│   ├── Models/
│   ├── Services/
│   └── D365.WhatsApp.Services.csproj
├── Configuration/
│   └── WhatsAppConfig.cs
└── D365.WhatsApp.sln

tests/
├── WhatsApp.Tests/
└── D365.Tests/
```

---

## Contributing

### Branch Naming

- `feature/description` - New feature
- `bugfix/description` - Bug fix
- `docs/description` - Documentation
- `refactor/description` - Code refactoring

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
- `refactor:` - Code refactoring
- `test:` - Tests

### Pull Request Process

1. Create feature branch
2. Make changes and commit
3. Push to remote
4. Create Pull Request
5. Ensure tests pass
6. Request review
7. Merge when approved

---

## Logging

### Log Levels

```csharp
_logger.LogDebug("Detailed diagnostic info");
_logger.LogInformation("General information");
_logger.LogWarning("Warning message");
_logger.LogError("Error occurred", exception);
_logger.LogCritical("Critical error");
```

### Best Practices

```csharp
// Good: Clear context
_logger.LogInformation($"Sending PO {poNumber} to {vendorName}");

// Bad: Unclear
_logger.LogInformation("Processing");

// Good: Mask sensitive data
var maskedPhone = "***-***-" + phone.Substring(phone.Length - 4);
_logger.LogInformation($"Sending to {maskedPhone}");
```

---

## Testing Guidelines

### Unit Test Template

```csharp
[TestFixture]
public class MyServiceTests
{
    private MyService _service;

    [SetUp]
    public void Setup()
    {
        _service = new MyService();
    }

    [Test]
    public void Method_WithCondition_ExpectedResult()
    {
        // Arrange
        var input = new Input();

        // Act
        var result = _service.Method(input);

        // Assert
        Assert.IsTrue(result);
    }
}
```

---

## IDE Configuration

### Visual Studio

1. Install extensions:
   - Code Cleanup
   - NUnit Test Adapter

2. Configure code style:
   - Tools → Options → C# → Code Style

### Visual Studio Code

1. Install extensions:
   - C#
   - .NET Extension Pack

2. Create `.vscode/settings.json`:

```json
{
    "omnisharp.enableRoslynAnalyzers": true,
    "files.exclude": {
        "**/bin": true,
        "**/obj": true
    }
}
```

---

## Related Pages

- [[Code Standards|Code-Standards]] - Detailed coding standards
- [[Testing Guide|Testing-Guide]] - Comprehensive testing
- [[Architecture Overview|Architecture-Overview]] - System design
- [[Contributing|Contributing]] - How to contribute

