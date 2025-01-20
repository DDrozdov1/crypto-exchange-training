namespace CryptoExchangeTrainingUI.Models.User
{
    public class TokenDto
    {
        public string Symbol { get; set; } = string.Empty; // Символ токена (BTC, ETH)
        public string Name { get; set; } = string.Empty; // Название токена (Bitcoin, Ethereum)
        public decimal Price { get; set; } // Цена токена
    }
}