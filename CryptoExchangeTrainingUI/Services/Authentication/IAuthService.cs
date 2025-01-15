using CryptoExchangeTrainingUI.Models.Authentication;

namespace CryptoExchangeTrainingUI.Services.Authentication
{
    public interface IAuthService
    {
        Task<AuthResponse> Login(LoginRequest loginRequest);
        Task<AuthResponse> Register(RegisterRequest registerRequest);
        Task<bool> IsAuthenticated();
        Task Logout();
        Task<string?> GetToken();
        Task<string> GetUserDisplayName();
    }


}
