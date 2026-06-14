# Architecture & Design

## System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                   PRESENTATION LAYER                                 │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │
│  │   D365 UI        │  │   Portal         │  │   Admin Dashboard│  │
│  └────────┬─────────┘  └──────────────────┘  └──────────────────┘  │
└───────────┼────────────────────────────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────────────────────────────────────────┐
│                  BUSINESS LOGIC LAYER                                │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │              POConfirmationService                           │   │
│  │  - Message generation                                        │   │
│  │  - Workflow orchestration                                    │   │
│  │  - Vendor validation                                         │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                              ▲                                       │
│                              │                                       │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │              POConfirmationPlugin                            │   │
│  │  - Event trigger on PO confirmation                          │   │
│  │  - Async processing                                          │   │
│  │  - Error handling                                            │   │
│  └──────────────────┬──────────────────────────────────────────┘   │
└──────────────────────┼──────────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────────┐
│                   API INTEGRATION LAYER                              │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │              WhatsAppService                                  │  │
│  │  - API endpoint management                                    │  │
│  │  - Message formatting                                         │  │
│  │  - Authentication                                             │  │
│  │  - Retry logic                                                │  │
│  │  - Error handling                                             │  │
│  └──────────────────┬───────────────────────────────────────────┘  │
└─────────────────────┼─────────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────────────┐
│               EXTERNAL SERVICES LAYER                                │
│                                                                       │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │   Meta WhatsApp Business API                                  │  │
│  │   Endpoint: graph.instagram.com/v18.0/{phone_id}/messages    │  │
│  └──────────────────┬───────────────────────────────────────────┘  │
└─────────────────────┼─────────────────────────────────────────────┘
                      │
                      ▼
                ┌──────────────┐
                │ Vendor Phone │
                │  WhatsApp    │
                │   Account    │
                └──────────────┘
```

---

## Component Architecture

### 1. Data Models

```
┌─────────────────────────────────┐
│   PurchaseOrderModel            │
├─────────────────────────────────┤
│ - PurchaseOrderId               │
│ - VendorAccountNumber           │
│ - VendorPhoneNumber             │
│ - PurchaseOrderDate             │
│ - TotalAmount                   │
│ - Lines: List<POLineModel>      │
│ - DeliveryDate                  │
│ - PaymentTerms                  │
│ - Notes                         │
└─────────────────────────────────┘
         │
         ├─► PurchaseOrderLineModel
         │   - ItemNumber
         │   - ProductName
         │   - Quantity
         │   - UnitPrice
         │
         └─► POConfirmationRequest
             - RequestId
             - PurchaseOrder
             - IncludeAttachment
```

### 2. Service Layer

```
┌──────────────────────────────────────┐
│    IPOConfirmationService            │
├──────────────────────────────────────┤
│ Methods:                             │
│ + SendPOConfirmationAsync()          │
│ + GenerateMessageAsync()             │
└──────────┬───────────────────────────┘
           │
           ├─► IWhatsAppService
           │   + SendMessageAsync()
           │   + ValidatePhoneNumberAsync()
           │
           └─► WhatsAppConfig
               + PhoneNumberId
               + BusinessAccountId
               + AccessToken
               + ApiVersion
```

### 3. Plugin Architecture

```
┌────────────────────────────────────────┐
│   D365 Business Event                  │
│   PurchaseOrderTable - Confirmed       │
└────────────────┬───────────────────────┘
                 │
                 ▼
┌────────────────────────────────────────┐
│   POConfirmationPlugin                 │
│   (Post Operation - Async)             │
├────────────────────────────────────────┤
│ 1. Extract PO entity                   │
│ 2. Retrieve full PO record             │
│ 3. Get vendor details                  │
│ 4. Validate phone number               │
│ 5. Create message log record           │
│ 6. Return                              │
└────────────────┬───────────────────────┘
                 │
                 ▼
┌────────────────────────────────────────┐
│   WhatsAppMessageLog Record            │
│   Status: Pending                      │
│   Ready for async processing           │
└────────────────────────────────────────┘
```

---

## Data Flow Diagram

```
                        USER CREATES PO
                             │
                             ▼
┌──────────────────────────────────────┐
│  Dynamics 365                        │
│  - PurchaseOrder Created             │
│  - Plugin Triggered                  │
│  - WhatsApp Message Log Created      │
└──────────────────┬───────────────────┘
                   │ (PO Details)
                   ▼
┌──────────────────────────────────────┐
│  POConfirmationService               │
│  - Read Message Log                  │
│  - Generate Message                  │
│  - Validate Phone Number             │
│  - Create WhatsAppMessage            │
└──────────────────┬───────────────────┘
                   │ (Message Object)
                   ▼
┌──────────────────────────────────────┐
│  WhatsAppService                     │
│  - Add Auth Headers                  │
│  - Serialize Message                 │
│  - Make HTTP POST                    │
│  - Parse Response                    │
└──────────────────┬───────────────────┘
                   │ (HTTP Request)
                   ▼
┌──────────────────────────────────────┐
│  Meta WhatsApp API                   │
│  graph.instagram.com/v18.0/messages  │
│  - Validate Request                  │
│  - Route to Vendor                   │
│  - Return Message ID                 │
└──────────────────┬───────────────────┘
                   │ (Response)
                   ▼
┌──────────────────────────────────────┐
│  WhatsAppService                     │
│  - Extract Message ID                │
│  - Check Status                      │
│  - Return Response                   │
└──────────────────┬───────────────────┘
                   │ (Message ID)
                   ▼
