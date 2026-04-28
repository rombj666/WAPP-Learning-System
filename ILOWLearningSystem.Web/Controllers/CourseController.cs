using ILOWLearningSystem.Web.Data;
using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ILOWLearningSystem.Web.Controllers;

[Authorize]
public class CourseController : Controller
{
    private readonly AppDbContext _db;

    public CourseController(AppDbContext db)
    {
        _db = db;
    }

    // ========== 只有一个 GetCurrentUserId ==========
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        var email = User.Identity?.Name;
        if (!string.IsNullOrEmpty(email))
        {
            return _db.Users.Where(u => u.Email == email).Select(u => u.UserId).FirstOrDefault();
        }

        return 0;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var courses = await _db.Courses.ToListAsync();

        if (User.Identity?.IsAuthenticated ?? false)
        {
            var userId = GetCurrentUserId();
            var enrolledCourseIds = await _db.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();
            ViewBag.EnrolledCourseIds = enrolledCourseIds;
        }

        return View(courses);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var course = _db.Courses
            .Include(c => c.Lessons)
            .Include(c => c.Assignments)
            .FirstOrDefault(c => c.CourseId == id);

        if (course == null)
        {
            return NotFound();
        }

        if (User.IsInRole("Admin") || User.IsInRole("Lecturer"))
        {
            ViewBag.IsEnrolled = true;
        }
        else
        {
            var userId = GetCurrentUserId();
            ViewBag.IsEnrolled = _db.Enrollments.Any(e => e.UserId == userId && e.CourseId == id && e.Status == "Active");
        }

        return View(course);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Student)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return RedirectToAction("Login", "Account");

        var exists = await _db.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == id);
        if (!exists)
        {
            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = id,
                EnrolledAt = DateTime.UtcNow,
                Status = "Active"
            };
            _db.Enrollments.Add(enrollment);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Successfully enrolled in the course!";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Lecturer}")]
    public IActionResult Create()
    {
        return View(new CourseCreateViewModel());
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Lecturer}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var course = new Course
        {
            Title = model.Title,
            Description = model.Description,
            Category = model.Category,
            LecturerName = model.LecturerName,
            CreatedAt = DateTime.UtcNow
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}