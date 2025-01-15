using System.Text.Json.Serialization;

namespace CryptoExchangeTrainingUI.Models.Authentication
{
    public class ErrorDetails
    {
        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; init; }
    }
}
