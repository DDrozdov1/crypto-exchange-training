using System;
using CryptoExchangeTrainingAPI.Models;

using System.Text.Json.Serialization;

namespace CryptoExchangeTrainingAPI.Models

{
    public class Trade
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string Pair { get; set; } = null!; 
        public string Type { get; set; } = null!; 
        public int Leverage { get; set; } 
        public decimal Amount { get; set; }
        public decimal EntryPrice { get; set; } 
        public decimal? ExitPrice { get; set; } 
        public decimal? StopLoss { get; set; } 
        public decimal? TakeProfit { get; set; }
        public decimal? ProfitLoss { get; set; } 
        public string Status { get; set; } = "open"; 
        public DateTime OpenedAt { get; set; } = DateTime.UtcNow; 
        public DateTime? ClosedAt { get; set; } 
        public decimal? Fee { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
    }
}