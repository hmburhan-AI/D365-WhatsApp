# Testing Guide

## Unit Testing

### WhatsApp Service Tests

```csharp
using NUnit.Framework;
using Moq;
using D365.WhatsApp.Services;
using D365.WhatsApp.Services.Models;

[TestFixture]
public class WhatsAppServiceTests
{
    private WhatsAppService _service;
    private Mock<HttpClient> _httpClientMock;
    private ILogger<WhatsAppService> _logger;

    [SetUp]
    public void Setup()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<WhatsAppService>();
        var httpClient = new HttpClient();
        
        _service = new WhatsAppService(
            httpClient,
            "test_phone_id",
            "test_token",
            "v18.0",
            _logger);
    }

    [Test]
    public async Task SendMessage_WithValidData_ReturnsSuccess()
    {
        var message = new WhatsAppMessage
        {
            RecipientPhoneNumber = "+11234567890",
            MessageType = "text",
            Text = new TextMessage { Body = "Test message" }
        };

        var result = await _service.SendMessageAsync(message);

        Assert.IsNotNull(result);
        Assert.IsNotEmpty(result.Messages);
    }

    [Test]
    public async Task ValidatePhoneNumber_WithValidFormat_ReturnsTrue()
    {
        var result = await _service.ValidatePhoneNumberAsync("+11234567890");
        Assert.IsTrue(result);
    }

    [Test]
    [TestCase("+1")]
    [TestCase("1-123-456-7890")]
    [TestCase("(123) 456-7890")]
    public async Task ValidatePhoneNumber_WithInvalidFormat_ReturnsFalse(string phone)
    {
        var result = await _service.ValidatePhoneNumberAsync(phone);
        Assert.IsFalse(result);
    }

    [Test]
    public void SendMessage_WithNullMessage_ThrowsException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () => 
            await _service.SendMessageAsync(null));
    }
}
```

### PO Confirmation Service Tests

```csharp
[TestFixture]
public class POConfirmationServiceTests
{
    private POConfirmationService _service;
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
        var po = new PurchaseOrderModel
        {
            PurchaseOrderId = "PO-001",
            VendorName = "Test Vendor",
            VendorPhoneNumber = "+11234567890",
            TotalAmount = 1000m,
            CurrencyCode = "USD"
        };

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

        var request = new POConfirmationRequest { PurchaseOrder = po };
        var result = await _service.SendPOConfirmationAsync(request);

        Assert.IsTrue(result);
    }

    [Test]
    public async Task GenerateMessage_WithValidPO_ReturnsFormattedMessage()
    {
        var po = new PurchaseOrderModel
        {
            PurchaseOrderId = "PO-001",
            VendorName = "ABC Supplies",
            TotalAmount = 5000m,
            CurrencyCode = "USD"
        };

        var message = await _service.GenerateMessageAsync(po);

        Assert.IsNotNull(message);
        Assert.Contains("PO-001", message);
        Assert.Contains("ABC Supplies", message);
    }
}
```

## Integration Testing

### Test Scenario: End-to-End PO Confirmation

```csharp
[TestFixture]
public class IntegrationTests
{
    private D365ServiceClient _d365Client;
    private IWhatsAppService _whatsAppService;
    private IPOConfirmationService _poService;

    [SetUp]
    public void Setup()
    {
        // Connect to D365 test environment
        _d365Client = new D365ServiceClient("D365_Test_Connection");
        // Initialize WhatsApp service
        _whatsAppService = new WhatsAppService(/* test config */);
        _poService = new POConfirmationService(_whatsAppService, /* logger */);
    }

    [Test]
    public async Task EndToEnd_CreatePOAndVerifyMessage()
    {
        // Step 1: Create Purchase Order in D365
        var poEntity = new Entity("PurchaseOrderTable")
        {
            ["PurchaseOrderNumber"] = "TEST-PO-001",
            ["VendorAccountNumber"] = "VENDOR001",
            ["TotalAmount"] = 1000m
        };
        var poId = _d365Client.Create(poEntity);

        // Step 2: Retrieve vendor with WhatsApp number
        var vendorEntity = _d365Client.Retrieve("VendorTable", 
            new ColumnSet("WhatsAppPhoneNumber"));
        var phoneNumber = vendorEntity["WhatsAppPhoneNumber"];

        // Step 3: Create PO model
        var po = new PurchaseOrderModel
        {
            PurchaseOrderId = "TEST-PO-001",
            VendorPhoneNumber = phoneNumber.ToString(),
            TotalAmount = 1000m,
            CurrencyCode = "USD"
        };

        // Step 4: Send confirmation
        var request = new POConfirmationRequest { PurchaseOrder = po };
        var result = await _poService.SendPOConfirmationAsync(request);

        // Step 5: Verify message log
        var messageLog = _d365Client.RetrieveMultiple(
            new QueryExpression("WhatsAppMessageLog")
            {
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("PurchaseOrderNumber", 
                            ConditionOperator.Equal, "TEST-PO-001")
                    }
                }
            });

        Assert.IsTrue(result);
        Assert.Greater(messageLog.Entities.Count, 0);
    }
}
```

