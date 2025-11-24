using ChatByWeb.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ChatByWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly KeycloakAuthService _authService;

        public UsersController(KeycloakAuthService authService)
        {
            _authService = authService;
        }

        // POST api/users/map-guids
        [HttpPost("map-guids")]
        public async Task<IActionResult> MapGuidsToUsernames([FromBody] List<string> guids)
        {
            if (guids == null || guids.Count == 0)
                return BadRequest("No GUIDs provided");

            var adminToken = await _authService.GetAdminTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var result = new Dictionary<string, string>();

            foreach (var guid in guids)
            {
                try
                {
                    var res = await client.GetAsync($"{_authService.KeycloakUrl}/admin/realms/{_authService.Realm}/users/{guid}");
                    if (res.IsSuccessStatusCode)
                    {
                        var data = JsonConvert.DeserializeObject<dynamic>(await res.Content.ReadAsStringAsync());
                        result[guid] = data.username;
                    }
                    else
                    {
                        result[guid] = guid; // fallback nếu không tìm thấy
                    }
                }
                catch
                {
                    result[guid] = guid; // fallback nếu request lỗi
                }
            }

            return Ok(result);
        }
    }
}
