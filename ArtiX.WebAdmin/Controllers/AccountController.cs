using System.Net.Http.Json;
using System.Security.Claims;
using ArtiX.WebAdmin.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtiX.WebAdmin.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private const string ApiClientName = "ArtiXApi";
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["Title"] = "Sign in";
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = _httpClientFactory.CreateClient(ApiClientName);
        var response = await client.PostAsJsonAsync("/auth/login", new
        {
            email = model.Email,
            password = model.Password
        });

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (!string.IsNullOrWhiteSpace(content?.Token))
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, model.Email),
                    new("access_token", content.Token)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Dashboard");
            }
        }

        ModelState.AddModelError(string.Empty, "Invalid credentials or the authentication service is unavailable.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        ViewData["Title"] = "Create account";
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = _httpClientFactory.CreateClient(ApiClientName);
        var response = await client.PostAsJsonAsync("/auth/register", new
        {
            firstName = model.FirstName,
            lastName = model.LastName,
            email = model.Email,
            phoneNumber = model.PhoneNumber,
            password = model.Password,
            confirmPassword = model.ConfirmPassword
            // TODO: include additional fields required by ArtiX.Api (e.g. company/role/branch identifiers).
        });

        if (response.IsSuccessStatusCode)
        {
            // Optionally sign in immediately after registration using the login endpoint
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError(string.Empty, "Registration failed or the service is unavailable. Please try again.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
