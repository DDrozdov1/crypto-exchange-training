namespace CryptoExchangeTrainingUI.Models.User
{
    public class UserDto
    {
        public string Email { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string Token { get; set; }
    }
}