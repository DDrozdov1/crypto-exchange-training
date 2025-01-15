namespace CryptoExchangeTrainingUI.Models.User
{
    public class UserAssetDto
    {
        public string Asset { get; set; } = string.Empty; // Название криптовалюты (например, BTC)
        public decimal Balance { get; set; } // Баланс
        public decimal DepositAmount { get; set; } // Для пополнения
    }
}