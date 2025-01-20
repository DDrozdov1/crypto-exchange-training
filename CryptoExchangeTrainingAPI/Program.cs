using System.Reflection;
using System.Text;
using CryptoExchangeTrainingAPI.Data;
using CryptoExchangeTrainingAPI.Models;
using CryptoExchangeTrainingAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Добавление Swagger (OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});
builder.Services.AddHostedService<TradeMonitoringService>();
// Настройка подключения к базе данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)); // Оптимизация запросов

// Настройка Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Блокировка после неудачных попыток входа
    options.Lockout.MaxFailedAccessAttempts = 5; // Максимальное количество попыток
    options.User.RequireUniqueEmail = true; // Требование уникального email
});

// Настройка JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // Убираем дополнительное время жизни токена
    };
});

// Регистрация сервисов
builder.Services.AddHttpClient<IHttpRequestService, HttpRequestService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddScoped<IMarketService, MarketService>(); 
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<JwtTokenService>();

// Добавление контроллеров
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // Используем camelCase
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;

    });

// Настройка CORS
// Добавление CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("https://localhost:7160") 
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); 
        });
});


var app = builder.Build();

// Использование Swagger для тестирования API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection(); // Перенаправление HTTP на HTTPS
}

// Использование CORS
app.UseCors("AllowSpecificOrigins");

// Использование аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

// Добавление WebSocket middleware
app.UseWebSockets();

// Маршруты для контроллеров
app.MapControllers();

// Запуск приложения
app.Run();