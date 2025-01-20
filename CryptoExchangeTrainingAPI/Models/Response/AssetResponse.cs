namespace CryptoExchangeTrainingAPI.Models.Response
{
    public class AssetResponse
    {
        public string Symbol { get; set; }
        public decimal Quantity { get; set; }
        public decimal CurrentPrice { get; set; }
    }
}
