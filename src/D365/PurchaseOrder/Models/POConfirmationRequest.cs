using System;

namespace D365.WhatsApp.PurchaseOrder.Models
{
    /// <summary>
    /// Request model for Purchase Order Confirmation via WhatsApp
    /// </summary>
    public class POConfirmationRequest
    {
        /// <summary>
        /// Unique Request ID for tracking
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Purchase Order Data
        /// </summary>
        public PurchaseOrderModel PurchaseOrder { get; set; }

        /// <summary>
        /// Include PDF attachment in WhatsApp message
        /// </summary>
        public bool IncludeAttachment { get; set; }

        /// <summary>
        /// Message template to use
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// Custom message body (if not using template)
        /// </summary>
        public string CustomMessage { get; set; }

        /// <summary>
        /// Request timestamp
        /// </summary>
        public DateTime RequestTimestamp { get; set; }

        /// <summary>
        /// Requester User ID
        /// </summary>
        public string RequestedBy { get; set; }

        public POConfirmationRequest()
        {
            RequestId = Guid.NewGuid().ToString();
            RequestTimestamp = DateTime.UtcNow;
            IncludeAttachment = true;
        }
    }
}
