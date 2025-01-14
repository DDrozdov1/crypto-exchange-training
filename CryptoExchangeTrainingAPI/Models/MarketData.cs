using System;

namespace CryptoExchangeTrainingAPI.Models
{
    public class MarketData
    {
        public int Id { get; set; } // Уникальный идентификатор записи
        public string Pair { get; set; } = null!; // Торговая пара
        public decimal Price { get; set; } // Цена криптовалюты
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Время записи
    }
}