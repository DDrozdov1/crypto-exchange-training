using System.Text.Json.Serialization;
using CryptoExchangeTrainingUI.Models.User;

namespace CryptoExchangeTrainingUI.Models.Authentication
{
    public class AuthResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; init; } = string.Empty;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; init; } = string.Empty;

        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; init; } = string.Empty;

        [JsonPropertyName("errors")]
        public List<string> Errors { get; init; } = new();
    }

}
