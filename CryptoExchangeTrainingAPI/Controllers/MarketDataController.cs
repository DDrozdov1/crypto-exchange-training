using Microsoft.AspNetCore.Mvc;
using CryptoExchangeTrainingAPI.Services;

namespace CryptoExchangeTrainingAPI.Controllers
{
    /// <summary>
    /// Контроллер для получения рыночных данных.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MarketDataController : ControllerBase
    {
        private readonly IMarketService _marketService;

        /// <summary>
        /// Конструктор MarketDataController.
        /// </summary>
        /// <param name="marketService">Сервис для получения рыночных данных.</param>
        public MarketDataController(IMarketService marketService)
        {
            _marketService = marketService;
        }

        /// <summary>
        /// Получить текущую цену для торговой пары.
        /// </summary>
        /// <param name="pair">Торговая пара (например, BTCUSDT).</param>
        /// <returns>Цена для указанной пары.</returns>
        /// <response code="200">Цена успешно получена.</response>
        /// <response code="400">Ошибка при получении рыночных данных.</response>
        [HttpGet("{pair}")]
        public async Task<IActionResult> GetMarketPrice(string pair)
        {
            try
            {
                var price = await _marketService.GetPriceAsync(pair.ToUpper());
                return Ok(new { Pair = pair.ToUpper(), Price = price });
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при получении рыночных данных: {ex.Message}");
            }
        }
    }
}