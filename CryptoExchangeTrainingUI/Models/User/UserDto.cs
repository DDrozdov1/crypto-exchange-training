namespace CryptoExchangeTrainingUI.Models.User
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty; // Уникальный идентификатор пользователя
        public string Email { get; set; } = string.Empty; // Email пользователя
        public DateTime CreatedAt { get; set; } // Время регистрации пользователя
        public decimal Balance { get; set; } // Баланс пользователя
        public DateTime? LastLoginAt { get; set; } // Время последнего входа
        public List<string> Roles { get; set; } = new(); // Роли пользователя
        public string Token { get; set; } = string.Empty; // JWT-токен
    }
}