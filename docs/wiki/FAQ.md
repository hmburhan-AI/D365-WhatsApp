# FAQ - Frequently Asked Questions

## General Questions

### Q: What is this project?
A: An integration solution for Dynamics 365 Finance that sends Purchase Order confirmations to vendors via WhatsApp.

### Q: What are the main benefits?
A:
- Automated vendor communication
- Real-time message delivery confirmation
- Reduced email delivery issues
- Better vendor experience
- Complete audit trail

### Q: Is it production-ready?
A: Yes! Enterprise-grade solution with comprehensive error handling, security, and documentation.

### Q: What's the cost?
A:
- Solution: Free (MIT License)
- WhatsApp API: ~$0.05 per message (after 1,000 free messages/month)

---

## Setup Questions

### Q: How long does setup take?
A: 2-3 days typically:
- WhatsApp setup: 1 day
- D365 customization: 1 day
- Code deployment: 0.5 day
- Testing: 0.5 day

### Q: What do I need to start?
A:
- Dynamics 365 Finance & Operations
- Meta WhatsApp Business Account
- Dedicated business phone number
- Azure subscription (optional)

### Q: Can I use my personal phone?
A: No. WhatsApp Business API requires a dedicated, verified business phone number.

### Q: Do I need Azure?
A: Optional. You can use:
- Local configuration files (dev only)
- Azure Key Vault (recommended for prod)
- Other secret management solutions

---

## Technical Questions

### Q: What .NET version is required?
A: .NET Framework 4.7.2 (for D365 plugins)

### Q: Can I customize the message?
A: Yes! Edit the template or modify the `GenerateMessageAsync` method.

### Q: How many messages can I send per minute?
A: API limit is 60 messages/minute. Implement batching for higher volume.

### Q: What if vendor doesn't have WhatsApp?
A: API returns error. Message marked as failed in the log.

### Q: Can I send documents?
A: Yes, using document message type with URL or attachment.

---

## Operations

### Q: How do I monitor performance?
A: Use provided SQL queries or set up Azure monitoring.

### Q: What's typical delivery time?
A: Usually < 5 seconds from PO confirmation to vendor's WhatsApp.

### Q: How are messages logged?
A: In WhatsAppMessageLog table with full audit trail.

### Q: Can I resend a message?
A: Yes, update message log status to "Pending" and reprocess.

### Q: How long are messages kept?
A: Configurable, default 90 days.

---

## Security

### Q: Are credentials stored securely?
A: Yes! Use Azure Key Vault for production (no hardcoded secrets).

### Q: Is phone data encrypted?
A: Yes! In transit (HTTPS) and at rest (database encryption).

### Q: Is this GDPR compliant?
A: Solution includes GDPR-ready features. Configure your retention policy.

### Q: Are phone numbers visible to vendors?
A: No. They only see your business name and phone.

---

## Troubleshooting

### Q: Plugin not triggering?
A: Check: registration status, entity name, event type, permissions

### Q: Messages not sending?
A: Verify: credentials, token, phone format, network connectivity

### Q: Configuration errors?
A: Ensure: all keys present, no hardcoded secrets, env vars set

### Q: Performance issues?
A: Check: plugin is async, queries optimized, network latency

---

## Support

### Q: How do I get help?
A: Check [[Troubleshooting Guide|Troubleshooting-Guide]] or create a GitHub issue

### Q: Can I contribute?
A: Yes! See [[Contributing|Contributing]] guide

### Q: Is commercial support available?
A: Not officially, but community support available

### Q: What's the license?
A: MIT License - free for commercial use

---

For more answers, see [[Common Issues|Common-Issues]] or [[Troubleshooting Guide|Troubleshooting-Guide]].

