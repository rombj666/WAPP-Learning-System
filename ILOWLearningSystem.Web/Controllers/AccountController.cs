using ILOWLearningSystem.Web.Models;
using ILOWLearningSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ILOWLearningSystem.Web.Controllers;

// Member 1: User Account Module + Smart Assist Learning Companion
public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var ok = await _authService.SignInAsync(HttpContext, model.Email, model.Password);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Dashboard", "Student");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (ok, errorMessage) = await _authService.RegisterAsync(model.FullName, model.Email, model.Password, model.Role);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(model);
        }

        await _authService.SignInAsync(HttpContext, model.Email, model.Password);
        return RedirectToAction("Dashboard", "Student");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _authService.SignOutAsync(HttpContext);
        return RedirectToAction("Index", "Home");
    }
}
