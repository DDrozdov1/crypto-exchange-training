using Blazored.LocalStorage;
using CryptoExchangeTrainingUI.Models.User;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Linq; // Добавьте это для использования методов расширения LINQ

namespace CryptoExchangeTrainingUI.Services.Trade
{
    public class TradeService : ITradeService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public TradeService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<List<TradeDto>> GetActiveTradesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<TradeDto>>("api/Trade/history");
                return response?.Where(t => t.Status == "open").ToList() ?? new List<TradeDto>();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении активных сделок", ex);
            }
        }

        public async Task<TradeDto> OpenTradeAsync(TradeRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Trade/open", request);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<TradeDto>();
                    // Обновляем токен с новым балансом
                    var token = response.Headers.GetValues("new-token").FirstOrDefault();
                    if (!string.IsNullOrEmpty(token))
                    {
                        await _localStorage.SetItemAsync("authToken", token);
                        ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(token);
                    }
                    return result;
                }
                throw new Exception(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при открытии сделки", ex);
            }
        }

        public async Task<TradeDto> CloseTradeAsync(int tradeId)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/Trade/close/{tradeId}", new { tradeId });
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<TradeDto>();

                    if (response.Headers.Contains("new-token"))
                    {
                        var token = response.Headers.GetValues("new-token").FirstOrDefault();
                        if (!string.IsNullOrEmpty(token))
                        {
                            await _localStorage.SetItemAsync("authToken", token);
                            ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(token);
                        }
                    }

                    // Принудительно обновляем список активных сделок
                    await GetActiveTradesAsync();

                    return result;
                }
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при закрытии сделки: {errorMessage}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при закрытии сделки: {ex.Message}", ex);
            }
        }

        public async Task<List<TradeDto>> GetTradeHistoryAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TradeDto>>("api/Trade/history")
                   ?? new List<TradeDto>();
        }
    }
}
