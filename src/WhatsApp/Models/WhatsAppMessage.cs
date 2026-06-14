using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace D365.WhatsApp.Services.Models
{
    /// <summary>
    /// WhatsApp Message Model for Meta Business API
    /// </summary>
    public class WhatsAppMessage
    {
        [JsonProperty("messaging_product")]
        public string MessagingProduct { get; set; } = "whatsapp";

        [JsonProperty("recipient_type")]
        public string RecipientType { get; set; } = "individual";

        [JsonProperty("to")]
        public string RecipientPhoneNumber { get; set; }

        [JsonProperty("type")]
        public string MessageType { get; set; }

        [JsonProperty("text")]
        public TextMessage Text { get; set; }

        [JsonProperty("document")]
        public MediaMessage Document { get; set; }

        [JsonProperty("image")]
        public MediaMessage Image { get; set; }

        [JsonProperty("template")]
        public TemplateMessage Template { get; set; }
    }

    /// <summary>
    /// Text message body
    /// </summary>
    public class TextMessage
    {
        [JsonProperty("preview_url")]
        public bool PreviewUrl { get; set; } = true;

        [JsonProperty("body")]
        public string Body { get; set; }
    }

    /// <summary>
    /// Media message (document, image, etc.)
    /// </summary>
    public class MediaMessage
    {
        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }
    }

    /// <summary>
    /// Template message
    /// </summary>
    public class TemplateMessage
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("language")]
        public LanguageMessage Language { get; set; }

        [JsonProperty("components")]
        public List<TemplateComponent> Components { get; set; }
    }

    /// <summary>
    /// Language settings for template
    /// </summary>
    public class LanguageMessage
    {
        [JsonProperty("code")]
        public string Code { get; set; } = "en"; // e.g., "en" for English
    }

    /// <summary>
    /// Template component (header, body, footer, buttons)
    /// </summary>
    public class TemplateComponent
    {
        [JsonProperty("type")]
        public string Type { get; set; } // header, body, footer, buttons

        [JsonProperty("parameters")]
        public List<TemplateParameter> Parameters { get; set; }
    }

    /// <summary>
    /// Template parameter
    /// </summary>
    public class TemplateParameter
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "text"; // text, image, document, video

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
