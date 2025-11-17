using ChatByWeb.Models;

namespace ChatByWeb.Services.Auth
{
    public class KeycloakAuthService: IAuthService
    {
        private readonly KeycloakClient _client;
        private readonly IHttpContextAccessor _ctx;

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

        public async Task<KeycloakTokenResponse?> RefreshAsync(string refreshToken)
        {
            var token = await _client.RefreshTokenAsync(refreshToken);

            if (token != null)
            {
                _ctx.HttpContext!.Session.SetString("access_token", token.access_token);
                _ctx.HttpContext!.Session.SetString("refresh_token", token.refresh_token);
            }

            return token;
        }
    }
}
