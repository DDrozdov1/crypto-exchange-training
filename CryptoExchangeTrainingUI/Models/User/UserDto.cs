namespace CryptoExchangeTrainingUI.Models.User
{
    public class UserDto
    {
        public string Id { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public List<string> Roles { get; init; } = new();
        public string Token { get; set; } = string.Empty;

    }
}
