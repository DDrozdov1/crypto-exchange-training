using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace CryptoExchangeTrainingAPI.Services
{

    public interface IMarketService
    {
        Task<decimal> GetPriceAsync(string pair);
    }

    public class MarketService : IMarketService
    {
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly HttpClient _httpClient;

        public MarketService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetPriceAsync(string pair)
        {
            if (!_cache.TryGetValue(pair, out decimal price))
            {
                // Если в кэше нет данных, делаем запрос
                var response = await _httpClient.GetAsync($"https://api.binance.com/api/v3/ticker/price?symbol={pair}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Не удалось получить данные о рынке.");
                }
                var content = await response.Content.ReadAsStringAsync();
                var marketData = JsonConvert.DeserializeObject<MarketDataResponse>(content);
                price = marketData.Price;

                _cache.Set(pair, price, TimeSpan.FromSeconds(10));
            }

            return price;
        }

        private class MarketDataResponse
        {
            [JsonProperty("symbol")]
            public string Symbol { get; set; } = null!;

            [JsonProperty("price")]
            public decimal Price { get; set; }
        }
    }
}