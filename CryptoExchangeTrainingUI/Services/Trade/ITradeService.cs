using CryptoExchangeTrainingUI.Models.User;

namespace CryptoExchangeTrainingUI.Services.Trade
{
    public interface ITradeService
    {
        Task<List<TradeDto>> GetActiveTradesAsync();
        Task<TradeDto> OpenTradeAsync(TradeRequest request);
        Task<TradeDto> CloseTradeAsync(int tradeId);
        Task<List<TradeDto>> GetTradeHistoryAsync();
    }
}
