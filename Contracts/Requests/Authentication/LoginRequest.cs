using System.Text.Json.Serialization;

namespace Contracts.Requests.Authentication
{
    public class LoginRequest
    {
        [JsonPropertyName("blackbox")]
        public string Blackbox { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("locale")]
        public string Locale { get; set; } = string.Empty;

        [JsonPropertyName("installationId")]
        public string InstallationId { get; set; } = "e9b6a1b9-205b-4bb7-a2fa-6f44a3590398";
    }
}
