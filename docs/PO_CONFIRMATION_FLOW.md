# Purchase Order Confirmation Flow

## Overview

This document describes the complete flow of Purchase Order confirmation from Dynamics 365 Finance to vendor via WhatsApp.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│          Dynamics 365 Finance & Operations                  │
│                                                              │
│  1. User creates/confirms Purchase Order                   │
│  2. PO entity record saved                                 │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│           Plugin Trigger (Post Operation)                   │
│                                                              │
│  POConfirmationPlugin.Execute()                            │
│  - Extracts PO details                                     │
│  - Retrieves vendor info                                   │
│  - Creates message log record                              │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│          WhatsApp Message Log Table (D365)                 │
│                                                              │
│  Status: Pending                                           │
│  Message: Ready for processing                            │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│     PO Confirmation Service (Web Service/Function)         │
│                                                              │
│  1. Read message log record                               │
│  2. Retrieve full PO details                              │
│  3. Generate formatted message                            │
│  4. Validate vendor phone number                          │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│         WhatsApp Service (API Wrapper)                     │
│                                                              │
│  1. Format message for Meta API                           │
│  2. Add auth headers                                      │
│  3. Send HTTP POST to Meta endpoint                       │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│        Meta WhatsApp Business API (Cloud)                   │
│                                                              │
│  POST /v18.0/phone_number_id/messages                     │
│  - Validates message                                      │
│  - Routes to vendor's WhatsApp                            │
│  - Returns message ID                                     │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│          Vendor's WhatsApp Account                          │
│                                                              │
│  ✓ Message Received                                        │
│  ✓ Notification Displayed                                 │
│  ✓ Vendor can review/respond                             │
└─────────────────────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│      Message Log Updated (D365) - Optional Webhook         │
│                                                              │
│  Status: Sent/Delivered                                   │
│  Message ID: Stored for tracking                          │
│  Delivery Time: Recorded                                  │
└─────────────────────────────────────────────────────────────┘
```

## Detailed Step-by-Step Flow

### Step 1: Purchase Order Creation in D365

**Location**: Accounts Payable → Purchase Orders

```csharp
// User actions
1. Open Accounts Payable module
2. Click "Purchase Orders"
3. Click "New" to create PO
4. Fill in details:
   - Vendor: Select from dropdown
   - Lines: Add items
   - Delivery address
   - Payment terms
5. Click "Confirm" button
```

**Data State**:
- PurchaseOrderTable record created
- Status: Confirmed
- VendorAccountNumber: VENDOR001

### Step 2: Plugin Trigger

**Plugin**: `POConfirmationPlugin`
**Event**: `PurchaseOrderTable` → `Post Operation` → `Create/Update`

```csharp
// Plugin Logic
public void Execute(IServiceProvider serviceProvider)
{
    // 1. Get PO entity
    var poEntity = (Entity)context.InputParameters["Target"];
    var poNumber = poEntity["PurchaseOrderNumber"];
    
    // 2. Retrieve full PO record
    var poRecord = RetrievePurchaseOrder(service, poNumber);
    
    // 3. Get vendor details
    var vendorAccountId = poRecord["VendorAccountNumber"];
    var vendor = RetrieveVendor(service, vendorAccountId);
    var phoneNumber = vendor["WhatsAppPhoneNumber"];
    
    // 4. Create WhatsApp Message Log
    CreateWhatsAppMessageLog(service, poNumber, phoneNumber, vendorName);
}
```

**Output**: WhatsAppMessageLog record with Status = "Pending"

### Step 3: Message Processing Service

**Service**: `POConfirmationService`
**Trigger**: Scheduled batch job or event-driven

```csharp
// Message Processing
public async Task<bool> SendPOConfirmationAsync(POConfirmationRequest request)
{
    // 1. Validate phone number
    var isValid = await ValidatePhoneNumberAsync(request.PhoneNumber);
    
    // 2. Generate message
    var messageBody = await GenerateMessageAsync(request.PurchaseOrder);
    
    // 3. Create WhatsApp message object
    var whatsAppMessage = new WhatsAppMessage
    {
        RecipientPhoneNumber = request.PhoneNumber,
        MessageType = "text",
        Text = new TextMessage { Body = messageBody }
    };
    
    // 4. Send via WhatsApp Service
    var response = await SendMessageAsync(whatsAppMessage);
    
    // 5. Update message log
    UpdateMessageLog(response.MessageId, "Sent");
}
```

### Step 4: Message Generation

**Output Example**:
```
🛒 Purchase Order Confirmation
━━━━━━━━━━━━━━━━━━━━━━━━━

PO Number: PO-2024-001
Vendor: ABC Supplies
Date: 14/06/2026

📦 Order Details:
Items Count: 3
Total Amount: 15,000.00 USD

📅 Expected Delivery: 21/06/2026

📍 Delivery Address:
123 Main Street
New York, NY 10001

💳 Payment Terms: Net 30

👤 Contact Person: John Smith

