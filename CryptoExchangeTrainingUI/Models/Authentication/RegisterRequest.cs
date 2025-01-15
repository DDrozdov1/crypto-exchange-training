using System.ComponentModel.DataAnnotations;

namespace CryptoExchangeTrainingUI.Models.Authentication
{
    public class RegisterRequest
    {
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;

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
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
        public string Password
        {
            get => _password;
            set => _password = value ?? string.Empty;
        }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => _confirmPassword = value ?? string.Empty;
        }
    }

}
