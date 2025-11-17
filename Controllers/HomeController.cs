using ChatByWeb.Models;
using ChatByWeb.Services.Api;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatByWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly string _conversationBaseUrl;
        private readonly string _messageBaseUrl;

        public HomeController(IApiClient apiClient, IConfiguration configuration)
        {
            _apiClient = apiClient;
            _conversationBaseUrl = configuration["Api:ConversationBaseUrl"];
            _messageBaseUrl = configuration["Api:MessageBaseUrl"];
        }

        // Hiển thị danh sách conversation
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("user_id");
            if (string.IsNullOrWhiteSpace(userId))
                return RedirectToAction("Index", "Login");

            List<ConversationModel> conversations;

            try
            {
                conversations = await _apiClient.GetAsync<List<ConversationModel>>(
                    _conversationBaseUrl,
                    new Dictionary<string, string> { ["X-User-Id"] = userId }
                ) ?? new List<ConversationModel>();
            }
            catch
            {
                conversations = new List<ConversationModel>();
            }

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
            var userId = HttpContext.Session.GetString("user_id");
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(otherUsers))
                return RedirectToAction("Index");

            var users = otherUsers.Split(',')
                                  .Select(u => u.Trim())
                                  .Where(u => !string.IsNullOrWhiteSpace(u))
                                  .ToList();

            // Thêm chính mình vào members
            users.Insert(0, userId);

            bool isDirect = users.Count == 2; // 2 người => direct chat, nhiều hơn => group
            string title = isDirect
                ? $"Chat giữa {users[0]} và {users[1]}"
                : (string.IsNullOrWhiteSpace(groupTitle) ? $"Nhóm chat ({string.Join(", ", users.Skip(1))})" : groupTitle);

            var createConversationData = new
            {
                isDirect,
                members = users,
                title,
                idempotencyKey = Guid.NewGuid().ToString()
            };

            try
            {
                await _apiClient.PostAsync<object>(
                    _conversationBaseUrl,
                    createConversationData,
                    new Dictionary<string, string> { ["X-User-Id"] = userId }
                );
            }
            catch { }

            return RedirectToAction("Index");
        }

        // Send message
        [HttpPost]
        public async Task<IActionResult> SendMessage(string conversationId, string content)
        {
            var userId = HttpContext.Session.GetString("user_id");
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(conversationId)) { 
                Console.WriteLine("______________________________________________________");
                return BadRequest();
            }   

            var messageData = new
            {
                conversationId = conversationId,
                senderId = userId,
                content = content
            };

            try
            {
                await _apiClient.PostAsync<object>(
                    $"{_messageBaseUrl}/send",
                    messageData,
                    new Dictionary<string, string> { ["X-User-Id"] = userId }
                );
            }
            catch {
                Console.WriteLine("Wrong____________________________________________-");
            }

            return RedirectToAction("ChatPartial", new { id = conversationId });
        }
    }
}
