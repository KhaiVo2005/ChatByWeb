using ChatByWeb.Models;

namespace ChatByWeb.Services.Auth
{
    public interface IAuthService
    {
        Task<KeycloakTokenResponse?> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string email, string firstName, string lastName, string password);
        Task<string> GetAdminTokenAsync();
        Task<string?> GetUsernameFromGuidAsync(string guid);
    }
}
