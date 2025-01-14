using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using CryptoExchangeTrainingAPI.Services;

namespace CryptoExchangeTrainingAPI.Controllers
{
    [ApiController]
    [Route("ws")]
    public class WebSocketController : ControllerBase
    {
        private readonly IMarketService _marketService;
        private readonly ILogger<WebSocketController> _logger;
        public WebSocketController(IMarketService marketService, ILogger<WebSocketController> logger)
        {
            _marketService = marketService;
            _logger = logger;
        }

        [HttpGet("price")]
        public async Task GetPriceWebSocket()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _logger.LogInformation("WebSocket connection opened.");
                await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            try
            {
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var pair = message.ToUpper();
                        _logger.LogInformation("Received pair message: {pair}", pair);
                        while (webSocket.State == WebSocketState.Open)
                        {
                            try
                            {
                                var price = await _marketService.GetPriceAsync(pair);
                                var response = Encoding.UTF8.GetBytes($"{{\"pair\": \"{pair}\", \"price\": \"{price}\"}}");
                                await webSocket.SendAsync(new ArraySegment<byte>(response, 0, response.Length),
                                WebSocketMessageType.Text, true, CancellationToken.None);
                                await Task.Delay(3000); // отправка цены раз в 3 секунды
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Error send data via websocket");
                                break;
                            }

                        }
                    }
                } while (!result.CloseStatus.HasValue);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in websocket connection");
            }
            finally
            {
                _logger.LogInformation("WebSocket connection closed.");
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }

        }
    }
}