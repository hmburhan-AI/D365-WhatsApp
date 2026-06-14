# Troubleshooting Guide

## Table of Contents
1. [Common Issues](#common-issues)
2. [Configuration Issues](#configuration-issues)
3. [WhatsApp API Issues](#whatsapp-api-issues)
4. [D365 Plugin Issues](#d365-plugin-issues)
5. [Message Delivery Issues](#message-delivery-issues)
6. [Performance Issues](#performance-issues)
7. [FAQ](#faq)

## Common Issues

### Issue: "Invalid Phone Number"

**Symptoms**: Error message: "Invalid phone number format"

**Root Cause**: Phone number not in E.164 format

**Solution**:
```
Valid E.164 Format:
  +[country_code][number]
  Example: +11234567890
  Example: +442071838750

Invalid Formats:
  1-123-456-7890 ❌
  (123) 456-7890 ❌
  123 456 7890 ❌
```

**Action Steps**:
1. Check vendor phone number in D365
2. Ensure format is +[country_code][number]
3. Remove spaces, dashes, parentheses
4. Test with validation method:
   ```csharp
   var isValid = Regex.IsMatch(phoneNumber, @"^\+[1-9]\d{1,14}$");
   ```

---

### Issue: "Authentication Failed"

**Symptoms**: Error code 401, "Unauthorized" response

**Root Cause**: Invalid or expired access token

**Solution**:
1. Verify access token in configuration
2. Check token expiration (60 days)
3. Generate new token if expired
4. Test token:
   ```bash
   curl -X GET "https://graph.instagram.com/v18.0/me?access_token=YOUR_TOKEN"
   ```

---

### Issue: "Message Not Sent"

**Symptoms**: No error but message doesn't arrive

**Root Cause**: Multiple possible causes

**Debug Steps**:
1. Check message log status in D365
2. Verify phone number exists and has WhatsApp
3. Check rate limits not exceeded
4. Review error logs for details

---

## Configuration Issues

### Issue: "Key Not Found in Configuration"

**Error**:
```
System.InvalidOperationException: WhatsApp:PhoneNumberId is not configured
```

**Solution**:
```json
// Ensure appsettings.json has all required keys:
{
  "WhatsApp": {
    "PhoneNumberId": "YOUR_VALUE",
    "BusinessAccountId": "YOUR_VALUE",
    "AccessToken": "YOUR_VALUE",
    "ApiVersion": "v18.0"
  }
}
```

### Issue: "Connection String Invalid"

**Solution**:
```bash
# Check D365 connection
ConnectionString: 
  Server=tcp:your-instance.database.windows.net,1433;
  Initial Catalog=YourDatabase;
  Persist Security Info=False;
  User ID=your_username;
  Password=your_password;
```

---

## WhatsApp API Issues

### Issue: Error Code 100 - Invalid Request

**Possible Causes**:
- Missing required fields in message
- Invalid message format
- Phone number ID mismatch

**Solution**:
```csharp
// Verify message structure
var message = new WhatsAppMessage
{
    MessagingProduct = "whatsapp",      // Required
    RecipientType = "individual",       // Required
    RecipientPhoneNumber = "+11234567890", // Required
    MessageType = "text",               // Required
    Text = new TextMessage
    {
        Body = "Your message",          // Required, max 4096 chars
        PreviewUrl = true               // Optional
    }
};
```

### Issue: Error Code 190 - Authentication Error

**Solution**:
1. Verify access token format
2. Check token hasn't expired
3. Regenerate token in Meta Business Platform
4. Ensure token has correct permissions

### Issue: Error Code 550 - Rate Limit Exceeded

**Error**: "Too many requests to this account"

**Solution**:
```csharp
// Implement rate limiting
public class RateLimitHandler
{
    private int _requestCount = 0;
    private DateTime _resetTime = DateTime.UtcNow;
    private const int MaxRequests = 60; // Per minute

    public bool CanSend()
    {
        if (DateTime.UtcNow - _resetTime > TimeSpan.FromMinutes(1))
        {
            _requestCount = 0;
            _resetTime = DateTime.UtcNow;
        }

        if (_requestCount < MaxRequests)
        {
            _requestCount++;
            return true;
        }
        return false;
    }
}
```

---

## D365 Plugin Issues

### Issue: "Plugin Not Triggering"

**Symptoms**: PO created but no message log entry

**Debug Steps**:
1. Check plugin is registered and active
2. Verify plugin step execution:
   ```csharp
   // In Plugin Registration Tool
   - Entity: PurchaseOrderTable
   - Event: Create, Update
   - Stage: Post Operation
   - Mode: Asynchronous
   ```
3. Check plugin trace logs:
   ```
   Settings > Administration > Plugin Trace Log
   ```
4. Verify entity name matches exactly

### Issue: "Plugin Execution Timeout"

**Error**: "Plugin execution was aborted"

**Solution**:
1. Reduce plugin processing time
2. Offload to async job
3. Optimize queries:
   ```csharp
   // Good: Only needed columns
   var cols = new ColumnSet("PurchaseOrderNumber", "VendorAccountNumber");
   
   // Bad: All columns (slow)
   var cols = new ColumnSet(true);
   ```

### Issue: "Plugin Step Not Executing"

**Check**:
1. Entity logical name is correct
2. Event (Create/Update/Delete) is correct
3. Stage (Pre/Post) is correct
4. Check filter criteria if any
5. Verify user has permissions

---

## Message Delivery Issues

### Issue: "Message Status Stuck on Pending"

**Symptoms**: Message log shows "Pending" for hours

**Solution**:
1. Check service is running
2. Review error logs
3. Verify phone number is valid
4. Check network connectivity
5. Manually trigger processing:
   ```csharp
   var processor = new POConfirmationProcessor();
   var pending = processor.GetPendingMessages();
   foreach(var msg in pending)
   {
       await processor.ProcessAsync(msg);
   }
   ```

### Issue: "Webhook Not Receiving Updates"

**Symptoms**: Message shows as "Sent" but no "Delivered" status

**Solution**:
1. Verify webhook URL is publicly accessible
2. Check webhook configuration in Meta platform
3. Verify webhook token is correct
4. Check firewall/security settings
5. Review webhook logs

---

## Performance Issues

### Issue: "D365 PO Creation is Slow"

**Symptoms**: PO creation takes 30+ seconds

**Solution**:
1. Make plugin async (already done)
2. Optimize vendor lookup:
   ```csharp
   // Use index on VendorAccountNumber
   query.Criteria.AddCondition(
       "VendorAccountNumber", 
       ConditionOperator.Equal, 
       vendorId);
   ```
3. Cache vendor data
4. Reduce message log fields

### Issue: "High API Response Time"

**Symptoms**: Messages take 10+ seconds to send

**Solution**:
1. Check network latency
2. Increase timeout:
   ```csharp
   _httpClient.Timeout = TimeSpan.FromSeconds(60);
   ```
3. Implement connection pooling
4. Use CDN endpoints if available

---

## FAQ

### Q1: How often can I send messages?
A: WhatsApp limits are:
- **Bulk messages**: 60 per minute
- **Unique contacts**: No limit
- **Retry limit**: 3 attempts

### Q2: What happens if vendor doesn't have WhatsApp?
A: Error response from Meta API:
```json
{
  "error": {
    "message": "Phone number is not a valid WhatsApp number",
    "code": 1200
  }
}
```

### Q3: Can I customize the message template?
A: Yes, edit `templates/PO_Confirmation.template`

### Q4: How long are messages retained?
A: Based on your retention policy:
- Message log: Configurable (default 90 days)
- WhatsApp servers: 30 days

### Q5: Can I send documents/PDFs?
A: Yes, using document message type:
```csharp
message.MessageType = "document";
message.Document = new MediaMessage
{
    Link = "https://url-to-pdf.com/po.pdf",
    Caption = "Purchase Order"
};
```

### Q6: What if I need to resend a message?
A: Update message log status to "Pending" and reprocess

### Q7: How do I track message delivery?
A: Check `WhatsAppMessageLog` table:
- Status: Pending → Sent → Delivered → Read
- DeliveryTime: Timestamp of each status change

### Q8: Can I send to multiple vendors at once?
A: Yes, but respect rate limits (60/min)

### Q9: What error logging is available?
A: Multiple levels:
- D365 Plugin trace logs
- Application logs (configured in appsettings)
- WhatsApp API response logs
- Message delivery logs

### Q10: How to test without real vendor?
A: Use WhatsApp Business test account:
1. Register test phone number
2. Use test access token
3. Messages go to test number

---

## Emergency Contacts

### Meta WhatsApp Support
- **URL**: https://www.facebook.com/business/help
- **Issue**: API errors, rate limits

### D365 Support
- **URL**: https://dynamics.microsoft.com/en-us/support/
- **Issue**: Plugin errors, D365 issues

### Repository Issues
- **URL**: https://github.com/hmburhan-AI/D365-WhatsApp/issues
- **Report**: Bugs, feature requests

---

## Monitoring Checklist

- [ ] Check message log daily
- [ ] Monitor error rate < 5%
- [ ] Verify delivery within 5 seconds
- [ ] Check rate limit status
- [ ] Review access token expiration
- [ ] Validate phone number format
- [ ] Test message delivery
- [ ] Review API response times
- [ ] Check D365 plugin performance
- [ ] Verify webhook status

---

**Last Updated**: 2026-06-14  
**Version**: 1.0.0