┌──────────────────────────────────────┐
│  POConfirmationService               │
│  - Update Message Log                │
│  - Set Status: Sent                  │
│  - Store Message ID                  │
└──────────────────┬───────────────────┘
                   │
                   ▼
┌──────────────────────────────────────┐
│  Dynamics 365                        │
│  - WhatsApp Message Log Updated      │
│  - Status: Sent                      │
│  - Message ID: Stored                │
└──────────────────────────────────────┘
                   │
                   ▼ (Optional: Webhook)
┌──────────────────────────────────────┐
│  Vendor WhatsApp Account             │
│  Message Delivered/Read              │
│  Update via Webhook                  │
└──────────────────────────────────────┘
```

---

## Design Patterns Used

### 1. Repository Pattern
```csharp
public interface IWhatsAppService { }
public class WhatsAppService : IWhatsAppService { }
```
**Purpose**: Abstract API calls and allow for mocking

### 2. Dependency Injection
```csharp
services.AddScoped<IPOConfirmationService, POConfirmationService>();
services.AddHttpClient<IWhatsAppService, WhatsAppService>();
```
**Purpose**: Loose coupling and testability

### 3. Factory Pattern
```csharp
var config = WhatsAppConfig.Load(configuration);
```
**Purpose**: Centralized configuration creation

### 4. Async/Await Pattern
```csharp
public async Task<WhatsAppResponse> SendMessageAsync(WhatsAppMessage message)
```
**Purpose**: Non-blocking I/O and responsiveness

### 5. Error Handling Pattern
```csharp
try { } catch (ArgumentNullException) { } catch (Exception) { }
```
**Purpose**: Specific error handling and recovery

---

## Security Architecture

### Authentication Flow

```
┌────────────────────┐
│  Client Request    │
│  (WhatsAppService) │
└─────────┬──────────┘
          │
          ▼
┌────────────────────────────────────────┐
│  Add Bearer Token                      │
│  Authorization: Bearer {access_token}  │
└─────────┬──────────────────────────────┘
          │
          ▼
┌────────────────────┐
│  HTTPS Request     │
│  (TLS 1.2+)        │
└─────────┬──────────┘
          │
          ▼
┌────────────────────────────────────────┐
│  Meta API                              │
│  - Validate Token                      │
│  - Check Permissions                   │
│  - Check Rate Limits                   │
│  - Process Request                     │
└────────────────────────────────────────┘
```

### Configuration Security

```
Production Environment:
┌──────────────────────┐
│  Azure Key Vault     │
├──────────────────────┤
│ - PhoneNumberId      │
│ - BusinessAccountId  │
│ - AccessToken        │
│ - ClientSecret       │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────────┐
│  Managed Identity        │
│  (No credentials stored) │
└──────────────────────────┘

Development Environment (Local Only):
┌──────────────────────┐
│  appsettings.local   │
│  (Not in git repo)   │
└──────────────────────┘
```

---

## Scalability Considerations

### Message Queue Pattern (Optional)

For high-volume scenarios:

```
┌──────────────────────┐
│  PO Created          │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────────────┐
│  Message Queue               │
│  (Azure Service Bus)         │
├──────────────────────────────┤
│ - Queue Messages             │
│ - Handle Rate Limiting       │
│ - Ensure Delivery            │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────┐
│  Worker Service      │
│  - Process Queue     │
│  - Send Messages     │
│  - Retry Logic       │
└──────────────────────┘
```

### Load Balancing

```
┌─────────────────────────────────────┐
│     Multiple D365 Instances          │
├──────────────┬──────────────────────┤
│  Instance 1  │  Instance 2          │
│  Plugin 1    │  Plugin 2            │
└──────────────┼──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│     Shared Message Queue             │
│     (Azure Service Bus)              │
└──────────────┬──────────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│  Multiple Worker Services            │
│  (Load Balanced)                     │
│  - Workers: 2-10 instances           │
│  - Auto-scaling enabled              │
└──────────────────────────────────────┘
```

---

## Error Handling Strategy

```
┌─────────────────────────────────────┐
│     Message Send Request             │
└──────────────────┬──────────────────┘
                   │
        ┌──────────┴──────────┐
        │                     │
        ▼                     ▼
   ┌─────────┐         ┌──────────┐
   │ Success │         │  Error   │
   └─────────┘         └────┬─────┘
        │                   │
        │         ┌─────────┼─────────┐
        │         │         │         │
        │         ▼         ▼         ▼
        │      Retry    Fatal    Temporary
        │      Error    Error      Error
        │         │         │         │
        │         ▼         ▼         ▼
        │      Retry    Log &      Wait &
        │      (3x)     Alert      Retry
        │         │         │         │
        └─────────┴─────────┴────────┘
                  │
                  ▼
         Update Message Log
```

---

## Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|----------|
| **Plugin** | C# .NET Framework 4.7.2 | D365 plugin development |
| **Service** | C# .NET Core | Business logic |
| **API** | .NET HttpClient | REST API calls |
| **Database** | SQL Server | D365 data storage |
| **Configuration** | Azure Key Vault | Secrets management |
| **Logging** | Serilog/ILogger | Application logging |
| **Testing** | NUnit/Moq | Unit testing |
| **Documentation** | Markdown | Project documentation |

---

**Last Updated**: 2026-06-14  
**Version**: 1.0.0
