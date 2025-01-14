using Microsoft.AspNetCore.Mvc;
using CryptoExchangeTrainingAPI.Data;
using Microsoft.AspNetCore.Authorization;
using CryptoExchangeTrainingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoExchangeTrainingAPI.Controllers
{
    /// <summary>
    /// Контроллер для управления уведомлениями.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        /// <summary>
        /// Конструктор контроллера NotificationController.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить список уведомлений текущего пользователя.
        /// </summary>
        /// <returns>Список уведомлений пользователя.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirst("id")?.Value;
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        /// <summary>
        /// Пометить уведомление как прочитанное.
        /// </summary>
        /// <param name="id">ID уведомления.</param>
        /// <returns>Результат операции.</returns>
        [HttpPost("read/{id}")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirst("id")?.Value;
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
            {
                return NotFound("Уведомление не найдено.");
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok("Уведомление помечено как прочитанное.");
        }

        /// <summary>
        /// Удалить уведомление.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = User.FindFirst("id")?.Value;
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
            {
                return NotFound("Уведомление не найдено.");
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok("Уведомление удалено.");
        }
    }
}