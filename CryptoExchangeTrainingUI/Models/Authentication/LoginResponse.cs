using System.Text.Json.Serialization;

namespace CryptoExchangeTrainingUI.Models.Authentication
{
    public class LoginResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; init; } = string.Empty;
    }
}
