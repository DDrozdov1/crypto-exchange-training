using System;
using CryptoExchangeTrainingAPI.Models;

using System.Text.Json.Serialization;

namespace CryptoExchangeTrainingAPI.Models

{
    public class Trade
    {
        public int Id { get; set; } // Уникальный идентификатор сделки
        public string UserId { get; set; } = null!;// Внешний ключ на пользователя
        public string Pair { get; set; } = null!; // Торговая пара (например, BTC/USDT)
        public string Type { get; set; } = null!; // Тип сделки ("buy" или "sell")
        public int Leverage { get; set; } // Маржинальное плечо
        public decimal Amount { get; set; } // Объем сделки (например, 0.01 BTC)
        public decimal EntryPrice { get; set; } // Цена открытия сделки
        public decimal? ExitPrice { get; set; } // Цена закрытия сделки (если сделка закрыта)
        public decimal? StopLoss { get; set; } // Уровень Stop Loss
        public decimal? TakeProfit { get; set; } // Уровень Take Profit
        public decimal? ProfitLoss { get; set; } // Прибыль/убыток сделки
        public string Status { get; set; } = "open"; // Статус сделки ("open" или "closed")
        public DateTime OpenedAt { get; set; } = DateTime.UtcNow; // Время открытия сделки
        public DateTime? ClosedAt { get; set; } // Время закрытия сделки
        public decimal? Fee { get; set; }

        // Связь с пользователем
        [JsonIgnore]
        public User? User { get; set; }
    }
}