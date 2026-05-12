using ILOWLearningSystem.Web.Data;
using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace ILOWLearningSystem.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return RedirectToAction("Login", "Account");
        }

        int userId = int.Parse(userIdClaim);

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var enrolledCourses = await _db.Enrollments
            .Where(e => e.UserId == userId)
            .Include(e => e.Course)
            .Select(e => e.Course!)
            .ToListAsync();

        var courseIds = enrolledCourses
            .Select(c => c.CourseId)
            .ToList();

        var upcomingAssignments = await _db.Assignments
            .Where(a =>
                courseIds.Contains(a.CourseId) &&
                a.DueDate != null)
            .OrderBy(a => a.DueDate)
            .ToListAsync();

        var recentSubmissions = await _db.Submissions
            .Where(s => s.UserId == userId)
            .Include(s => s.Assignment)
            .OrderByDescending(s => s.SubmittedAt)
            .Take(5)
            .ToListAsync();

        var model = new StudentDashboardViewModel
        {
            StudentName = user.FullName,
            EnrolledCourses = enrolledCourses,
            UpcomingAssignments = upcomingAssignments,
            RecentSubmissions = recentSubmissions
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}