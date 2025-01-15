using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using CryptoExchangeTrainingUI.Configuration;
using CryptoExchangeTrainingUI.Services.Authentication;
using CryptoExchangeTrainingUI.Services.Common;

namespace CryptoExchangeTrainingUI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Конфигурация
            services.Configure<ApiSettings>(
                configuration.GetSection("ApiSettings"));

            // Сервисы Blazored
            services.AddBlazoredLocalStorage();

            // Аутентификация
            services.AddAuthorizationCore();
            services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

            // Сервисы приложения
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IApiErrorHandler, ApiErrorHandler>();

            return services;
        }
    }
}