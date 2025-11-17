namespace ChatByWeb.Services.Api
{
    public interface IApiClient
    {
        Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null);
        Task<T?> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null);
        Task<T?> PutAsync<T>(string url, object data, Dictionary<string, string>? headers = null);
        Task<T?> DeleteAsync<T>(string url, object data = null, Dictionary<string, string>? headers = null);
    }
}
