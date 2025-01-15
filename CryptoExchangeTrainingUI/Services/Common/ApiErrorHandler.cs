using CryptoExchangeTrainingUI.Models.Common;
using System.Text.Json;

namespace CryptoExchangeTrainingUI.Services.Common
{
    public class ApiErrorHandler : IApiErrorHandler
    {
        private readonly ILogger<ApiErrorHandler> _logger;

        public ApiErrorHandler(ILogger<ApiErrorHandler> logger)
        {
            _logger = logger;
        }

        public async Task<List<string>> HandleApiError(HttpResponseMessage response)
        {
            var errors = new List<string>();

            try
            {
                var content = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content);

                if (errorResponse?.Errors != null && errorResponse.Errors.Any())
                {
                    errors.AddRange(errorResponse.Errors);
                }
                else if (!string.IsNullOrEmpty(errorResponse?.Message))
                {
                    errors.Add(errorResponse.Message);
                }
                else
                {
                    errors.Add($"HTTP Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing API error response");
                errors.Add("An unexpected error occurred");
            }

            return errors;
        }
    }
}
