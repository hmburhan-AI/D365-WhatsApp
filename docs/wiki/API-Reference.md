# API Reference

## WhatsApp Service

### Interface: IWhatsAppService

```csharp
public interface IWhatsAppService
{
    Task<WhatsAppResponse> SendMessageAsync(WhatsAppMessage message);
    Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
}
```

---

## Methods

### SendMessageAsync

**Description**: Sends a message via Meta WhatsApp Business API

**Parameters**:
- `message` (WhatsAppMessage): Message object

**Returns**: WhatsAppResponse with message status

**Throws**: Exception if API call fails

**Example**:

```csharp
var message = new WhatsAppMessage
{
    RecipientPhoneNumber = "+11234567890",
    MessageType = "text",
    Text = new TextMessage { Body = "Test message" }
};

var response = await whatsAppService.SendMessageAsync(message);
var messageId = response.Messages[0].MessageId;
```

---

### ValidatePhoneNumberAsync

**Description**: Validates phone number format (E.164)

**Parameters**:
- `phoneNumber` (string): Phone in E.164 format

**Returns**: true if valid, false otherwise

**Example**:

```csharp
var isValid = await whatsAppService.ValidatePhoneNumberAsync("+11234567890");
if (!isValid)
    throw new ArgumentException("Invalid phone number");
```

---

## Models

### WhatsAppMessage

```csharp
public class WhatsAppMessage
{
    public string MessagingProduct { get; set; }      // "whatsapp"
    public string RecipientType { get; set; }         // "individual"
    public string RecipientPhoneNumber { get; set; }   // "+1234567890"
    public string MessageType { get; set; }           // "text", "document"
    public TextMessage Text { get; set; }
    public MediaMessage Document { get; set; }
}
```

### WhatsAppResponse

```csharp
public class WhatsAppResponse
{
    public List<MessageStatus> Messages { get; set; }
    public List<Contact> Contacts { get; set; }
}
```

---

## Error Handling

### Error Codes

| Code | Status | Solution |
|------|--------|----------|
| 100 | Invalid Request | Check message format |
| 190 | Authentication Error | Verify access token |
| 550 | Rate Limit Exceeded | Implement batching |
| 400 | Invalid Phone | Use E.164 format |
| 404 | Not Found | Check phone number ID |

---

## Examples

### Send Text Message

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

try
{
    var response = await whatsAppService.SendMessageAsync(message);
    Console.WriteLine($"Success: {response.Messages[0].MessageId}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Send Document Message

```csharp
var message = new WhatsAppMessage
{
    RecipientPhoneNumber = "+11234567890",
    MessageType = "document",
    Document = new MediaMessage
    {
        Link = "https://example.com/po.pdf",
        Caption = "Purchase Order"
    }
};

var response = await whatsAppService.SendMessageAsync(message);
```

---

For complete API documentation, see the [[API Reference|API-Reference]] page.