## Manual Testing

### Test Case 1: Basic PO Confirmation

**Objective**: Verify basic PO confirmation flow

**Steps**:
1. Log in to D365
2. Navigate to Accounts Payable → Purchase Orders
3. Create new PO with test vendor
4. Add vendor with WhatsApp phone
5. Confirm PO
6. Check vendor WhatsApp for message
7. Verify message log in D365

**Expected Result**: Message received within 5 seconds

**Pass/Fail**: _____

---

### Test Case 2: Invalid Phone Number

**Objective**: Verify error handling for invalid phone

**Steps**:
1. Create PO with invalid phone number
2. Confirm PO
3. Check message log status

**Expected Result**: Status = "Failed", Error message logged

**Pass/Fail**: _____

---

### Test Case 3: Multiple Vendors

**Objective**: Verify batch processing

**Steps**:
1. Create 10 POs for different vendors
2. Confirm all POs
3. Monitor message delivery
4. Check completion status

**Expected Result**: All messages delivered within 60 seconds

**Pass/Fail**: _____

---

### Test Case 4: Message Resend

**Objective**: Verify retry mechanism

**Steps**:
1. Create PO
2. Simulate API failure
3. Verify automatic retry
4. Check message eventually succeeds

**Expected Result**: Message delivered after retry

**Pass/Fail**: _____

---

## Performance Testing

### Load Test Script

```csharp
[Test]
public async Task LoadTest_Send100Messages()
{
    var stopwatch = Stopwatch.StartNew();
    var tasks = new List<Task>();

    for (int i = 0; i < 100; i++)
    {
        var po = CreateTestPO(i);
        var request = new POConfirmationRequest { PurchaseOrder = po };
        tasks.Add(_poService.SendPOConfirmationAsync(request));
    }

    await Task.WhenAll(tasks);
    stopwatch.Stop();

    Console.WriteLine($"Sent 100 messages in {stopwatch.ElapsedMilliseconds}ms");
    Assert.Less(stopwatch.ElapsedMilliseconds, 600000); // 10 minutes max
}
```

### Performance Baselines

| Metric | Baseline | Acceptable | Critical |
|--------|----------|------------|----------|
| Message Send Time | < 2s | < 5s | > 10s |
| Plugin Execution | < 1s | < 2s | > 5s |
| API Response | < 1s | < 3s | > 5s |
| Delivery Rate | > 99% | > 95% | < 90% |
| Error Rate | < 1% | < 5% | > 10% |

---

## Test Reporting

### Test Summary Template

```
Test Date: [DATE]
Tester: [NAME]
Environment: [DEV/TEST/PROD]
Version: [VERSION]

Test Results:
- Total Tests: ___
- Passed: ___
- Failed: ___
- Blocked: ___
- Skipped: ___

Success Rate: ___%

Critical Issues: [LIST]
Major Issues: [LIST]
Minor Issues: [LIST]

Recommendation:
☐ Ready for Production
☐ Ready with Fixes
☐ Not Ready

Notes:
[ADD NOTES]
```

---

## Regression Testing

After each update, run:

1. Unit test suite
   ```bash
   dotnet test
   ```

2. Integration tests
   ```bash
   dotnet test tests/IntegrationTests/
   ```

3. Manual smoke test
   - Create 1 test PO
   - Verify message delivery
   - Check message log

---

**Last Updated**: 2026-06-14
