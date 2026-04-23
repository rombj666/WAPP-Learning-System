using ILOWLearningSystem.Web.Data;
using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ILOWLearningSystem.Web.Controllers;

[Authorize]
public class AssignmentController : Controller
{
    private readonly AppDbContext _db;

    public AssignmentController(AppDbContext db)
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
    public async Task<IActionResult> ByCourse(int courseId)
    {
        var course = await _db.Courses
            .Include(c => c.Assignments)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

        if (course == null)
        {
            return NotFound();
        }

        if (User.IsInRole(UserRoles.Student))
        {
            var userId = GetCurrentUserId();
            var submissions = await _db.Submissions
                .Where(s => s.UserId == userId && course.Assignments.Select(a => a.AssignmentId).Contains(s.AssignmentId))
                .ToDictionaryAsync(s => s.AssignmentId, s => s.Status);
            ViewBag.Submissions = submissions;
        }

        return View(course);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var assignment = await _db.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.AssignmentId == id);

        if (assignment == null)
        {
            return NotFound();
        }

        if (User.IsInRole(UserRoles.Student))
        {
            var userId = GetCurrentUserId();
            var submission = await _db.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == id && s.UserId == userId);
            ViewBag.Submission = submission;
        }

        return View(assignment);
    }

    [HttpGet]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Lecturer}")]
    public IActionResult Create(int courseId)
    {
        return View(new AssignmentCreateViewModel { CourseId = courseId });
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Lecturer}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssignmentCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var assignment = new Assignment
        {
            CourseId = model.CourseId,
            Title = model.Title,
            Description = model.Description,
            DueDate = model.DueDate,
            TotalMarks = model.TotalMarks,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _db.Assignments.Add(assignment);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Assignment creation failed. Please try again.";
            return View(model);
        }

        return RedirectToAction(nameof(ByCourse), new { courseId = model.CourseId });
    }
}