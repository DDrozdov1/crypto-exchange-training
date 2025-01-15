using Microsoft.AspNetCore.SignalR;

namespace CryptoExchangeTrainingAPI.Services
{
    public class PriceHub : Hub
    {
        public async Task UpdatePrices(Dictionary<string, decimal> prices)
        {
            await Clients.All.SendAsync("ReceivePriceUpdates", prices);
        }
    }
}
