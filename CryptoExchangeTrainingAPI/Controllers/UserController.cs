using Microsoft.AspNetCore.Mvc;
using CryptoExchangeTrainingAPI.Data;
using Microsoft.AspNetCore.Authorization;
using CryptoExchangeTrainingAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CryptoExchangeTrainingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("id")?.Value;
            if (userId == null)
            {
                return Unauthorized("Пользователь не найден.");
            }
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Unauthorized("Пользователь не найден.");
            }

            var assets = await _context.UserAssets
              .Where(a => a.UserId == userId)
              .Select(a => new
              {
                  a.Asset,
                  a.Balance
              })
              .ToListAsync();


            var assetsWithPrice = new List<object>();

            foreach (var asset in assets)
            {
                // Временно установим цену в 100, тут будет логика получения реальной цены
                decimal currentPrice = 100.00m;

                assetsWithPrice.Add(new
                {
                    Symbol = asset.Asset,
                    Quantity = asset.Balance,
                    CurrentPrice = currentPrice
                });
            }

            return Ok(new
            {
                user.Email,
                user.Balance,
                user.CreatedAt,
                user.LastLoginAt,
                Assets = assetsWithPrice
            });
        }

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

            // Логирование перед изменением
            Console.WriteLine($"Баланс до пополнения: {user.Balance}");

            user.Balance += request.Amount;
            await _context.SaveChangesAsync();

            // Логирование после сохранения изменений
            Console.WriteLine($"Баланс после пополнения: {user.Balance}");

            return Ok(new { Balance = user.Balance });
        }

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