using System;

namespace CryptoExchangeTrainingAPI.Models
{
    public class HistoricalData
    {
        public int Id { get; set; }
        public string Pair { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public decimal Price { get; set; }
    }
}