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

    private int GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdStr, out var id) ? id : 0;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var courses = await _db.Courses.ToListAsync();
        
        // If logged in as student, mark which ones they are enrolled in
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
    public async Task<IActionResult> Details(int id)
    {
        var course = await _db.Courses
            .Include(c => c.Lessons)
            .Include(c => c.Assignments)
            .FirstOrDefaultAsync(c => c.CourseId == id);

        if (course == null)
        {
            return NotFound();
        }

        // Check enrollment if logged in
        if (User.Identity?.IsAuthenticated ?? false)
        {
            var userId = GetCurrentUserId();
            var isEnrolled = await _db.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == id);
            ViewBag.IsEnrolled = isEnrolled;

            if (isEnrolled)
            {
                var submissions = await _db.Submissions
                    .Where(s => s.UserId == userId && course.Assignments.Select(a => a.AssignmentId).Contains(s.AssignmentId))
                    .ToDictionaryAsync(s => s.AssignmentId, s => s.Status);
                ViewBag.Submissions = submissions;
            }
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
