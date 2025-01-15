namespace CryptoExchangeTrainingUI.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            var hasNumber = password.Any(char.IsDigit);
            var hasUpperChar = password.Any(char.IsUpper);
            var hasLowerChar = password.Any(char.IsLower);
            var hasMinimumChars = password.Length >= 6;

            return hasNumber && hasUpperChar && hasLowerChar && hasMinimumChars;
        }
    }
}
