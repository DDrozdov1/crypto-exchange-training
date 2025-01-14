namespace CryptoExchangeTrainingAPI.Models
{
    public class TradeDto
    {
        public int Id { get; set; }
        public string Pair { get; set; } = null!;
        public string Type { get; set; } = null!;
        public int Leverage { get; set; }
        public decimal Amount { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal? StopLoss { get; set; }
        public decimal? TakeProfit { get; set; }
        public string Status { get; set; } = "open";
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
