using Microsoft.AspNetCore.Mvc;
using CryptoExchangeTrainingAPI.Data;
using Microsoft.AspNetCore.Authorization;
using CryptoExchangeTrainingAPI.Models;
using Microsoft.EntityFrameworkCore;
using CryptoExchangeTrainingAPI.Services;
using System.Globalization;

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
        /// <param name="notificationService">Сервис для отправки уведомлений.</param>
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

            var fee = request.Amount * 0.001m; // Пример комиссии

            if (user.Balance < request.Amount + fee)
            {
                return BadRequest("Недостаточно средств для открытия сделки с учетом комиссии.");
            }

            user.Balance -= request.Amount + fee;

            var orderBook = await _marketService.GetOrderBookAsync(request.Pair);
            if (orderBook == null || orderBook.Asks == null || orderBook.Bids == null || (!orderBook.Asks.Any() && !orderBook.Bids.Any()))
            {
                return BadRequest("Не удалось получить данные о рыночных ордерах.");
            }

            decimal price;
            if (request.Type.ToLower() == "buy")
            {
                price = decimal.Parse(orderBook.Asks.First()[0], CultureInfo.InvariantCulture);
            }
            else
            {
                price = decimal.Parse(orderBook.Bids.First()[0], CultureInfo.InvariantCulture);
            }

            // Создание сделки
            var trade = new Trade
            {
                UserId = userId,
                Pair = request.Pair.ToUpper(),
                Type = request.Type.ToLower(),
                Leverage = request.Leverage,
                Amount = request.Amount,
                EntryPrice = price, // использовать цену из книги ордеров
                StopLoss = request.StopLoss,
                TakeProfit = request.TakeProfit,
                OpenedAt = DateTime.UtcNow,
                Status = "open",
                Fee = fee,
            };

            _context.Trades.Add(trade);
            await _context.SaveChangesAsync();
            await _notificationService.CreateNotificationAsync(userId, $"Сделка на {request.Pair} успешно открыта.");

            // Возвращаем TradeDto
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

            // Получение текущей цены
            var exitPrice = await _marketService.GetPriceAsync(trade.Pair);
            trade.ExitPrice = exitPrice;


            // Расчет прибыли/убытка
            var profitLoss = (exitPrice - trade.EntryPrice) * trade.Amount * trade.Leverage;
            trade.ProfitLoss = profitLoss;

            // Обновление баланса пользователя
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Balance += profitLoss;

                // Логика работы с активами
                if (trade.Type.ToLower() == "buy")
                {
                    var userAsset = await _context.UserAssets.FirstOrDefaultAsync(ua => ua.UserId == userId && ua.Asset == trade.Pair);
                    if (userAsset == null)
                    {
                        await _context.UserAssets.AddAsync(new UserAsset { UserId = userId, Asset = trade.Pair, Balance = trade.Amount });
                    }
                    else
                    {
                        userAsset.Balance += trade.Amount;
                    }
                    await _context.SaveChangesAsync();
                }
                else if (trade.Type.ToLower() == "sell")
                {
                    var userAsset = await _context.UserAssets.FirstOrDefaultAsync(ua => ua.UserId == userId && ua.Asset == trade.Pair);
                    if (userAsset != null)
                    {
                        userAsset.Balance -= trade.Amount;
                    }

                    await _context.SaveChangesAsync();
                }
            }

            // Закрытие сделки
            trade.Status = "closed";
            trade.ClosedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _notificationService.CreateNotificationAsync(userId, $"Сделка на {trade.Pair} успешно закрыта с прибылью {profitLoss}.");


            return Ok(trade);
        }

    }
}