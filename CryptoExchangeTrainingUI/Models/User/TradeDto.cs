namespace CryptoExchangeTrainingUI.Models.User
{
    public class TradeDto
    {
        public int Id { get; set; }
        public string Pair { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "buy" или "sell"
        public int Leverage { get; set; }
        public decimal Amount { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal? StopLoss { get; set; }
        public decimal? TakeProfit { get; set; }
        public string Status { get; set; } = string.Empty; // "open" или "closed"
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}