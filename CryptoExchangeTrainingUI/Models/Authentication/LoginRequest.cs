using System.ComponentModel.DataAnnotations;

namespace CryptoExchangeTrainingUI.Models.Authentication
{
    public class LoginRequest
    {
        private string _email = string.Empty;
        private string _password = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email
        {
            get => _email;
            set => _email = value?.Trim() ?? string.Empty;
        }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string Password
        {
            get => _password;
            set => _password = value ?? string.Empty;
        }
    }
}
