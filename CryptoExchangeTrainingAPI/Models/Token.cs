namespace CryptoExchangeTrainingAPI.Models
{
    public class Token
    {
        public int Id { get; set; } // Уникальный идентификатор токена
        public string Name { get; set; } = string.Empty; // Название токена (например, BTC, ETH)
        public string Symbol { get; set; } = string.Empty; // Символ токена (например, BTC, ETH)

        public ICollection<UserAsset> UserAssets { get; set; } = new List<UserAsset>(); // Связь с активами пользователей
    }
}