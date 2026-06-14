using System;
using System.Collections.Generic;

namespace D365.WhatsApp.PurchaseOrder.Models
{
    /// <summary>
    /// Represents a Purchase Order entity from D365 Finance & Operations
    /// </summary>
    public class PurchaseOrderModel
    {
        /// <summary>
        /// Purchase Order Number
        /// </summary>
        public string PurchaseOrderId { get; set; }

        /// <summary>
        /// Vendor Account Number
        /// </summary>
        public string VendorAccountNumber { get; set; }

        /// <summary>
        /// Vendor Name
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Vendor WhatsApp Phone Number (E.164 format: +1234567890)
        /// </summary>
        public string VendorPhoneNumber { get; set; }

        /// <summary>
        /// Purchase Order Date
        /// </summary>
        public DateTime PurchaseOrderDate { get; set; }

        /// <summary>
        /// Delivery Address
        /// </summary>
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// Expected Delivery Date
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// Total PO Amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Currency Code (e.g., USD, EUR)
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Purchase Order Lines/Items
        /// </summary>
        public List<PurchaseOrderLineModel> Lines { get; set; }

        /// <summary>
        /// Contact Person Name
        /// </summary>
        public string ContactPersonName { get; set; }

        /// <summary>
        /// Payment Terms
        /// </summary>
        public string PaymentTerms { get; set; }

        /// <summary>
        /// Special Instructions or Notes
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Purchase Order Status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Company/Legal Entity ID
        /// </summary>
        public string LegalEntityId { get; set; }

        /// <summary>
        /// Document PDF URL or Base64 encoded content
        /// </summary>
        public string DocumentUrl { get; set; }

        /// <summary>
        /// Document File Path
        /// </summary>
        public string DocumentFilePath { get; set; }

        public PurchaseOrderModel()
        {
            Lines = new List<PurchaseOrderLineModel>();
        }
    }

    /// <summary>
    /// Represents a line item in a Purchase Order
    /// </summary>
    public class PurchaseOrderLineModel
    {
        /// <summary>
        /// Line Number
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Item/Product Number
        /// </summary>
        public string ItemNumber { get; set; }

        /// <summary>
        /// Product Name/Description
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Quantity Ordered
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit of Measurement
        /// </summary>
        public string UnitOfMeasurement { get; set; }

        /// <summary>
        /// Unit Price
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Line Total Amount
        /// </summary>
        public decimal LineTotal { get; set; }
    }
}
