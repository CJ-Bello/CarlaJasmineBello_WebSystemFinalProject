using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using StudentGradeManagement.MVC.Models;

namespace StudentGradeManagement.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AccountController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Token") != null)
            {
                return RedirectToDashboard();
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var baseUrl = _configuration["ApiSettings:BaseUrl"];

                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    ModelState.AddModelError("", "API address is not configured.");
                    return View(model);
                }

                var client = _httpClientFactory.CreateClient();

                var loginResponse = await client.PostAsJsonAsync(
                    $"{baseUrl.TrimEnd('/')}/api/auth/login",
                    new
                    {
                        email = model.Email,
                        password = model.Password
                    });

                if (!loginResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid email or password."
                    );

                    return View(model);
                }

                var result = await loginResponse.Content
                    .ReadFromJsonAsync<ApiLoginResponse>();

                if (result == null || string.IsNullOrWhiteSpace(result.Token))
                {
                    ModelState.AddModelError(
                        "",
                        "Unable to process the login response."
                    );

                    return View(model);
                }

                HttpContext.Session.SetString("Token", result.Token);
                HttpContext.Session.SetString("Email", result.Email);
                HttpContext.Session.SetString("Role", result.Role);
                HttpContext.Session.SetString("FullName", result.FullName);

                return RedirectToDashboard();
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "Cannot connect to the API server. Make sure it is running."
                );

                return View(model);
            }
        }

        private IActionResult RedirectToDashboard()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role == "Faculty")
            {
                return RedirectToAction("Index", "Faculty");
            }

            if (role == "Student")
            {
                return RedirectToAction("Index", "Student");
            }

            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
