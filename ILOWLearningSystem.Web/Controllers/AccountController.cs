using ILOWLearningSystem.Web.Data;
using ILOWLearningSystem.Web.Models;
using ILOWLearningSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ILOWLearningSystem.Web.Controllers;

// Member 1: User Account Module + Smart Assist Learning Companion
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;

    public AccountController(
        IAuthService authService,
        AppDbContext db,
        IEmailService emailService)
    {
        _authService = authService;
        _db = db;
        _emailService = emailService;
    }

    // =========================
    // LOGIN
    // =========================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
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

        var (ok, errorMessage) =
            await _authService.SignInAsync(
                HttpContext,
                model.Email,
                model.Password);

        if (!ok)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl)
            && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null)
        {
            await _authService.SignOutAsync(HttpContext);

            return RedirectToAction("Login");
        }

        // SAVE SESSION
        HttpContext.Session.SetString(
            "UserId",
            user.UserId.ToString());

        // ADMIN / LECTURER
        if (user.Role == UserRoles.Admin
            || user.Role == UserRoles.Lecturer)
        {
            return RedirectToAction(
                "Dashboard",
                "Admin");
        }

        // STUDENT
        return RedirectToAction(
            "Dashboard",
            "Home");
    }

    // =========================
    // REGISTER
    // =========================

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

        var (ok, errorMessage) =
            await _authService.RegisterAsync(
                model.FullName,
                model.Email,
                model.Password,
                UserRoles.Student);

        if (!ok)
        {
            ModelState.AddModelError(
                string.Empty,
                errorMessage);

            return View(model);
        }

        await _authService.SignInAsync(
            HttpContext,
            model.Email,
            model.Password);

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user != null)
        {
            HttpContext.Session.SetString(
                "UserId",
                user.UserId.ToString());
        }

        return RedirectToAction(
            "Dashboard",
            "Home");
    }

    // =========================
    // FORGOT PASSWORD
    // =========================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "No account found with that email.");

            return View(model);
        }

        var otp = new Random()
            .Next(100000, 999999)
            .ToString();

        user.ResetOtp = otp;
        user.ResetOtpExpiry = DateTime.UtcNow.AddMinutes(5);

        await _db.SaveChangesAsync();

        var subject = "ILOW Password Reset OTP";

        var body =
            $"Your OTP code is: {otp}. It will expire in 5 minutes.";

        await _emailService.SendEmailAsync(
            model.Email,
            subject,
            body);

        TempData["OtpMessage"] =
            "OTP has been sent to your email.";

        return RedirectToAction(
            "VerifyOtp",
            new { email = model.Email });
    }

    // =========================
    // VERIFY OTP
    // =========================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult VerifyOtp(string email)
    {
        return View(new VerifyOtpViewModel
        {
            Email = email
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(
        VerifyOtpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Account not found.");

            return View(model);
        }

        if (string.IsNullOrWhiteSpace(user.ResetOtp)
            || user.ResetOtpExpiry == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "No OTP request found.");

            return View(model);
        }

        if (DateTime.UtcNow > user.ResetOtpExpiry.Value)
        {
            ModelState.AddModelError(
                string.Empty,
                "OTP has expired.");

            return View(model);
        }

        if (!string.Equals(
            user.ResetOtp,
            model.Otp,
            StringComparison.Ordinal))
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid OTP.");

            return View(model);
        }

        return RedirectToAction(
            "ResetPassword",
            new
            {
                email = model.Email,
                otp = model.Otp
            });
    }

    // =========================
    // RESET PASSWORD
    // =========================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(
        string email,
        string otp)
    {
        return View(new ResetPasswordViewModel
        {
            Email = email,
            Otp = otp
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Account not found.");

            return View(model);
        }

        if (user.ResetOtp != model.Otp)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid OTP.");

            return View(model);
        }

        user.Password = model.NewPassword;

        user.ResetOtp = null;
        user.ResetOtpExpiry = null;

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Password reset successful.";

        return RedirectToAction("Login");
    }

    // =========================
    // LOGOUT
    // =========================

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();

        await _authService.SignOutAsync(HttpContext);

        return RedirectToAction(
            "Login",
            "Account");
    }

    // =========================
    // PROFILE
    // =========================

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim)
            || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login");
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        return View(user);
    }

    // =========================
    // EDIT PROFILE
    // =========================

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim)
            || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login");
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var model = new EditProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            ProfileThemeColor =
                string.IsNullOrWhiteSpace(user.ProfileThemeColor)
                ? "#163b7a"
                : user.ProfileThemeColor
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(
        EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim)
            || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login");
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        user.FullName = model.FullName;

        user.ProfileThemeColor =
            string.IsNullOrWhiteSpace(model.ProfileThemeColor)
            ? "#163b7a"
            : model.ProfileThemeColor;

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Profile updated successfully.";

        return RedirectToAction("Profile");
    }
}