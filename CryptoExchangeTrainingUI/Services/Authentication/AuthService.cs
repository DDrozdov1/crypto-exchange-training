using Blazored.LocalStorage;
using CryptoExchangeTrainingUI.Helpers;
using CryptoExchangeTrainingUI.Models.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using CryptoExchangeTrainingUI.Services.Common;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace CryptoExchangeTrainingUI.Services.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILogger<AuthService> _logger;
        private readonly IApiErrorHandler _errorHandler;

        public AuthService(
            HttpClient httpClient,
            ILocalStorageService localStorage,
            AuthenticationStateProvider authStateProvider,
            ILogger<AuthService> logger,
            IApiErrorHandler errorHandler)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            _authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        }

        public async Task<string?> GetToken()
        {
            try
            {
                return await _localStorage.GetItemAsync<string>(Constants.Storage.AuthToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving token");
                return null;
            }
        }

        public async Task<bool> IsAuthenticated()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>(Constants.Storage.AuthToken);
                if (string.IsNullOrEmpty(token))
                    return false;

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken == null)
                    return false;

                return jsonToken.ValidTo > DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authentication status");
                return false;
            }
        }

        public async Task<AuthResponse> Login(LoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation("Attempting login for user: {Email}", loginRequest.Email);

                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

                // Читаем содержимое ответа
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Raw response: {Response}", responseContent);

                // Проверяем, что ответ не пустой и является валидным JSON
                if (!string.IsNullOrEmpty(responseContent))
                {
                    try
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var result = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
                            {
                                await _localStorage.SetItemAsync(Constants.Storage.AuthToken, result.Token);
                                _httpClient.DefaultRequestHeaders.Authorization =
                                    new AuthenticationHeaderValue("Bearer", result.Token);
                                ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
                                return result;
                            }
                        }

                        // Пытаемся прочитать сообщение об ошибке
                        try
                        {
                            var errorResult = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                            var errorMessage = errorResult?.GetValueOrDefault("message")?.ToString();

                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
                                {
                                    return new AuthResponse
                                    {
                                        Success = false,
                                        Message = "User not found",
                                        Errors = new List<string> { "No account exists with this email address" }
                                    };
                                }
                                if (errorMessage.Contains("password", StringComparison.OrdinalIgnoreCase))
                                {
                                    return new AuthResponse
                                    {
                                        Success = false,
                                        Message = "Invalid password",
                                        Errors = new List<string> { "The password you entered is incorrect" }
                                    };
                                }

                                return new AuthResponse
                                {
                                    Success = false,
                                    Message = errorMessage,
                                    Errors = new List<string> { errorMessage }
                                };
                            }
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Error parsing error response");
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Error deserializing response: {Response}", responseContent);
                    }
                }

                // Если не удалось обработать ответ, возвращаем общую ошибку
                return new AuthResponse
                {
                    Success = false,
                    Message = "Login failed",
                    Errors = new List<string>
            {
                $"Unable to process server response. Status: {response.StatusCode}",
                "Please try again later"
            }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt");
                return new AuthResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred",
                    Errors = new List<string> { "Please check your connection and try again" }
                };
            }
        }

        public async Task Logout()
        {
            try
            {
                await _localStorage.RemoveItemAsync(Constants.Storage.AuthToken);
                await _localStorage.RemoveItemAsync(Constants.Storage.RefreshToken);
                _httpClient.DefaultRequestHeaders.Authorization = null;
                ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserLogout();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                throw;
            }
        }
        public async Task<AuthResponse> Register(RegisterRequest registerRequest)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<AuthResponse>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        return result;
                    }
                }

                var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                return new AuthResponse
                {
                    Success = false,
                    Message = errorResponse?["message"]?.ToString() ?? "Registration failed",
                    Errors = new List<string> { errorResponse?["message"]?.ToString() ?? "Unknown error" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                return new AuthResponse
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<string> GetUserDisplayName()
        {
            var token = await GetToken();
            if (string.IsNullOrEmpty(token))
                return string.Empty;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                var email = jsonToken?.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty;
                return email.Split('@')[0];
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<AuthResponse> RefreshToken()
        {
            try
            {
                var refreshToken = await _localStorage.GetItemAsync<string>(Constants.Storage.RefreshToken);
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Refresh token not found in storage");
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "No refresh token available"
                    };
                }

                var response = await _httpClient.PostAsJsonAsync(Constants.Api.RefreshToken,
                    new { RefreshToken = refreshToken });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
                    {
                        await _localStorage.SetItemAsync(Constants.Storage.AuthToken, result.Token);
                        await _localStorage.SetItemAsync(Constants.Storage.RefreshToken, result.RefreshToken);

                        _httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.Token);

                        return result;
                    }
                }

                var errors = await _errorHandler.HandleApiError(response);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Token refresh failed",
                    Errors = errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return new AuthResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred during token refresh",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}