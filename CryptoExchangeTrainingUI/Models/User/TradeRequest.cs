using System.ComponentModel.DataAnnotations;

namespace CryptoExchangeTrainingUI.Models.User
{
    public class TradeRequest
    {
        [Required]
        public string Pair { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [Range(1, 100)]
        public int Leverage { get; set; }

        public decimal? StopLoss { get; set; }
        public decimal? TakeProfit { get; set; }
    }
}