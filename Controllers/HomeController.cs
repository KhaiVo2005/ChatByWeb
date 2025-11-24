using ChatByWeb.Models;
using ChatByWeb.Services.Api;
using ChatByWeb.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChatByWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly string _conversationBaseUrl;
        private readonly string _messageBaseUrl;
        private readonly IAuthService _auth;

        public HomeController(IApiClient apiClient, IConfiguration configuration, IAuthService auth)
        {
            _apiClient = apiClient;
            _conversationBaseUrl = configuration["Api:ConversationBaseUrl"];
            _messageBaseUrl = configuration["Api:MessageBaseUrl"];
            _auth = auth;
        }

        public async Task<IActionResult> Index()
        {
            var guid = HttpContext.Session.GetString("guid");
            var token = HttpContext.Session.GetString("access_token");
            if (string.IsNullOrWhiteSpace(guid) || string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Index", "Login");

            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = "Bearer " + token
            };

            List<ConversationModel> conversations = new List<ConversationModel>();
            try
            {
                conversations = await _apiClient.GetAsync<List<ConversationModel>>(
                    $"{_conversationBaseUrl}?userId={guid}", headers
                ) ?? new List<ConversationModel>();
            }
            catch
            {
                // optional: log lỗi
            }

            ViewBag.ConversationBaseUrl = _conversationBaseUrl;
            ViewBag.MessageBaseUrl = _messageBaseUrl;

            return View(conversations);
        }

        [HttpGet("Home/ChatPartial/{id}")]
        public IActionResult ChatPartial(string id)
        {
            ViewBag.ConversationId = id;
            return PartialView("ChatPartial");
        }

        [HttpPost]
        public async Task<IActionResult> CreateConversation(string otherUsers, string? groupTitle)
        {
            var token = HttpContext.Session.GetString("access_token");
            if (string.IsNullOrWhiteSpace(otherUsers) || string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Index");

            string adminToken = await _auth.GetAdminTokenAsync();
            var memberGuids = await GetGuidsFromUsernames(otherUsers, adminToken);

            if (!memberGuids.Any())
                return RedirectToAction("Index");

            bool isDirect = memberGuids.Count == 1;
            var usernames = otherUsers.Split(',').Select(u => u.Trim()).ToList();
            string title = isDirect
                ? $"Chat với {usernames[0]}"
                : (string.IsNullOrWhiteSpace(groupTitle) ? $"Nhóm chat ({string.Join(", ", usernames)})" : groupTitle);

            var conversationData = new
            {
                isDirect,
                members = memberGuids,
                title,
                idempotencyKey = Guid.NewGuid().ToString()
            };

            try
            {
                var headers = new Dictionary<string, string>
                {
                    ["Authorization"] = "Bearer " + token
                };

                await _apiClient.PostAsync<object>(_conversationBaseUrl, conversationData, headers);
            }
            catch
            {
                // optional: log lỗi
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string conversationId, string content)
        {
            var guid = HttpContext.Session.GetString("guid"); // sử dụng GUID của user
            var token = HttpContext.Session.GetString("access_token");
            if (string.IsNullOrWhiteSpace(guid) || string.IsNullOrWhiteSpace(conversationId) || string.IsNullOrWhiteSpace(token))
                return BadRequest();

            var messageData = new
            {
                conversationId,
                senderId = guid,
                content
            };

            try
            {
                var headers = new Dictionary<string, string>
                {
                    ["Authorization"] = "Bearer " + token
                };

                await _apiClient.PostAsync<object>($"{_messageBaseUrl}/send", messageData, headers);
            }
            catch
            {
                // optional: log lỗi
            }

            return RedirectToAction("ChatPartial", new { id = conversationId });
        }

        // Helper method để lấy GUID từ username
        private async Task<List<string>> GetGuidsFromUsernames(string otherUsers, string adminToken)
        {
            var usernames = otherUsers.Split(',')
                                      .Select(u => u.Trim())
                                      .Where(u => !string.IsNullOrWhiteSpace(u))
                                      .ToList();

            var guids = new List<string>();
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            foreach (var username in usernames)
            {
                try
                {
                    var url = $"http://localhost:8082/admin/realms/chatapp/users?username={username}";
                    var resp = await client.GetAsync(url);
                    resp.EnsureSuccessStatusCode();

                    var body = await resp.Content.ReadAsStringAsync();
                    var users = System.Text.Json.JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(body);

                    if (users != null && users.Count > 0)
                    {
                        var guid = users[0].GetProperty("id").GetString();
                        if (!string.IsNullOrWhiteSpace(guid))
                            guids.Add(guid);
                    }
                }
                catch
                {
                    // optional: log lỗi
                }
            }

            return guids;
        }
    }
}
