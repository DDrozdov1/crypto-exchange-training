using Microsoft.AspNetCore.Mvc;
using CryptoExchangeTrainingAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CryptoExchangeTrainingAPI.Controllers
{
    /// <summary>
    /// Контроллер для управления аутентификацией и регистрацией пользователей.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Конструктор AuthController.
        /// </summary>
        /// <param name="userManager">Менеджер пользователей Identity.</param>
        /// <param name="signInManager">Менеджер для управления входом пользователей.</param>
        /// <param name="configuration">Конфигурация приложения.</param>
        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Регистрация нового пользователя.
        /// </summary>
        /// <param name="model">Данные для регистрации пользователя.</param>
        /// <returns>Результат регистрации.</returns>
        /// <response code="200">Пользователь успешно зарегистрирован.</response>
        /// <response code="400">Некорректные данные для регистрации.</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Неверные данные."
                });
            }

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Errors = new List<string> { "Email already registered" }
                });
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Balance = 10000.00m // Начальный баланс
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new
                {
                    Success = false,
                    Errors = errors
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Пользователь успешно зарегистрирован."
            });
        }

        /// <summary>
        /// Авторизация пользователя.
        /// </summary>
        /// <param name="model">Данные для входа пользователя.</param>
        /// <returns>JWT токен и информация о пользователе.</returns>
        /// <response code="200">Успешная авторизация.</response>
        /// <response code="400">Некорректные данные для входа.</response>
        /// <response code="401">Неверный email или пароль.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid) return BadRequest("Неверные данные.");

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return Unauthorized("Неверный email или пароль.");
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

            if (!result.Succeeded)
            {
                return Unauthorized("Неверный email или пароль.");
            }

            // Генерация JWT-токена
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("id", user.Id)
    };

            // Проверка на null для конфигурационных значений
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];
            if (string.IsNullOrEmpty(secretKey))
            {
                return StatusCode(500, "JWT Secret Key не настроен.");
            }

            var issuer = jwtSettings["Issuer"];
            if (string.IsNullOrEmpty(issuer))
            {
                return StatusCode(500, "JWT Issuer не настроен.");
            }

            var audience = jwtSettings["Audience"];
            if (string.IsNullOrEmpty(audience))
            {
                return StatusCode(500, "JWT Audience не настроен.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(jwtSettings["ExpiryMinutes"])),
                signingCredentials: creds
            );

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = user.Email,
                Balance = user.Balance
            });
        }
    }
}