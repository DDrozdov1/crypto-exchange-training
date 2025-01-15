using Microsoft.AspNetCore.Mvc;
using CryptoExchangeTrainingAPI.Data;
using Microsoft.AspNetCore.Authorization;
using CryptoExchangeTrainingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoExchangeTrainingAPI.Controllers
{
    /// <summary>
    /// Контроллер для управления пользователями.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор UserController.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить профиль текущего пользователя.
        /// </summary>
        /// <returns>Данные профиля пользователя.</returns>
        /// <response code="200">Профиль успешно получен.</response>
        /// <response code="401">Пользователь не авторизован.</response>        
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("id")?.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Unauthorized("Пользователь не найден.");
            }

            return Ok(new
            {
                user.Email,
                user.Balance,
                user.CreatedAt,
                user.LastLoginAt
            });
        }

        /// <summary>
        /// Пополнить баланс пользователя.
        /// </summary>
        /// <param name="request">Сумма для пополнения.</param>
        /// <returns>Обновленный баланс пользователя.</returns>
        /// <response code="200">Баланс успешно пополнен.</response>
        /// <response code="400">Некорректные данные для пополнения.</response>
        [HttpPost("deposit")]
        [Authorize]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDto request)
        {
            if (request == null || request.Amount <= 0)
            {
                return BadRequest("Сумма пополнения должна быть больше нуля.");
            }

            var userId = User.FindFirst("id")?.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Unauthorized("Пользователь не найден.");
            }

            user.Balance += request.Amount;
            await _context.SaveChangesAsync();

            return Ok(new { Balance = user.Balance });
        }
        /// <summary>
        /// Получить историю сделок пользователя.
        /// </summary>
        /// <returns>Список сделок текущего пользователя.</returns>
        /// <response code="200">История сделок успешно получена.</response>
        /// <response code="401">Пользователь не авторизован.</response>
        [HttpGet("transactions")]
        [Authorize]
        public async Task<IActionResult> GetTransactionHistory()
        {
            var userId = User.FindFirst("id")?.Value;

            // Проверяем, что пользователь авторизован
            if (userId == null)
            {
                return Unauthorized(new { Success = false, Message = "Пользователь не авторизован." });
            }

            // Получаем сделки пользователя из базы данных
            var trades = await _context.Trades
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.OpenedAt)
                .ToListAsync();

            // Преобразуем сделку в DTO, если нужно
            var tradeDtos = trades.Select(t => new TradeDto
            {
                Id = t.Id,
                Pair = t.Pair,
                Type = t.Type,
                Leverage = t.Leverage,
                Amount = t.Amount,
                EntryPrice = t.EntryPrice,
                StopLoss = t.StopLoss,
                TakeProfit = t.TakeProfit,
                Status = t.Status,
                OpenedAt = t.OpenedAt,
                ClosedAt = t.ClosedAt
            }).ToList();

            return Ok(new
            {
                Success = true,
                Data = tradeDtos
            });
        }

    }
}