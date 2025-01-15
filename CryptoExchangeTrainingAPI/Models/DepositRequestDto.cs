using System.ComponentModel.DataAnnotations;

namespace CryptoExchangeTrainingAPI.Models
{
    public class DepositRequestDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумма должна быть больше 0.")]
        public decimal Amount { get; set; }
        public string Asset { get; set; } = string.Empty;

    }
}