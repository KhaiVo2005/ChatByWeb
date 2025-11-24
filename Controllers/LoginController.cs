using ChatByWeb.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatByWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAuthService _auth;

        public LoginController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username và password không được để trống.";
                return View();
            }

            var token = await _auth.LoginAsync(username, password);

            if (token == null)
            {
                ViewBag.Error = "Đăng nhập thất bại.";
                return View();
            }

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token.access_token);
            var userGuid = jwt.Subject;

            // Dùng username làm user_id luôn
            HttpContext.Session.SetString("user_id", username);
            HttpContext.Session.SetString("access_token", token.access_token);
            HttpContext.Session.SetString("guid", userGuid);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
