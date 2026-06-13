# D365 Finance - WhatsApp Integration

A comprehensive design pack and implementation starter package for integrating Dynamics 365 Finance and Operations with Meta WhatsApp API. This project enables automated sending of business documents (like Purchase Orders) through WhatsApp to vendors.

## Overview

This integration solution provides:
- **Purchase Order Confirmation**: Automatically send PO confirmations to vendors via WhatsApp
- **Real-time Notifications**: Trigger notifications based on D365FO business events
- **Vendor Communication**: Streamlined vendor communication through WhatsApp
- **Document Delivery**: Secure and reliable document delivery with delivery confirmation

## Features

✅ Purchase Order Confirmation (PO Confirmation)  
✅ WhatsApp API Integration  
✅ Vendor Phone Number Management  
✅ Document Template Support  
✅ Error Handling & Logging  
✅ Delivery Status Tracking  
✅ Extensible Architecture  

## Architecture

```
┌──────────────────┐
│  Dynamics 365    │
│  Finance & Ops   │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  D365 Plugin/    │
│  Event Handler   │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  WhatsApp        │
│  Service Layer   │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Meta WhatsApp   │
│  Business API    │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Vendor WhatsApp │
│  Account         │
└──────────────────┘
```

## Project Structure

```
D365-WhatsApp/
├── docs/
│   ├── README.md
│   ├── SETUP.md
│   └── API_REFERENCE.md
├── src/
│   ├── D365/
│   │   ├── PurchaseOrder/
│   │   └── Plugins/
│   ├── WhatsApp/
│   │   ├── Services/
│   │   └── Models/
│   └── Configuration/
├── tests/
├── templates/
│   └── PO_Confirmation.template
├── config/
│   ├── appsettings.json
│   └── whatsapp-config.json
└── README.md
```

## Getting Started

1. **Prerequisites**
   - Dynamics 365 Finance & Operations
   - Meta WhatsApp Business Account
   - WhatsApp Business Phone Number
   - API Credentials (Phone Number ID, Business Account ID, Access Token)

2. **Installation**
   - See [SETUP.md](docs/SETUP.md) for detailed setup instructions

3. **Configuration**
   - Configure WhatsApp API credentials
   - Set up vendor phone numbers in D365
   - Deploy plugins and event handlers

## Documentation

- [Setup Guide](docs/SETUP.md) - Complete setup instructions
- [API Reference](docs/API_REFERENCE.md) - API endpoints and methods
- [Developer Guide](docs/DEVELOPER_GUIDE.md) - Development and customization guide
- [PO Confirmation Flow](docs/PO_CONFIRMATION_FLOW.md) - Detailed PO confirmation process

## Quick Links

- [Purchase Order Confirmation Implementation](src/D365/PurchaseOrder/POConfirmation.cs)
- [WhatsApp Service](src/WhatsApp/Services/WhatsAppService.cs)
- [Configuration Guide](docs/CONFIGURATION.md)

## Support

For issues, questions, or contributions, please create an issue in the repository.

## License

MIT License - See LICENSE file for details
