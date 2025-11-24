using ChatByWeb.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace ChatByWeb.Services.Auth
{
    public class KeycloakAuthService : IAuthService
    {
        private readonly KeycloakClient _client;
        private readonly IHttpContextAccessor _ctx;

        private readonly string adminUsername = "khai";
        private readonly string adminPassword = "123";
        private readonly string keycloakUrl = "http://localhost:8082";
        private readonly string realm = "chatapp";

        public string KeycloakUrl => keycloakUrl;  // public property
        public string Realm => realm;              // public property

        public KeycloakAuthService(KeycloakClient client, IHttpContextAccessor ctx)
        {
            _client = client;
            _ctx = ctx;
        }

        public async Task<KeycloakTokenResponse?> LoginAsync(string username, string password)
        {
            var token = await _client.GetTokenAsync(username, password);
            if (token != null)
            {
                _ctx.HttpContext!.Session.SetString("access_token", token.access_token);
                _ctx.HttpContext!.Session.SetString("refresh_token", token.refresh_token);
            }
            return token;
        }

        public async Task<bool> RegisterAsync(string username, string email, string firstName, string lastName, string password)
        {
            try
            {
                var adminToken = await GetAdminTokenAsync();
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

                var userObj = new
                {
                    username = username,
                    email = email,
                    firstName = firstName,
                    lastName = lastName,
                    enabled = true,
                    emailVerified = true,
                    credentials = new[]
                    {
                new {
                    type = "password",
                    value = password,
                    temporary = false
                }
            }
                };

                var content = new StringContent(JsonConvert.SerializeObject(userObj), Encoding.UTF8, "application/json");

                var url = $"{keycloakUrl}/admin/realms/{realm}/users";
                var response = await client.PostAsync(url, content);

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"StatusCode: {response.StatusCode}");
                Console.WriteLine($"ResponseBody: {responseBody}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetAdminTokenAsync()
        {
            var client = new HttpClient();
            var dict = new Dictionary<string, string>
            {
                { "client_id", "admin-cli" },
                { "username", adminUsername },
                { "password", adminPassword },
                { "grant_type", "password" }
            };

            var tokenUrl = $"{keycloakUrl}/realms/master/protocol/openid-connect/token";
            var resp = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(dict));
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(json)!;
            return data.access_token;
        }

        public async Task<string?> GetUsernameFromGuidAsync(string guid)
        {
            try
            {
                var adminToken = await GetAdminTokenAsync();
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

                var url = $"{keycloakUrl}/admin/realms/{realm}/users/{guid}";
                var resp = await client.GetAsync(url);
                if (!resp.IsSuccessStatusCode) return null;

                var json = await resp.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(json)!;
                return data.username;
            }
            catch
            {
                return null;
            }
        }
    }
}
