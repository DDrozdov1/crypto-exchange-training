using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using CryptoExchangeTrainingAPI.Controllers;
using CryptoExchangeTrainingAPI.Data;
using CryptoExchangeTrainingAPI.Models;
using CryptoExchangeTrainingAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace CryptoExchangeTrainingUnitTests.Controllers
{
    public class TradeControllerTests
    {
        [Fact]
        public async Task GetTradeHistory_ReturnsTradeHistory()
        {
            // Arrange
            var userId = "testUserId";
            var trades = new List<Trade>
            {
                new Trade { Id = 1, UserId = userId, Pair = "BTCUSD", Status = "open" },
                new Trade { Id = 2, UserId = userId, Pair = "ETHUSD", Status = "closed" },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Trade>>();
            mockSet.As<IQueryable<Trade>>().Setup(m => m.Provider).Returns(trades.Provider);
            mockSet.As<IQueryable<Trade>>().Setup(m => m.Expression).Returns(trades.Expression);
            mockSet.As<IQueryable<Trade>>().Setup(m => m.ElementType).Returns(trades.ElementType);
            mockSet.As<IQueryable<Trade>>().Setup(m => m.GetEnumerator()).Returns(trades.GetEnumerator);

            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(x => x.Trades).Returns(mockSet.Object);

            var marketServiceMock = new Mock<IMarketService>();
            var notificationServiceMock = new Mock<INotificationService>();

            var controller = new TradeController(mockContext.Object, marketServiceMock.Object, notificationServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim("id", userId)
                        }))
                    }
                }
            };

            // Act
            var result = await controller.GetTradeHistory() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<TradeDto>>(result.Value);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task OpenTrade_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var userId = "testUserId";
            var user = new User { Id = userId, Balance = 1000M };

            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(x => x.Users.FindAsync(userId)).ReturnsAsync(user);

            var mockSet = new Mock<DbSet<Trade>>();
            mockContext.Setup(x => x.Trades).Returns(mockSet.Object);

            var marketServiceMock = new Mock<IMarketService>();
            marketServiceMock.Setup(x => x.GetPriceAsync(It.IsAny<string>())).ReturnsAsync(10000M);

            var notificationServiceMock = new Mock<INotificationService>();

            var controller = new TradeController(mockContext.Object, marketServiceMock.Object, notificationServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim("id", userId)
                        }))
                    }
                }
            };

            var request = new OpenTradeRequestDto
            {
                Pair = "BTCUSD",
                Type = "buy",
                Leverage = 1,
                Amount = 100M,
                StopLoss = 9000M,
                TakeProfit = 11000M
            };

            // Act
            var result = await controller.OpenTrade(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tradeDto = Assert.IsAssignableFrom<TradeDto>(okResult.Value);
            Assert.Equal("BTCUSD", tradeDto.Pair);
            Assert.Equal("buy", tradeDto.Type);
            Assert.Equal(1, tradeDto.Leverage);
            Assert.Equal(100M, tradeDto.Amount);
            Assert.Equal(10000M, tradeDto.EntryPrice);
        }

        // Добавьте больше тестов, следуя этому паттерну для других методов контроллера

        [Fact]
        public async Task CloseTrade_TradeNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = "testUserId";
            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(x => x.Trades.FirstOrDefaultAsync(It.IsAny<Func<Trade, bool>>())).ReturnsAsync((Trade)null);

            var marketServiceMock = new Mock<IMarketService>();
            var notificationServiceMock = new Mock<INotificationService>();

            var controller = new TradeController(mockContext.Object, marketServiceMock.Object, notificationServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim("id", userId)
                        }))
                    }
                }
            };

            // Act
            var result = await controller.CloseTrade(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Сделка не найдена.", notFoundResult.Value);
        }
    }

}