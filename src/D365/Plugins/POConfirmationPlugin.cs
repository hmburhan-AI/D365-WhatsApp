using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace D365.WhatsApp.Plugins
{
    /// <summary>
    /// Plugin to trigger PO confirmation on Purchase Order creation/confirmation in D365
    /// This plugin executes on the PurchaseOrderTable entity
    /// </summary>
    public class POConfirmationPlugin : IPlugin
    {
        private const string PluginName = "POConfirmationPlugin";

        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                // Obtain the execution context from the service provider
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                trace.Trace($"[{PluginName}] Starting execution");

                // Check if the target entity is PurchaseOrderTable
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];

                    if (entity.LogicalName != "PurchaseOrderTable")
                    {
                        trace.Trace($"[{PluginName}] Entity is not PurchaseOrderTable: {entity.LogicalName}");
                        return;
                    }

                    trace.Trace($"[{PluginName}] Processing PurchaseOrderTable");

                    // Get PO number
                    string poNumber = entity.GetAttributeValue<string>("PurchaseOrderNumber");
                    trace.Trace($"[{PluginName}] PO Number: {poNumber}");

                    // Get the full PO record
                    var poRecord = RetrievePurchaseOrder(service, poNumber, trace);

                    if (poRecord != null)
                    {
                        // Extract vendor information
                        string vendorAccountId = poRecord.GetAttributeValue<string>("VendorAccountNumber");
                        string vendorName = poRecord.GetAttributeValue<string>("VendorName");
                        
                        trace.Trace($"[{PluginName}] Vendor: {vendorName} ({vendorAccountId})");

                        // Retrieve vendor contact information including phone number
                        var vendorRecord = RetrieveVendor(service, vendorAccountId, trace);

                        if (vendorRecord != null)
                        {
                            string phoneNumber = vendorRecord.GetAttributeValue<string>("WhatsAppPhoneNumber");

                            if (!string.IsNullOrEmpty(phoneNumber))
                            {
                                trace.Trace($"[{PluginName}] Vendor phone: {phoneNumber}");

                                // Create a record in the WhatsApp Message Log for processing
                                CreateWhatsAppMessageLog(service, poNumber, phoneNumber, vendorName, trace);
                            }
                            else
                            {
                                trace.Trace($"[{PluginName}] No WhatsApp phone number found for vendor");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"[{PluginName}] Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves Purchase Order details
        /// </summary>
        private Entity RetrievePurchaseOrder(IOrganizationService service, string poNumber, ITracingService trace)
        {
            try
            {
                QueryExpression query = new QueryExpression("PurchaseOrderTable")
                {
                    ColumnSet = new ColumnSet("PurchaseOrderNumber", "VendorAccountNumber", "VendorName", "PurchaseOrderDate", "TotalAmount")
                };

                query.Criteria.AddCondition("PurchaseOrderNumber", ConditionOperator.Equal, poNumber);

                var result = service.RetrieveMultiple(query);

                if (result.Entities.Count > 0)
                {
                    return result.Entities[0];
                }

                trace.Trace($"[POConfirmationPlugin] PO not found: {poNumber}");
                return null;
            }
            catch (Exception ex)
            {
                trace.Trace($"[POConfirmationPlugin] Error retrieving PO: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Retrieves Vendor details
        /// </summary>
        private Entity RetrieveVendor(IOrganizationService service, string vendorAccountId, ITracingService trace)
        {
            try
            {
                QueryExpression query = new QueryExpression("VendorTable")
                {
                    ColumnSet = new ColumnSet("VendorAccountNumber", "Name", "WhatsAppPhoneNumber", "Email")
                };

                query.Criteria.AddCondition("VendorAccountNumber", ConditionOperator.Equal, vendorAccountId);

                var result = service.RetrieveMultiple(query);

                if (result.Entities.Count > 0)
                {
                    return result.Entities[0];
                }

                trace.Trace($"[POConfirmationPlugin] Vendor not found: {vendorAccountId}");
                return null;
            }
            catch (Exception ex)
            {
                trace.Trace($"[POConfirmationPlugin] Error retrieving vendor: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates a WhatsApp Message Log record for async processing
        /// </summary>
        private void CreateWhatsAppMessageLog(IOrganizationService service, string poNumber, string phoneNumber, string vendorName, ITracingService trace)
        {
            try
            {
                Entity messageLog = new Entity("WhatsAppMessageLog")
                {
                    ["PurchaseOrderNumber"] = poNumber,
                    ["RecipientPhoneNumber"] = phoneNumber,
                    ["VendorName"] = vendorName,
                    ["MessageType"] = "PO_Confirmation",
                    ["Status"] = "Pending",
                    ["CreatedDate"] = DateTime.UtcNow
                };

                service.Create(messageLog);
                trace.Trace($"[POConfirmationPlugin] WhatsApp message log created for PO: {poNumber}");
            }
            catch (Exception ex)
            {
                trace.Trace($"[POConfirmationPlugin] Error creating message log: {ex.Message}");
            }
        }
    }
}
