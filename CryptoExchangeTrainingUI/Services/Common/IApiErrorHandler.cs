namespace CryptoExchangeTrainingUI.Services.Common
{
    public interface IApiErrorHandler
    {
        Task<List<string>> HandleApiError(HttpResponseMessage response);
    }
}
