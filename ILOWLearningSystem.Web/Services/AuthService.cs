using System.Security.Claims;
using ILOWLearningSystem.Web.Data;
using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace ILOWLearningSystem.Web.Services;

// Member 1: User Account Module + Smart Assist Learning Companion
public interface IAuthService
{
    Task<bool> SignInAsync(HttpContext httpContext, string email, string password);
    Task SignOutAsync(HttpContext httpContext);
    Task<(bool Ok, string ErrorMessage)> RegisterAsync(string fullName, string email, string password, string role);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> SignInAsync(HttpContext httpContext, string email, string password)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            return false;
        }

        if (!string.Equals(user.Password, password, StringComparison.Ordinal))
        {
            return false;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true
            });

        return true;
    }

    public Task SignOutAsync(HttpContext httpContext)
    {
        return httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<(bool Ok, string ErrorMessage)> RegisterAsync(string fullName, string email, string password, string role)
    {
        var exists = await _db.Users.AsNoTracking().AnyAsync(u => u.Email == email);
        if (exists)
        {
            return (false, "Email already exists.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            role = UserRoles.Student;
        }

        var user = new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            Password = password,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return (true, string.Empty);
    }
}
