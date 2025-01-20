using CryptoExchangeTrainingAPI.Data;
using CryptoExchangeTrainingAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class TradeMonitoringService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TradeMonitoringService> _logger;

    public TradeMonitoringService(IServiceScopeFactory scopeFactory, ILogger<TradeMonitoringService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var marketService = scope.ServiceProvider.GetRequiredService<IMarketService>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    var openTrades = await dbContext.Trades
                        .Where(t => t.Status == "open")
                        .ToListAsync(stoppingToken);

                    foreach (var trade in openTrades)
                    {
                        var currentPrice = await marketService.GetPriceAsync(trade.Pair);

                        bool shouldClose = false;
                        string closeReason = "";

                        if (trade.Type == "buy" && currentPrice <= trade.StopLoss)
                        {
                            shouldClose = true;
                            closeReason = "Stop Loss triggered";
                        }
                        else if (trade.Type == "sell" && currentPrice >= trade.StopLoss)
                        {
                            shouldClose = true;
                            closeReason = "Stop Loss triggered";
                        }

                        if (trade.Type == "buy" && currentPrice >= trade.TakeProfit)
                        {
                            shouldClose = true;
                            closeReason = "Take Profit reached";
                        }
                        else if (trade.Type == "sell" && currentPrice <= trade.TakeProfit)
                        {
                            shouldClose = true;
                            closeReason = "Take Profit reached";
                        }

                        if (shouldClose)
                        {
                            trade.Status = "closed";
                            trade.ClosedAt = DateTime.UtcNow;
                            trade.ExitPrice = currentPrice;

                            var profitLoss = (currentPrice - trade.EntryPrice) * trade.Amount * trade.Leverage;

                            if (trade.Type == "sell")
                            {
                                profitLoss = (trade.EntryPrice - currentPrice) * trade.Amount * trade.Leverage;
                            }

                            trade.ProfitLoss = profitLoss;

                            var user = await dbContext.Users.FindAsync(new object[] { trade.UserId }, stoppingToken);
                            if (user != null)
                            {
                                user.Balance += profitLoss;
                            }

                            await dbContext.SaveChangesAsync(stoppingToken);

                            await notificationService.CreateNotificationAsync(
                                trade.UserId,
                                $"Сделка {trade.Pair} автоматически закрыта: {closeReason}. " +
                                $"Прибыль/убыток: {profitLoss}"
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in trade monitoring service");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}