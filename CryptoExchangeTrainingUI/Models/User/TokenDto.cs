namespace CryptoExchangeTrainingUI.Models.User
{
    public class TokenDto
    {
        public string Symbol { get; set; } = string.Empty; // Символ токена (например, BTC)
        public string Name { get; set; } = string.Empty; // Название токена
        public decimal Price { get; set; } // Добавлено поле для цены

    }
}
