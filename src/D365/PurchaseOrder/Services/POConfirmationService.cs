using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using D365.WhatsApp.PurchaseOrder.Models;
using D365.WhatsApp.Services;
using D365.WhatsApp.Services.Models;

namespace D365.WhatsApp.PurchaseOrder.Services
{
    /// <summary>
    /// Interface for Purchase Order Confirmation Service
    /// </summary>
    public interface IPOConfirmationService
    {
        Task<bool> SendPOConfirmationAsync(POConfirmationRequest request);
        Task<string> GenerateMessageAsync(PurchaseOrderModel po);
    }

    /// <summary>
    /// Service to handle Purchase Order Confirmation via WhatsApp
    /// </summary>
    public class POConfirmationService : IPOConfirmationService
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<POConfirmationService> _logger;

        public POConfirmationService(
            IWhatsAppService whatsAppService,
            ILogger<POConfirmationService> logger)
        {
            _whatsAppService = whatsAppService ?? throw new ArgumentNullException(nameof(whatsAppService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Sends Purchase Order confirmation via WhatsApp
        /// </summary>
        public async Task<bool> SendPOConfirmationAsync(POConfirmationRequest request)
        {
            try
            {
                if (request == null || request.PurchaseOrder == null)
                {
                    _logger.LogError("Invalid PO confirmation request");
                    return false;
                }

                var po = request.PurchaseOrder;

                // Validate vendor phone number
                var isValidPhone = await _whatsAppService.ValidatePhoneNumberAsync(po.VendorPhoneNumber);
                if (!isValidPhone)
                {
                    _logger.LogError($"Invalid vendor phone number: {po.VendorPhoneNumber}");
                    return false;
                }

                _logger.LogInformation($"Sending PO confirmation for {po.PurchaseOrderId} to {po.VendorPhoneNumber}");

                // Generate message
                var messageBody = await GenerateMessageAsync(po);

                // Create WhatsApp message
                var whatsAppMessage = new WhatsAppMessage
                {
                    RecipientPhoneNumber = po.VendorPhoneNumber,
                    MessageType = "text",
                    Text = new TextMessage
                    {
                        Body = messageBody,
                        PreviewUrl = true
                    }
                };

                // Send message
                var response = await _whatsAppService.SendMessageAsync(whatsAppMessage);

                if (response?.Messages?.Count > 0)
                {
                    _logger.LogInformation($"PO confirmation sent successfully. Message ID: {response.Messages[0].MessageId}");
                    return true;
                }

                _logger.LogError("Failed to send PO confirmation - no message ID received");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending PO confirmation: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Generates a formatted message for the Purchase Order
        /// </summary>
        public async Task<string> GenerateMessageAsync(PurchaseOrderModel po)
        {
            try
            {
                var message = new System.Text.StringBuilder();

                message.AppendLine("🛒 *Purchase Order Confirmation*");
                message.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━━━");
                message.AppendLine();
                message.AppendLine($"*PO Number:* {po.PurchaseOrderId}");
                message.AppendLine($"*Vendor:* {po.VendorName}");
                message.AppendLine($"*Date:* {po.PurchaseOrderDate:dd/MM/yyyy}");
                message.AppendLine();
                message.AppendLine($"📦 *Order Details:*");
                message.AppendLine($"Items Count: {po.Lines.Count}");
                message.AppendLine($"Total Amount: {po.TotalAmount:C} {po.CurrencyCode}");
                message.AppendLine();

                if (po.DeliveryDate.HasValue)
                {
                    message.AppendLine($"📅 *Expected Delivery:* {po.DeliveryDate:dd/MM/yyyy}");
                }

                message.AppendLine();
                message.AppendLine($"📍 *Delivery Address:*");
                message.AppendLine(po.DeliveryAddress);

                if (!string.IsNullOrEmpty(po.PaymentTerms))
                {
                    message.AppendLine();
                    message.AppendLine($"💳 *Payment Terms:* {po.PaymentTerms}");
                }

                if (!string.IsNullOrEmpty(po.ContactPersonName))
                {
                    message.AppendLine();
                    message.AppendLine($"👤 *Contact Person:* {po.ContactPersonName}");
                }

                message.AppendLine();
                message.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━━━");
                message.AppendLine($"Please confirm receipt of this PO.");

                if (!string.IsNullOrEmpty(po.Notes))
                {
                    message.AppendLine();
                    message.AppendLine($"*Notes:* {po.Notes}");
                }

                return await Task.FromResult(message.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating message: {ex.Message}", ex);
                throw;
            }
        }
    }
}
