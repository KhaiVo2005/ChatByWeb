using ChatByWeb.Services.Api;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (headers != null)
            foreach (var kv in headers)
                request.Headers.Add(kv.Key, kv.Value);

        var resp = await _http.SendAsync(request);
        if (resp.IsSuccessStatusCode)
            return await resp.Content.ReadFromJsonAsync<T>();
        return default;
    }

    public async Task<T?> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(data)
        };
        if (headers != null)
            foreach (var kv in headers)
                request.Headers.Add(kv.Key, kv.Value);

        var resp = await _http.SendAsync(request);
        if (resp.IsSuccessStatusCode)
            return await resp.Content.ReadFromJsonAsync<T>();
        return default;
    }

    public async Task<T?> PutAsync<T>(string url, object data, Dictionary<string, string>? headers = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(data)
        };
        if (headers != null)
            foreach (var kv in headers)
                request.Headers.Add(kv.Key, kv.Value);

        var resp = await _http.SendAsync(request);
        if (resp.IsSuccessStatusCode)
            return await resp.Content.ReadFromJsonAsync<T>();
        return default;
    }

    public async Task<T?> DeleteAsync<T>(string url, object data = null, Dictionary<string, string>? headers = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        if (data != null)
            request.Content = JsonContent.Create(data);
        if (headers != null)
            foreach (var kv in headers)
                request.Headers.Add(kv.Key, kv.Value);

        var resp = await _http.SendAsync(request);
        if (resp.IsSuccessStatusCode)
            return await resp.Content.ReadFromJsonAsync<T>();
        return default;
    }
}
