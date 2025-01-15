namespace CryptoExchangeTrainingUI.Helpers
{
    public static class Constants
    {
        public static class Storage
        {
            public const string AuthToken = "authToken";
            public const string RefreshToken = "refreshToken";
            public const string UserData = "userData";
        }

        public static class Api
        {
            public const string Login = "api/auth/login";
            public const string Register = "api/auth/register";
            public const string RefreshToken = "api/auth/refresh";
        }

        public static class Routes
        {
            public const string Login = "/login";
            public const string Register = "/register";
            public const string Home = "/";
            public const string Profile = "/profile";
        }
    }
}