━━━━━━━━━━━━━━━━━━━━━━━━━
Please confirm receipt of this Purchase Order.
```

### Step 5: WhatsApp API Call

**Endpoint**: `POST https://graph.instagram.com/v18.0/{phone_number_id}/messages`

**Request Payload**:
```json
{
  "messaging_product": "whatsapp",
  "to": "+11234567890",
  "type": "text",
  "text": {
    "body": "🛒 Purchase Order Confirmation..."
  }
}
```

**Response**:
```json
{
  "messages": [
    {
      "id": "wamid.ABC123DEF456=",
      "message_status": "accepted"
    }
  ],
  "contacts": [
    {
      "input": "+11234567890",
      "wa_id": "11234567890"
    }
  ]
}
```

### Step 6: Message Delivery

**WhatsApp Server Processing**:
1. Message queued
2. Route to vendor's WhatsApp account
3. Vendor notification
4. Status update: "sent" → "delivered" → "read"

**Webhook Response** (if configured):
```json
{
  "object": "whatsapp_business_account",
  "entry": [{
    "id": "123456789",
    "changes": [{
      "value": {
        "messaging_product": "whatsapp",
        "metadata": {
          "display_phone_number": "16505551234",
          "phone_number_id": "123456789"
        },
        "statuses": [{
          "id": "wamid.ABC123DEF456=",
          "status": "delivered",
          "timestamp": "1668582600",
          "recipient_id": "11234567890"
        }]
      }
    }]
  }]
}
```

### Step 7: D365 Update

**Update Message Log**:
```csharp
var messageLog = service.Retrieve("WhatsAppMessageLog", messageLogId, 
    new ColumnSet("MessageId", "Status"));

messageLog["MessageId"] = "wamid.ABC123DEF456=";
messageLog["Status"] = "Sent"; // or "Delivered"
messageLog["SentDate"] = DateTime.UtcNow;

service.Update(messageLog);
```

## Error Handling Flow

```
┌─────────────────────────────────────┐
│  Error Detected                      │
└────────────┬────────────────────────┘
             │
      ┌──────┴──────┐
      │             │
      ▼             ▼
  RETRY        FAIL
  │             │
  │             ▼
  │        Log Error
  │        Update Status: "Failed"
  │        Store Error Message
  │             │
  │             ▼
  │        Notify Admin
  │        (Email/Teams)
  │             │
  └─────────────┴─────────────┐
                               ▼
                        Admin Review
```

## Status Transitions

```
Pending
  │
  ├─ Phone Validation Passed ──┐
  │                            │
  │  Phone Validation Failed ──► Error
  │                            │
  ▼                            │
Sending                        │
  │                            │
  ├─ API Call Success ──┐      │
  │                     │      │
  │  API Call Failed ───┼─────► Error
  │                     │      │
  ▼                     │      │
Sent                    │      ▼
  │                     │   Failed
  │  (Optional)         │      │
  ├─ Webhook Received   │      │
  │                     │      │
  ▼                     │      │
Delivered               │      │
  │                     │      │
  ├─ Webhook Received   │      │
  │                     │      │
  ▼                     │      │
Read                    │      │
                        │      │
                        └──────┘
```

## Exception Handling

### Invalid Phone Number
```csharp
try {
    await ValidatePhoneNumberAsync(phoneNumber);
}
catch (ArgumentException ex) {
    log.Error($"Invalid phone format: {phoneNumber}");
    messageLog.Status = "Failed";
    messageLog.ErrorMessage = "Invalid phone number format";
}
```

### API Rate Limit
```csharp
if (response.StatusCode == 429) {
    // Too Many Requests
    log.Warning("Rate limit exceeded, retrying...");
    await Task.Delay(60000); // Wait 1 minute
    return await RetryAsync();
}
```

### Authentication Failure
```csharp
if (response.StatusCode == 401) {
    // Unauthorized
    log.Error("Authentication failed - check access token");
    messageLog.Status = "Failed";
    messageLog.ErrorMessage = "Authentication failed";
    NotifyAdministrator();
}
```

## Monitoring & Tracking

### Query Message Status
```sql
SELECT 
    PurchaseOrderNumber,
    RecipientPhoneNumber,
    Status,
    MessageId,
    CreatedDate,
    SentDate,
    DATEDIFF(SECOND, CreatedDate, SentDate) as ProcessingTimeSeconds
FROM WhatsAppMessageLog
WHERE CreatedDate > DATEADD(day, -7, GETDATE())
ORDER BY CreatedDate DESC
```

### Track Success Rate
```sql
SELECT 
    CONVERT(DATE, CreatedDate) as Date,
    Status,
    COUNT(*) as Count,
    COUNT(*) * 100.0 / SUM(COUNT(*)) OVER (PARTITION BY CONVERT(DATE, CreatedDate)) as Percentage
FROM WhatsAppMessageLog
GROUP BY CONVERT(DATE, CreatedDate), Status
ORDER BY Date DESC, Status
```
