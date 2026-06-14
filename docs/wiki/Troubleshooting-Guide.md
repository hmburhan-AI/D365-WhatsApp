# Troubleshooting Guide

## Common Issues & Solutions

---

## Issue: Invalid Phone Number

**Error**: "Invalid phone number format"

**Solution**:
- Phone must be in E.164 format: `+[country_code][number]`
- Valid: `+11234567890`
- Invalid: `1-123-456-7890`

**Fix**:
1. Check vendor phone in D365
2. Remove spaces, dashes, parentheses
3. Add country code with +
4. Test with validation method

---

## Issue: Authentication Failed

**Error**: "401 Unauthorized"

**Causes**:
- Invalid or expired access token
- Incorrect Phone Number ID
- Missing Bearer token

**Solution**:
1. Verify access token in configuration
2. Check token expiration (60 days)
3. Generate new token if expired
4. Test token:
   ```bash
   curl -X GET "https://graph.instagram.com/v18.0/me?access_token=YOUR_TOKEN"
   ```

---

## Issue: Plugin Not Triggering

**Symptoms**: PO created but no message log entry

**Check**:
1. Plugin is registered and active
2. Entity name: `PurchaseOrderTable` (exact match)
3. Event: Create/Update
4. Stage: Post Operation
5. Check plugin trace logs in D365

**Solution**:
1. Open Plugin Registration Tool
2. Verify plugin registration
3. Check plugin step configuration
4. Review D365 plugin trace log
5. Re-register if needed

---

## Issue: Messages Not Sending

**Symptoms**: No error but message doesn't arrive

**Debug Steps**:
1. Check message log status
2. Verify vendor WhatsApp account exists
3. Check API rate limits not exceeded
4. Review error logs
5. Check network connectivity

**Solution**:
```sql
SELECT TOP 10 
    PurchaseOrderNumber, 
    Status, 
    ErrorMessage, 
    CreatedDate
FROM WhatsAppMessageLog
ORDER BY CreatedDate DESC
```

---

## Issue: Rate Limit Exceeded

**Error**: "Error 550 - Too many requests"

**Cause**: Exceeded 60 messages/minute limit

**Solution**:
1. Implement message queue
2. Batch send with delays
3. Distribute sending over time
4. Use Azure Service Bus for scalability

```csharp
foreach(var po in purchaseOrders)
{
    await SendAsync(po);
    await Task.Delay(1000); // 1 second delay
}
```

---

## Issue: D365 PO Creation is Slow

**Cause**: Plugin delays

**Solution**:
1. Verify plugin is async (should be)
2. Optimize vendor lookup queries
3. Check system performance
4. Use indexes on VendorAccountNumber

---

## Issue: Message Stuck on Pending

**Symptoms**: Message status is "Pending" for hours

**Solution**:
1. Check if processing service is running
2. Review error logs
3. Verify network connectivity
4. Manually trigger processing
5. Check API availability

---

## Issue: Webhook Not Receiving Updates

**Symptoms**: Messages show "Sent" but no "Delivered" status

**Solution**:
1. Verify webhook URL is publicly accessible
2. Check webhook configuration in Meta platform
3. Verify webhook token is correct
4. Check firewall/security settings
5. Review webhook logs

---

## Issue: Configuration Key Not Found

**Error**: "WhatsApp:PhoneNumberId is not configured"

**Solution**:
```json
{
  "WhatsApp": {
    "PhoneNumberId": "YOUR_VALUE",
    "BusinessAccountId": "YOUR_VALUE",
    "AccessToken": "YOUR_VALUE",
    "ApiVersion": "v18.0"
  }
}
```

---

## Issue: Plugin Execution Timeout

**Error**: "Plugin execution was aborted"

**Solution**:
1. Reduce plugin processing time
2. Offload to async job
3. Optimize queries (only needed columns)
4. Check system performance

---

## Performance Issues

### High API Response Time
- Check network latency
- Increase timeout settings
- Use connection pooling
- Monitor API status

### D365 Slowdown
- Plugin should be async
- Check for blocking queries
- Monitor system resources
- Review plugin execution time

---

## Error Codes Reference

| Code | Meaning | Solution |
|------|---------|----------|
| 100 | Invalid Request | Check message format |
| 190 | Authentication Error | Verify access token |
| 400 | Invalid Phone | Use E.164 format |
| 404 | Not Found | Check phone number ID |
| 550 | Rate Limit | Implement batching |

---

## Getting Help

1. Check [[FAQ|FAQ]] for common questions
2. Review [[Common Issues|Common-Issues]]
3. Search documentation
4. Create GitHub issue with:
   - Error message
   - Steps to reproduce
   - Environment details
   - Expected vs actual behavior

---

## Related Pages

- [[Setup Instructions|Setup-Instructions]] - Setup help
- [[Configuration|Configuration]] - Configuration issues
- [[API Reference|API-Reference]] - API documentation
- [[Monitoring|Monitoring]] - Monitoring setup

