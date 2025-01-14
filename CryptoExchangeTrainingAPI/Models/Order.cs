using System;
using CryptoExchangeTrainingAPI.Models;

namespace CryptoExchangeTrainingAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;// Внешний ключ на пользователя
        public string Pair { get; set; } = null!;
        public string Type { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Связь с пользователем
        public User? User { get; set; }
    }
}