using Microsoft.AspNetCore.Mvc;
using CryptoExchangeTrainingAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CryptoExchangeTrainingAPI.Controllers
{
    /// <summary>
    /// Контроллер для управления активами пользователя.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AssetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор контроллера AssetController.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        public AssetController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить список активов пользователя.
        /// </summary>
        /// <returns>Список активов текущего пользователя.</returns>
        /// <response code="200">Список активов успешно получен.</response>
        /// <response code="401">Пользователь не авторизован.</response>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserAssets()
        {
            var userId = User.FindFirst("id")?.Value;
            var assets = await _context.UserAssets
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return Ok(assets);
        }
    }
}