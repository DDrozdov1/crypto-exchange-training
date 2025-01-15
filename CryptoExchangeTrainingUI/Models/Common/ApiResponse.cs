namespace CryptoExchangeTrainingUI.Models.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public List<string> Errors { get; init; } = new();
        public T? Data { get; init; }
    }
}
