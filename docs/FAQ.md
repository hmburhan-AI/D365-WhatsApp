# FAQ - Frequently Asked Questions

## General Questions

### Q1: What is this project about?
A: This is a complete implementation package for integrating Dynamics 365 Finance & Operations with Meta WhatsApp Business API to automatically send Purchase Order confirmations to vendors via WhatsApp.

### Q2: What are the benefits?
A:
- ✅ Automated vendor communication
- ✅ Reduced email delivery issues
- ✅ Real-time message delivery confirmation
- ✅ Improved vendor experience
- ✅ Better audit trail
- ✅ Cost reduction in communications

### Q3: Is this open source?
A: Yes! MIT License - free for personal and commercial use.

### Q4: What are the system requirements?
A:
- Dynamics 365 Finance & Operations v10.0+
- Meta WhatsApp Business Account
- .NET Framework 4.7.2+
- Azure subscription (optional, for Key Vault)

---

## Setup & Installation

### Q5: How long does setup take?
A: Approximately 2-3 days for complete implementation:
- WhatsApp setup: 1 day
- D365 customization: 1 day
- Code deployment: 0.5 day
- Testing: 0.5 day

### Q6: What if I don't have a WhatsApp Business Account?
A:
1. Visit [Meta Business](https://business.facebook.com)
2. Sign up for Meta Business Account
3. Register business phone number
4. Get API credentials
5. Estimated time: 1-2 days

### Q7: Can I use my personal phone number?
A: No. WhatsApp Business API requires a dedicated business phone number, which must be verified.

### Q8: How much does WhatsApp API cost?
A: Based on message usage:
- First 1,000 messages/month: Free
- Additional: ~$0.05 per message
- See [Meta Pricing](https://www.whatsapp.com/business/pricing/) for details

### Q9: Do I need Azure for this solution?
A: Optional. You can:
- Use appsettings.json files (development)
- Use Azure Key Vault (production - recommended)
- Use other secret management solutions

### Q10: What if my D365 environment is on-premises?
A: This solution works with both cloud and on-premises D365, but you'll need:
- Network connectivity for WhatsApp API calls
- HTTPS access to internet
- Proper firewall rules

---

## Technical Questions

### Q11: What .NET version does this use?
A: .NET Framework 4.7.2 (required for D365 plugins)

### Q12: Can I customize the message template?
A: Yes! Edit the template file:
```
templates/PO_Confirmation.template
```
Or modify the `GenerateMessageAsync` method in `POConfirmationService.cs`

### Q13: How do I handle different languages?
A: The message template supports any language. Simply:
1. Update the template text for your language
2. Implement language detection if needed
3. Create multiple templates per language

### Q14: What if the vendor doesn't have WhatsApp?
A: The API returns an error:
```
"Phone number is not a valid WhatsApp number"
```
The message is marked as "Failed" in the log, and you can implement fallback to email.

### Q15: How do I handle phone number variations?
A: Use the validation method which enforces E.164 format:
```csharp
var isValid = await _whatsAppService.ValidatePhoneNumberAsync(phoneNumber);
```
Accepted format: `+[country_code][number]`

---

## Security & Privacy

### Q16: How is my access token stored?
A: Multiple options:
- Development: `appsettings.json` (LOCAL DEVELOPMENT ONLY)
- Production: Azure Key Vault (recommended)
- Alternative: Environment variables

### Q17: Is phone data encrypted?
A: Yes:
- In transit: HTTPS (enforced)
- At rest: Database encryption (D365 standard)
- In logs: Phone numbers are masked

### Q18: Is this GDPR compliant?
A: The solution includes GDPR-ready features:
- Phone number masking in logs
- Data retention policies
- Audit trail
- But you must configure your own data retention policy

### Q19: How are messages logged?
A: In the `WhatsAppMessageLog` table:
- Message content: Full
- Vendor phone: Stored (masked in logs)
- Status: Tracked
- Timestamps: All events recorded
- Retention: Configurable (default 90 days)

### Q20: Can vendors see my phone number?
A: No. They see:
- Your business name (configured in WhatsApp)
- Your business phone number (the one you registered)
- Not your D365 environment details

---

## Functionality

### Q21: When is the message sent?
A: On PO confirmation:
1. User confirms PO in D365
2. Plugin triggers
3. Message queued
4. Background job sends message
5. Typically within 5-10 seconds

### Q22: Can I send documents/PDFs with the PO?
A: Yes, using document message type:
```csharp
message.Document = new MediaMessage
{
    Link = "https://url-to-pdf.com/po.pdf",
    Caption = "Purchase Order Document"
};
```

### Q23: How do I track if vendor received the message?
A: Three message statuses:
- **Sent**: Message reached WhatsApp server
- **Delivered**: Delivered to vendor's device
- **Read**: Vendor opened the message

Optional: Set up webhook for real-time updates

### Q24: Can I resend a message?
A: Yes:
1. Update message log status to "Pending"
2. Reprocess the message
3. New message ID generated

### Q25: How many messages can I send per minute?
A: API limit: 60 messages/minute
Best practice: Send in batches with small delays

---

## Troubleshooting

### Q26: Messages not sending - what to check?
A:
1. ✅ Vendor phone number in E.164 format?
2. ✅ WhatsApp Business Account active?
3. ✅ Phone number verified in WhatsApp?
4. ✅ Access token valid?
5. ✅ Network connectivity?
6. ✅ Check error logs in D365

### Q27: D365 PO creation is slow after deployment
A: Likely plugin delay. Verify:
1. Plugin is async (should be)
2. No sync plugin errors
3. Plugin timeout set correctly
4. Check system performance

### Q28: How do I test without real vendors?
A: Options:
1. Use WhatsApp test account
2. Use your own test phone number
3. Set up sandbox environment
4. Mock WhatsApp API in development

### Q29: What if I get "Rate Limit Exceeded"?
A: API limit reached (60/minute). Solution:
1. Implement message queue
2. Batch send with delays
3. Distribute sending over time
4. Consider throttling

### Q30: How do I report a bug?
A: Create an issue on GitHub:
1. Go to [Issues](https://github.com/hmburhan-AI/D365-WhatsApp/issues)
2. Click "New Issue"
3. Provide details:
   - Error message
   - Steps to reproduce
   - Environment details
   - Expected vs actual behavior

---

## Performance & Optimization

### Q31: How long should message delivery take?
A: Typical timeline:
- D365 to service: < 1s
- Service processing: < 1s
- API call: < 2s
- WhatsApp delivery: < 2s
- **Total: < 5 seconds**

### Q32: Can I batch send multiple POs?
A: Yes, but respect rate limits:
```csharp
foreach(var po in purchaseOrders)
{
    await SendPOConfirmationAsync(po);
    await Task.Delay(1000); // 1 second delay
}
```

### Q33: How do I monitor performance?
A: Use provided SQL queries:
```sql
-- Average delivery time
SELECT AVG(DATEDIFF(SECOND, CreatedDate, SentDate)) 
FROM WhatsAppMessageLog
WHERE Status = 'Sent'

-- Success rate
SELECT COUNT(*) * 100.0 / 
       (SELECT COUNT(*) FROM WhatsAppMessageLog) 
FROM WhatsAppMessageLog 
WHERE Status IN ('Sent', 'Delivered', 'Read')
```

### Q34: What's the maximum message size?
A: WhatsApp limits:
- Text: 4,096 characters
- Document: 100 MB
- Image: 5 MB

### Q35: Can I scale this to millions of vendors?
A: Yes with considerations:
- Implement message queue (Azure Service Bus)
- Use batch processing
- Monitor API rate limits
- Distribute load over time

---

## Support & Community

### Q36: How do I get help?
A: Multiple options:
1. Check [Troubleshooting Guide](TROUBLESHOOTING.md)
2. Check [SETUP.md](SETUP.md)
3. Create GitHub issue
4. Review documentation
5. Check discussion forum

### Q37: Can I contribute to this project?
A: Yes! See [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md):
1. Fork repository
2. Create feature branch
3. Make changes
4. Create pull request
5. Follow code standards

### Q38: How often is this updated?
A: 
- Bug fixes: As needed
- Minor updates: Monthly
- Major updates: Quarterly
- Follow GitHub releases for updates

### Q39: Is commercial support available?
A: This is open source without official commercial support. For support:
1. Community forum
2. GitHub discussions
3. Create detailed issues
4. Consult documentation

### Q40: Can I use this in production?
A: Yes! This is production-ready:
- ✅ Comprehensive error handling
- ✅ Security best practices
- ✅ Performance optimized
- ✅ Fully documented
- ✅ Tested thoroughly

But follow implementation checklist first.

---

## License & Legal

### Q41: What license is this?
A: MIT License - see [LICENSE](../LICENSE) file

### Q42: Can I use this commercially?
A: Yes! MIT License allows commercial use.

### Q43: Do I need to give credit?
A: Not required, but appreciated!

### Q44: Can I modify the code?
A: Yes! MIT License allows modifications.

### Q45: What's the warranty?
A: None. MIT License provides "AS IS" without warranties. Use at your own risk in production.

---

## More Questions?

If your question isn't answered here:

1. 📖 Check the [full documentation](./)
2. 🐛 Search [existing issues](https://github.com/hmburhan-AI/D365-WhatsApp/issues)
3. 💬 Start a [discussion](https://github.com/hmburhan-AI/D365-WhatsApp/discussions)
4. 📝 Create a [new issue](https://github.com/hmburhan-AI/D365-WhatsApp/issues/new)

---

**Last Updated**: 2026-06-14  
**Version**: 1.0.0
