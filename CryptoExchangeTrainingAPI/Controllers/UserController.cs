using Microsoft.AspNetCore.Mvc;
using CryptoExchangeTrainingAPI.Data;
using Microsoft.AspNetCore.Authorization;
using CryptoExchangeTrainingAPI.Models;

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
    }
}