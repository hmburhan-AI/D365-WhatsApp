using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace D365.WhatsApp.Services.Models
{
    /// <summary>
    /// Response model from WhatsApp API
    /// </summary>
    public class WhatsAppResponse
    {
        [JsonProperty("messages")]
        public List<MessageStatus> Messages { get; set; }

        [JsonProperty("contacts")]
        public List<Contact> Contacts { get; set; }
    }

    /// <summary>
    /// Message status information
    /// </summary>
    public class MessageStatus
    {
        [JsonProperty("id")]
        public string MessageId { get; set; }

        [JsonProperty("message_status")]
        public string Status { get; set; } // accepted, sent, delivered, read, failed
    }

    /// <summary>
    /// Contact information
    /// </summary>
    public class Contact
    {
        [JsonProperty("input")]
        public string PhoneNumber { get; set; }

        [JsonProperty("wa_id")]
        public string WhatsAppId { get; set; }
    }

    /// <summary>
    /// Error response from WhatsApp API
    /// </summary>
    public class WhatsAppErrorResponse
    {
        [JsonProperty("error")]
        public ErrorDetail Error { get; set; }
    }

    /// <summary>
    /// Error details
    /// </summary>
    public class ErrorDetail
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("error_subcode")]
        public int ErrorSubcode { get; set; }
    }
}
