using System;
using Microsoft.Extensions.Configuration;

namespace D365.WhatsApp.Configuration
{
    /// <summary>
    /// WhatsApp Configuration holder
    /// </summary>
    public class WhatsAppConfig
    {
        public string PhoneNumberId { get; set; }
        public string BusinessAccountId { get; set; }
        public string AccessToken { get; set; }
        public string ApiVersion { get; set; }
        public int TimeoutSeconds { get; set; }
        public bool EnableLogging { get; set; }
        public string LogFilePath { get; set; }

        /// <summary>
        /// Load configuration from IConfiguration
        /// </summary>
        public static WhatsAppConfig Load(IConfiguration config)
        {
            return new WhatsAppConfig
            {
                PhoneNumberId = config["WhatsApp:PhoneNumberId"] ?? throw new InvalidOperationException("WhatsApp:PhoneNumberId is not configured"),
                BusinessAccountId = config["WhatsApp:BusinessAccountId"] ?? throw new InvalidOperationException("WhatsApp:BusinessAccountId is not configured"),
                AccessToken = config["WhatsApp:AccessToken"] ?? throw new InvalidOperationException("WhatsApp:AccessToken is not configured"),
                ApiVersion = config["WhatsApp:ApiVersion"] ?? "v18.0",
                TimeoutSeconds = int.Parse(config["WhatsApp:TimeoutSeconds"] ?? "30"),
                EnableLogging = bool.Parse(config["WhatsApp:EnableLogging"] ?? "true"),
                LogFilePath = config["WhatsApp:LogFilePath"] ?? "logs/whatsapp.log"
            };
        }
    }
}
