using Microsoft.AspNetCore.Mvc;
using CryptoExchangeTrainingAPI.Services;
using System;
using System.Threading.Tasks;

namespace CryptoExchangeTrainingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChartController : ControllerBase
    {
        private readonly IMarketService _marketService;

        public ChartController(IMarketService marketService)
        {
            _marketService = marketService;
        }

        [HttpGet("historical/{pair}")]
        public async Task<IActionResult> GetHistoricalData(string pair, DateTime? startTime, DateTime? endTime)
        {
            if (startTime == null)
            {
                startTime = DateTime.UtcNow.AddDays(-7);
            }
            if (endTime == null)
            {
                endTime = DateTime.UtcNow;
            }

            try
            {
                var historicalData = await _marketService.GetHistoricalDataAsync(pair.ToUpper(), startTime.Value, endTime.Value);
                return Ok(historicalData);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при получении исторических данных: {ex.Message}");
            }
        }
    }
}