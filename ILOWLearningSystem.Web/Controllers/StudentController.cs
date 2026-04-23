using ILOWLearningSystem.Web.Data;
using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ILOWLearningSystem.Web.Controllers;

[Authorize(Roles = UserRoles.Student)]
public class StudentController : Controller
{
    private readonly AppDbContext _db;

    public StudentController(AppDbContext db)
    {
        _db = db;
    }

    private int GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdStr, out var id) ? id : 0;
    }

    [Authorize(Roles = UserRoles.Student)]
    public async Task<IActionResult> Dashboard()
    {
        var userId = GetCurrentUserId();
        var user = await _db.Users.FindAsync(userId);

        var viewModel = new StudentDashboardViewModel
        {
            StudentName = user?.FullName ?? "Student",
            EnrolledCourses = await _db.Enrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                .Select(e => e.Course!)
                .ToListAsync(),
            UpcomingAssignments = await _db.Assignments
                .Include(a => a.Course)
                .Where(a => _db.Enrollments.Any(e => e.UserId == userId && e.CourseId == a.CourseId))
                .Where(a => a.DueDate > DateTime.UtcNow)
                .OrderBy(a => a.DueDate)
                .Take(5)
                .ToListAsync(),
            RecentSubmissions = await _db.Submissions
                .Where(s => s.UserId == userId)
                .Include(s => s.Assignment)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(5)
                .ToListAsync()
        };

        return View(viewModel);
    }
}
