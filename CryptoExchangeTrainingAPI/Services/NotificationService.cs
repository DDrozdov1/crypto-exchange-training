using CryptoExchangeTrainingAPI.Data;
using CryptoExchangeTrainingAPI.Models;

namespace CryptoExchangeTrainingAPI.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string message);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(string userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }
        public async Task CreateNotificationsAsync(List<string> userIds, string message)
        {
            var notifications = userIds.Select(userId => new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
        }
    }
}