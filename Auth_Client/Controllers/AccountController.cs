using Auth_Data.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Security.Claims;

namespace Auth_Client.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public AccountController(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var url = $"{_config["ApiUrl"]}/register";
            var response = await _httpClient.PostAsJsonAsync(url, model);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Login");

            ModelState.AddModelError("", "Registration failed.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_config["ApiUrl"]}/login", model);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View(model);
            }

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Email),
                new Claim("Token", token.Token)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }
        public class TokenResponse
        {
            public string Token { get; set; }
        }
    }
}
