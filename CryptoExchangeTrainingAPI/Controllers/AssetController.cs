using CryptoExchangeTrainingAPI.Data;
using CryptoExchangeTrainingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoExchangeTrainingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AssetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Получить список активов пользователя
        [HttpGet]
        public async Task<IActionResult> GetUserAssets()
        {
            var userId = User.Identity?.Name ?? "guest"; // Поддержка для авторизованных и гостевых пользователей

            var userAssets = await _context.UserAssets
                .Include(a => a.Token)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            if (!userAssets.Any())
            {
                return Ok(new List<object>());
            }

            return Ok(userAssets.Select(a => new
            {
                Asset = a.Token.Name,
                Balance = a.Balance
            }));
        }

        // Пополнение токена
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDto request)
        {
            if (request.Amount < 10 || request.Amount > 1000)
            {
                return BadRequest(new { Message = "Сумма пополнения должна быть от 10 до 1000." });
            }

            var userId = User.Identity?.Name ?? "guest";

            var userAsset = await _context.UserAssets
                .Include(a => a.Token)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Token.Symbol == request.Asset);
            
            if (userAsset == null)
            {
                return NotFound(new { Message = "Актив не найден." });
            }

            userAsset.Balance += request.Amount;
            userAsset.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Баланс токена {request.Asset} пополнен на {request.Amount}." });
        }
    }

}