using ChatByWeb.Models;

namespace ChatByWeb.Services.Auth
{
    public interface IAuthService
    {
        Task<KeycloakTokenResponse?> LoginAsync(string username, string password);
        Task<KeycloakTokenResponse?> RefreshAsync(string refreshToken);
    }
}
