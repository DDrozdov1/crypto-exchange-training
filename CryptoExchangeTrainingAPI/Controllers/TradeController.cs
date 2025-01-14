using Microsoft.AspNetCore.Mvc;
using CryptoExchangeTrainingAPI.Data;
using Microsoft.AspNetCore.Authorization;
using CryptoExchangeTrainingAPI.Models;
using Microsoft.EntityFrameworkCore;
using CryptoExchangeTrainingAPI.Services;

namespace CryptoExchangeTrainingAPI.Controllers
{
    /// <summary>
    /// Контроллер для управления сделками.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TradeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMarketService _marketService;
        private readonly INotificationService _notificationService;


        /// <summary>
        /// Конструктор контроллера TradeController.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        /// <param name="marketService">Сервис для получения рыночных данных.</param>
        public TradeController(ApplicationDbContext context, IMarketService marketService, INotificationService notificationService)
        {
            _context = context;
            _marketService = marketService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Получить историю сделок текущего пользователя.
        /// </summary>
        /// <returns>Список сделок.</returns>
        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetTradeHistory()
        {
            var userId = User.FindFirst("id")?.Value;

            var trades = await _context.Trades
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.OpenedAt)
                .Select(t => new TradeDto
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
                })
                .ToListAsync();

            return Ok(trades);
        }

        /// <summary>
        /// Открыть новую сделку.
        /// </summary>
        /// <param name="request">Данные для открытия сделки.</param>
        /// <returns>Информация о созданной сделке.</returns>
        [HttpPost("open")]
        [Authorize]
        public async Task<IActionResult> OpenTrade([FromBody] OpenTradeRequestDto request)
        {
            // Проверка на валидность запроса
            if (request == null || string.IsNullOrEmpty(request.Pair) || string.IsNullOrEmpty(request.Type) || request.Amount <= 0)
            {
                return BadRequest("Некорректные данные для открытия сделки.");
            }

            var userId = User.FindFirst("id")?.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Unauthorized("Пользователь не найден.");
            }

            // Проверка баланса пользователя
            if (user.Balance < request.Amount)
            {
                return BadRequest("Недостаточно средств для открытия сделки.");
            }

            // Уменьшение баланса пользователя
            user.Balance -= request.Amount;

            // Создание сделки
            var trade = new Trade
            {
                UserId = userId, // Устанавливаем ID текущего пользователя
                Pair = request.Pair.ToUpper(),
                Type = request.Type.ToLower(),
                Leverage = request.Leverage,
                Amount = request.Amount,
                EntryPrice = await _marketService.GetPriceAsync(request.Pair), // Получение текущей цены из MarketService
                StopLoss = request.StopLoss,
                TakeProfit = request.TakeProfit,
                OpenedAt = DateTime.UtcNow,
                Status = "open"
            };

            // Сохранение сделки в базе данных
            await _context.Trades.AddAsync(trade);
            await _context.SaveChangesAsync();
            await _notificationService.CreateNotificationAsync(userId, $"Сделка на {request.Pair} успешно открыта.");

            // Возвращаем TradeDto вместо сущности Trade
            var tradeDto = new TradeDto
            {
                Id = trade.Id,
                Pair = trade.Pair,
                Type = trade.Type,
                Leverage = trade.Leverage,
                Amount = trade.Amount,
                EntryPrice = trade.EntryPrice,
                StopLoss = trade.StopLoss,
                TakeProfit = trade.TakeProfit,
                Status = trade.Status,
                OpenedAt = trade.OpenedAt,
                ClosedAt = trade.ClosedAt
            };

            return Ok(tradeDto);
        }

        /// <summary>
        /// Закрыть сделку.
        /// </summary>
        /// <param name="id">ID сделки.</param>
        /// <returns>Информация о закрытой сделке.</returns>
        [HttpPost("close/{id}")]
        [Authorize]
        public async Task<IActionResult> CloseTrade(int id)
        {
            var userId = User.FindFirst("id")?.Value;
            var trade = await _context.Trades.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (trade == null)
            {
                return NotFound("Сделка не найдена.");
            }

            if (trade.Status != "open")
            {
                return BadRequest("Сделка уже закрыта.");
            }

            // Закрытие сделки
            trade.Status = "closed";
            trade.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _notificationService.CreateNotificationAsync(userId, $"Сделка на {trade.Pair} успешно закрыта.");

            return Ok(trade);
        }
    }
}