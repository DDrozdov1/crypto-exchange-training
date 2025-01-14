using System;

namespace CryptoExchangeTrainingAPI.Models
{
    public class Notification
    {
        public int Id { get; set; } // Уникальный идентификатор уведомления
        public string UserId { get; set; } = null!;// Внешний ключ на пользователя
        public string Message { get; set; } = null!; // Текст уведомления
        public bool IsRead { get; set; } = false; // Прочитано или нет
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Время создания уведомления

        // Связь с пользователем
        public User? User { get; set; }
    }
}