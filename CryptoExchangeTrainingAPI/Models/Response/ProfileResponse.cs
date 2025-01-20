namespace CryptoExchangeTrainingAPI.Models.Response
{
    public class ProfileResponse
    {
        public string Email { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }  // добавляем ?
        public List<AssetResponse> Assets { get; set; }
    }
}
