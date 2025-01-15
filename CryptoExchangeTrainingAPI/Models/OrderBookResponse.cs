using Newtonsoft.Json;
using System.Collections.Generic;

namespace CryptoExchangeTrainingAPI.Models
{
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