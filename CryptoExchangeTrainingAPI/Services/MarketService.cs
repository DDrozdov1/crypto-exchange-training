using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Linq;
using CryptoExchangeTrainingAPI.Models;
using CryptoExchangeTrainingAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CryptoExchangeTrainingAPI.Services
{
    public interface IMarketService
    {
        Task<decimal> GetPriceAsync(string pair);
        Task<List<HistoricalData>> GetHistoricalDataAsync(string pair, DateTime startTime, DateTime endTime);
        Task<OrderBookResponse> GetOrderBookAsync(string pair);
    }

    public class MarketService : IMarketService
    {
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        public MarketService(HttpClient httpClient, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
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

        public async Task<OrderBookResponse> GetOrderBookAsync(string pair)
        {
            var response = await _httpClient.GetAsync($"https://api.binance.com/api/v3/depth?symbol={pair}&limit=10");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Не удалось получить данные из OrderBook.");
            }
            var content = await response.Content.ReadAsStringAsync();
            var orderBook = JsonConvert.DeserializeObject<OrderBookResponse>(content);
            return orderBook;
        }
        public async Task<List<HistoricalData>> GetHistoricalDataAsync(string pair, DateTime startTime, DateTime endTime)
        {
            var data = new List<HistoricalData>();
            var interval = "1h";
            long startTimeUnix = ((DateTimeOffset)startTime).ToUnixTimeMilliseconds();
            long endTimeUnix = ((DateTimeOffset)endTime).ToUnixTimeMilliseconds();

            var response = await _httpClient.GetAsync($"https://api.binance.com/api/v3/klines?symbol={pair}&interval={interval}&startTime={startTimeUnix}&endTime={endTimeUnix}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Не удалось получить исторические данные с рынка.");
            }
            var content = await response.Content.ReadAsStringAsync();

            var klines = JsonConvert.DeserializeObject<List<List<object>>>(content);
            if (klines != null)
            {
                data = klines.Select(k => new HistoricalData
                {
                    Pair = pair,
                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)k[0]).DateTime,
                    Price = decimal.Parse((string)k[4], CultureInfo.InvariantCulture),
                }).ToList();
            }
            return data;
        }

        private class MarketDataResponse
        {
            [JsonProperty("symbol")]
            public string Symbol { get; set; } = null!;

            [JsonProperty("price")]
            public decimal Price { get; set; }
        }

    }

    public class OrderBookResponse
    {
        [JsonProperty("lastUpdateId")]
        public long LastUpdateId { get; set; }

        [JsonProperty("bids")]
        public List<List<string>> Bids { get; set; } = null!;

        [JsonProperty("asks")]
        public List<List<string>> Asks { get; set; } = null!;
    }
}