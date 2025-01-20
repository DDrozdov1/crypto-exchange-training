namespace CryptoExchangeTrainingUI.Models.User
{
    public class UserAssetDto
    {
        public string Asset { get; set; } = string.Empty; // Название криптовалюты (BTC, ETH)
        public decimal Balance { get; set; } // Баланс пользователя
    }
}