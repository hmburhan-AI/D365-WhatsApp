# Architecture Overview

## System Design

The D365 WhatsApp integration uses a layered architecture for maintainability and scalability.

---

## Architecture Layers

### 1. Presentation Layer
- D365 User Interface
- Purchase Order Form
- Message Log Dashboard

### 2. Plugin Layer (Event Handler)
- **POConfirmationPlugin**
- Triggers on PO confirmation
- Async processing
- Error handling

### 3. Business Logic Layer
- **POConfirmationService**
- Message generation
- Workflow orchestration
- Vendor validation

### 4. API Integration Layer
- **WhatsAppService**
- Phone validation
- Message formatting
- API communication

### 5. External Services Layer
- Meta WhatsApp Business API
- Cloud messaging
- Delivery tracking

---

## Component Diagram

```
┌─────────────────────────────────┐
│   Dynamics 365 Finance          │
│   (PO Confirmation)             │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│  POConfirmationPlugin           │
│  - Event Trigger                │
│  - Async Processing             │
│  - Error Handling               │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│  POConfirmationService          │
│  - Message Generation           │
│  - Workflow Logic               │
│  - Vendor Validation            │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│  WhatsAppService                │
│  - Phone Validation             │
│  - Message Formatting           │
│  - API Communication            │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│  Meta WhatsApp Business API     │
│  - Message Routing              │
│  - Delivery Tracking            │
│  - Status Updates               │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│  Vendor WhatsApp Account        │
│  - Message Received             │
│  - Can Review & Respond         │
└─────────────────────────────────┘
```

---

## Data Flow

```
1. PO Created in D365
   ↓
2. Plugin Triggered
   - Extract PO details
   - Retrieve vendor info
   - Create message log
   ↓
3. POConfirmationService Processes
   - Generate message
   - Validate phone
   - Create request
   ↓
4. WhatsAppService Sends
   - Add auth headers
   - Format message
   - HTTP POST to API
   ↓
5. Meta API Processes
   - Validate message
   - Route to vendor
   - Return message ID
   ↓
6. Message Delivered
   - Update message log
   - Store message ID
   - Set status: Sent
```

---

## Key Patterns

### Dependency Injection
```csharp
services.AddScoped<IPOConfirmationService, POConfirmationService>();
services.AddHttpClient<IWhatsAppService, WhatsAppService>();
```

### Async/Await
```csharp
public async Task<bool> SendPOConfirmationAsync(POConfirmationRequest request)
```

### Repository Pattern
```csharp
public interface IWhatsAppService { }
public class WhatsAppService : IWhatsAppService { }
```

---

## Security Architecture

- 🔒 Bearer token authentication
- 🔐 HTTPS-only communication
- 🛡️ Phone number masking
- ✅ Input validation
- 🔑 Secure credential storage

See [[Security Model|Security-Model]] for details.

---

## Scalability

### Current Architecture
- Single instance deployment
- Asynchronous plugin processing
- Rate limiting: 60 messages/minute

### For High Volume
- Message queue (Azure Service Bus)
- Multiple worker instances
- Batch processing
- Load balancing

---

## Related Pages

- [[Components|Components]] - System components
- [[Data Models|Data-Models]] - Data structures
- [[Security Model|Security-Model]] - Security details
- [[API Reference|API-Reference]] - API documentation

