# Quick Start Guide

## ⚡ 5-Minute Setup

Get up and running with D365 WhatsApp integration in 5 minutes.

---

## Prerequisites

- ✅ Dynamics 365 Finance & Operations
- ✅ Meta WhatsApp Business Account
- ✅ Administrator access

---

## Step 1: Get WhatsApp Credentials (2 min)

1. Go to [Meta Business Platform](https://business.facebook.com)
2. Navigate to **WhatsApp** → **Getting Started**
3. Copy:
   - **Phone Number ID**
   - **Business Account ID**
   - **Access Token**

---

## Step 2: Configure D365 (2 min)

1. Open **Accounts Payable** → **Vendors**
2. Add field: `WhatsAppPhoneNumber`
3. Add vendor phone in E.164 format: `+1234567890`
4. Create table: `WhatsAppMessageLog`

---

## Step 3: Deploy Code (1 min)

```bash
# Clone repository
git clone https://github.com/hmburhan-AI/D365-WhatsApp.git
cd D365-WhatsApp

# Update config
cp config/appsettings.json config/appsettings.local.json

# Build
dotnet build --configuration Release
```

---

## Step 4: Test ✅

1. Create Purchase Order in D365
2. Add vendor with WhatsApp number
3. Confirm the PO
4. Check vendor's WhatsApp for message
5. Verify message log entry in D365

**Done!** Your integration is now live. 🎉

---

## Next Steps

- 📖 Read [[Setup Instructions|Setup-Instructions]] for detailed guide
- 🔧 Configure [[Configuration|Configuration]]
- 🧪 Run [[Testing Guide|Testing-Guide]]
- 📊 Monitor with [[Monitoring|Monitoring]]

