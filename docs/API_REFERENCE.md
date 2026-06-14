# API Reference - D365 WhatsApp Integration

## Table of Contents
1. [WhatsApp Service](#whatsapp-service)
2. [PO Confirmation Service](#po-confirmation-service)
3. [Models](#models)
4. [Error Codes](#error-codes)
5. [Examples](#examples)

## WhatsApp Service

### Interface: IWhatsAppService

#### Method: SendMessageAsync

```csharp
public interface IWhatsAppService
{
    Task<WhatsAppResponse> SendMessageAsync(WhatsAppMessage message);
    Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
}
```

**Description**: Sends a message via Meta WhatsApp Business API

**Parameters**:
- `message` (WhatsAppMessage): Message object containing recipient and content

**Returns**: WhatsAppResponse containing message status and ID

**Exceptions**:
- `ArgumentNullException`: If message is null
- `Exception`: If API call fails

**Example**:
```csharp
var message = new WhatsAppMessage
{
    RecipientPhoneNumber = "+11234567890",
    MessageType = "text",
    Text = new TextMessage
    {
        Body = "Hello, this is a test message"
    }
};

var response = await whatsAppService.SendMessageAsync(message);
var messageId = response.Messages[0].MessageId;
```

#### Method: ValidatePhoneNumberAsync

```csharp
public Task<bool> ValidatePhoneNumberAsync(string phoneNumber)
```

**Description**: Validates phone number format (E.164)

**Parameters**:
- `phoneNumber` (string): Phone number in E.164 format

**Returns**: True if valid, false otherwise

**Example**:
```csharp
var isValid = await whatsAppService.ValidatePhoneNumberAsync("+11234567890");
```

## PO Confirmation Service

### Interface: IPOConfirmationService

#### Method: SendPOConfirmationAsync

```csharp
public interface IPOConfirmationService
{
    Task<bool> SendPOConfirmationAsync(POConfirmationRequest request);
    Task<string> GenerateMessageAsync(PurchaseOrderModel po);
}
```

**Description**: Sends Purchase Order confirmation via WhatsApp

**Parameters**:
- `request` (POConfirmationRequest): PO confirmation request details

**Returns**: True if sent successfully, false otherwise

**Example**:
```csharp
var po = new PurchaseOrderModel
{
    PurchaseOrderId = "PO-001",
    VendorName = "Vendor Name",
    VendorPhoneNumber = "+11234567890",
    PurchaseOrderDate = DateTime.Now,
    TotalAmount = 5000m,
    CurrencyCode = "USD"
};

var request = new POConfirmationRequest
{
    PurchaseOrder = po,
    IncludeAttachment = true
};

var result = await poConfirmationService.SendPOConfirmationAsync(request);
```

#### Method: GenerateMessageAsync

```csharp
public Task<string> GenerateMessageAsync(PurchaseOrderModel po)
```

**Description**: Generates formatted message from Purchase Order

**Parameters**:
- `po` (PurchaseOrderModel): Purchase Order object

**Returns**: Formatted message string

**Example**:
```csharp
var message = await poConfirmationService.GenerateMessageAsync(po);
Console.WriteLine(message);
```

## Models

### PurchaseOrderModel

```csharp
public class PurchaseOrderModel
{
    public string PurchaseOrderId { get; set; }
    public string VendorAccountNumber { get; set; }
    public string VendorName { get; set; }
    public string VendorPhoneNumber { get; set; }
    public DateTime PurchaseOrderDate { get; set; }
    public string DeliveryAddress { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; }
    public List<PurchaseOrderLineModel> Lines { get; set; }
    public string ContactPersonName { get; set; }
    public string PaymentTerms { get; set; }
    public string Notes { get; set; }
    public string Status { get; set; }
    public string DocumentUrl { get; set; }
}
```

### WhatsAppMessage

```csharp
public class WhatsAppMessage
{
    public string MessagingProduct { get; set; } = "whatsapp";
    public string RecipientType { get; set; } = "individual";
    public string RecipientPhoneNumber { get; set; }
    public string MessageType { get; set; } // "text", "document", "image"
    public TextMessage Text { get; set; }
    public MediaMessage Document { get; set; }
    public TemplateMessage Template { get; set; }
}
```

### WhatsAppResponse

```csharp
public class WhatsAppResponse
{
    public List<MessageStatus> Messages { get; set; }
    public List<Contact> Contacts { get; set; }
}

public class MessageStatus
{
    public string MessageId { get; set; }
    public string Status { get; set; } // "accepted", "sent", "delivered", "read", "failed"
}
```

### POConfirmationRequest

```csharp
public class POConfirmationRequest
{
    public string RequestId { get; set; }
    public PurchaseOrderModel PurchaseOrder { get; set; }
    public bool IncludeAttachment { get; set; }
    public string TemplateId { get; set; }
    public string CustomMessage { get; set; }
    public DateTime RequestTimestamp { get; set; }
    public string RequestedBy { get; set; }
}
```

## Error Codes

| Code | Status | Meaning |
|------|--------|----------|
| 100  | Invalid Request | Message validation failed |
| 190  | Authentication Error | Invalid access token |
| 550  | Rate Limit Exceeded | Too many requests |
| 200  | Success | Message sent successfully |
| 400  | Invalid Phone | Invalid phone number format |
| 404  | Not Found | Phone number not found |

## Examples

### Example 1: Send Simple Text Message

```csharp
var message = new WhatsAppMessage
{
    RecipientPhoneNumber = "+11234567890",
    MessageType = "text",
    Text = new TextMessage
    {
        Body = "Hello! This is a test message."
    }
};

try
{
    var response = await whatsAppService.SendMessageAsync(message);
    Console.WriteLine($"Message sent: {response.Messages[0].MessageId}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Example 2: Send PO Confirmation

```csharp
var purchaseOrder = new PurchaseOrderModel
{
    PurchaseOrderId = "PO-2024-001",
    VendorAccountNumber = "VENDOR001",
    VendorName = "ABC Supplies",
    VendorPhoneNumber = "+11234567890",
    PurchaseOrderDate = DateTime.Now,
    TotalAmount = 15000m,
    CurrencyCode = "USD",
    DeliveryAddress = "123 Main St, City, State 12345",
    DeliveryDate = DateTime.Now.AddDays(7),
    PaymentTerms = "Net 30",
    Lines = new List<PurchaseOrderLineModel>
    {
        new PurchaseOrderLineModel
        {
            LineNumber = 1,
            ItemNumber = "ITEM001",
            ProductName = "Product A",
            Quantity = 100,
            UnitOfMeasurement = "PC",
            UnitPrice = 150m
        }
    }
};

var request = new POConfirmationRequest
{
    PurchaseOrder = purchaseOrder,
    IncludeAttachment = true
};

var result = await poConfirmationService.SendPOConfirmationAsync(request);
if (result)
{
    Console.WriteLine("PO confirmation sent successfully");
}
else
{
    Console.WriteLine("Failed to send PO confirmation");
}
```

### Example 3: Error Handling

```csharp
try
{
    var isValid = await whatsAppService.ValidatePhoneNumberAsync(phoneNumber);
    
    if (!isValid)
    {
        Console.WriteLine("Invalid phone number format");
        return;
    }
    
    var message = new WhatsAppMessage
    {
        RecipientPhoneNumber = phoneNumber,
        MessageType = "text",
        Text = new TextMessage { Body = "Test message" }
    };
    
    var response = await whatsAppService.SendMessageAsync(message);
    
    if (response?.Messages?.Count > 0)
    {
        Console.WriteLine($"Success: {response.Messages[0].MessageId}");
    }
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"Argument error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```
