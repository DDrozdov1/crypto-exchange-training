using System;
using CryptoExchangeTrainingAPI.Models;

namespace CryptoExchangeTrainingAPI.Models
{
    public class UserAsset
    {
        public int Id { get; set; } // Уникальный идентификатор
        public string UserId { get; set; } = null!; // Внешний ключ на пользователя
        public string Asset { get; set; } = null!; // Название актива (например, BTC, USDT)
        public decimal Balance { get; set; } = 0.00m; // Баланс актива
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Время последнего обновления баланса

        // Связь с пользователем
        public User? User { get; set; }
        public Token? Token { get; set; }

    }
}