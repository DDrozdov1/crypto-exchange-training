namespace CryptoExchangeTrainingUI.Models.User
{
    public class DepositRequestDto
    {
        public string Asset { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}