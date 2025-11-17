using ChatByWeb.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class KeycloakClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public KeycloakClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<KeycloakTokenResponse?> GetTokenAsync(string username, string password)
    {
        var url = $"{_config["Keycloak:AuthUrl"]}/protocol/openid-connect/token";

        var data = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _config["Keycloak:ClientId"] ,
            ["client_secret"] = _config["Keycloak:ClientSecret"],
            ["username"] = username,
            ["password"] = password
        };

        var response = await _http.PostAsync(url, new FormUrlEncodedContent(data));

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<KeycloakTokenResponse>(content);
    }

    public async Task<KeycloakTokenResponse?> RefreshTokenAsync(string refreshToken)
    {
        var url = $"{_config["Keycloak:AuthUrl"]}/protocol/openid-connect/token";

        var data = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _config["Keycloak:ClientId"],
            ["client_secret"] = _config["Keycloak:ClientSecret"],
            ["refresh_token"] = refreshToken,
        };

        var response = await _http.PostAsync(url, new FormUrlEncodedContent(data));

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<KeycloakTokenResponse>(content);
    }
}
