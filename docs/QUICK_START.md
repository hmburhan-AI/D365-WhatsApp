# Quick Start Guide

## 5-Minute Setup

Get up and running with D365 WhatsApp integration in just 5 minutes.

### Prerequisites
- Dynamics 365 Finance & Operations instance
- Meta WhatsApp Business Account
- Administrator access to both systems

### Step 1: Get WhatsApp Credentials (2 minutes)

1. Go to [Meta Business Platform](https://business.facebook.com)
2. Navigate to **WhatsApp** → **Getting Started**
3. Copy these credentials:
   - **Phone Number ID**: `123456789012345`
   - **Business Account ID**: `987654321098765`
   - **Access Token**: Generate from System User

### Step 2: Configure D365 (2 minutes)

1. Open **Accounts Payable** → **Vendors**
2. Add vendor field: `WhatsAppPhoneNumber` (Text, Phone format)
3. Fill in vendor phone in E.164 format: `+1234567890`
4. Create table: `WhatsAppMessageLog` with status tracking

### Step 3: Deploy Code (1 minute)

```bash
# Clone repository
git clone https://github.com/hmburhan-AI/D365-WhatsApp.git

# Update configuration
cp config/appsettings.json config/appsettings.local.json

# Add your WhatsApp credentials to appsettings.local.json
# Build
dotnet build --configuration Release
```

### Step 4: Test

1. Create a Purchase Order in D365
2. Add vendor with WhatsApp number
3. Confirm the PO
4. Check vendor's WhatsApp for message
5. Verify message log entry in D365

**✅ Done!** Your integration is now live.

---

For detailed setup, see [SETUP.md](SETUP.md)
