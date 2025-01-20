using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace CryptoExchangeTrainingAPI.Models
{
    public class User : IdentityUser
    {
        public decimal Balance { get; set; } = 10000.00m; // Начальный баланс пользователя
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Время регистрации
        public DateTime? LastLoginAt { get; set; } // Время последнего входа
        public ICollection<Trade> Trades { get; set; } = new List<Trade>();
        public ICollection<UserAsset> UserAssets { get; set; } = new List<UserAsset>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
