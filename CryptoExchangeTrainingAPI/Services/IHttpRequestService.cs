using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CryptoExchangeTrainingAPI.Services
{
    public interface IHttpRequestService
    {
        Task<T> GetAsync<T>(string url) where T : class;
    }

    public class HttpRequestService : IHttpRequestService
    {
        private readonly HttpClient _httpClient;

        public HttpRequestService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T> GetAsync<T>(string url) where T : class
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP запрос не успешен. Код: {response.StatusCode}, URL: {url}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content) ?? throw new Exception("Ошибка десериализации.");
        }
    }
}