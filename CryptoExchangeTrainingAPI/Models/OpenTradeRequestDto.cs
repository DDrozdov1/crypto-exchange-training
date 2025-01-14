namespace CryptoExchangeTrainingAPI.Models
{
    public class OpenTradeRequestDto
    {
        public string Pair { get; set; } = null!; // Торговая пара
        public string Type { get; set; } = null!; // Тип сделки ("buy" или "sell")
        public int Leverage { get; set; } // Маржинальное плечо
        public decimal Amount { get; set; } // Объем сделки
        public decimal? StopLoss { get; set; } // Уровень Stop Loss
        public decimal? TakeProfit { get; set; } // Уровень Take Profit
    }
}