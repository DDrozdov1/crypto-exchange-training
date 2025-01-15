using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using CryptoExchangeTrainingUI.Services.Authentication;
using CryptoExchangeTrainingUI.Services.Common;
using CryptoExchangeTrainingUI;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http.Headers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Information);
});
builder.Services.AddScoped(sp =>
{
    var client = new HttpClient
    {
        BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7243")
    };
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    return client;
});

// Сервисы
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IApiErrorHandler, ApiErrorHandler>();

// Логирование
builder.Logging.SetMinimumLevel(LogLevel.Information);

await builder.Build().RunAsync();