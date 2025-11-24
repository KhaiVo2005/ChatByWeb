using ChatByWeb.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ChatByWeb.Controllers
{
    public class RegisterController : Controller
    {
        private readonly KeycloakAuthService _auth;

        public RegisterController(KeycloakAuthService auth)
        {
            _auth = auth;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Index(string username, string email, string firstName, string lastName, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Tất cả các thông tin đều bắt buộc, không được để trống.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Password và confirm password không khớp.";
                return View();
            }

            var success = await _auth.RegisterAsync(username, email, firstName, lastName, password);
            if (!success)
            {
                ViewBag.Error = "Đăng ký thất bại.";
                return View();
            }

            var token = await _auth.LoginAsync(username, password);
            if (token == null)
            {
                ViewBag.Error = "Rớt môn ròi :(((.";
                return View();
            }

            HttpContext.Session.SetString("user_id", username);
            HttpContext.Session.SetString("access_token", token.access_token);

            return RedirectToAction("Index", "Home");
        }
    }
}
