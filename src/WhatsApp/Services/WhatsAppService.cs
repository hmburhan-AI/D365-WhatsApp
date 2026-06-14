using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using D365.WhatsApp.Services.Models;

namespace D365.WhatsApp.Services
{
    /// <summary>
    /// WhatsApp Business API Service for sending messages
    /// </summary>
    public interface IWhatsAppService
    {
        Task<WhatsAppResponse> SendMessageAsync(WhatsAppMessage message);
        Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
    }

    /// <summary>
    /// Implementation of WhatsApp Service
    /// </summary>
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly string _phoneNumberId;
        private readonly string _accessToken;
        private readonly string _apiVersion;
        private readonly ILogger<WhatsAppService> _logger;

        /// <summary>
        /// Constructor for WhatsAppService
        /// </summary>
        public WhatsAppService(
            HttpClient httpClient,
            string phoneNumberId,
            string accessToken,
            string apiVersion,
            ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _phoneNumberId = phoneNumberId ?? throw new ArgumentNullException(nameof(phoneNumberId));
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            _apiVersion = apiVersion ?? "v18.0";
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Sends a message via WhatsApp Business API
        /// </summary>
        public async Task<WhatsAppResponse> SendMessageAsync(WhatsAppMessage message)
        {
            try
            {
                _logger.LogInformation($"Sending WhatsApp message to {message.RecipientPhoneNumber}");

                var url = $"https://graph.instagram.com/{_apiVersion}/{_phoneNumberId}/messages";

                var content = new StringContent(
                    JsonConvert.SerializeObject(message),
                    Encoding.UTF8,
                    "application/json");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Message sent successfully to {message.RecipientPhoneNumber}");
                    var result = JsonConvert.DeserializeObject<WhatsAppResponse>(responseContent);
                    return result;
                }
                else
                {
                    _logger.LogError($"Failed to send message. Status: {response.StatusCode}, Response: {responseContent}");
                    var errorResponse = JsonConvert.DeserializeObject<WhatsAppErrorResponse>(responseContent);
                    throw new Exception($"WhatsApp API Error: {errorResponse?.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending WhatsApp message: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Validates phone number format
        /// </summary>
        public async Task<bool> ValidatePhoneNumberAsync(string phoneNumber)
        {
            try
            {
                _logger.LogInformation($"Validating phone number: {phoneNumber}");

                if (string.IsNullOrEmpty(phoneNumber))
                {
                    _logger.LogWarning("Phone number is empty");
                    return false;
                }

                // E.164 format validation: +[1-9]{1}[0-9]{1,14}
                var e164Pattern = @"^\+[1-9]\d{1,14}$";
                var isValid = System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, e164Pattern);

                if (!isValid)
                {
                    _logger.LogWarning($"Phone number {phoneNumber} is not in valid E.164 format");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validating phone number: {ex.Message}", ex);
                return false;
            }
        }
    }
}
