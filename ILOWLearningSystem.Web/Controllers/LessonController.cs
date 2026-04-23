using ILOWLearningSystem.Web.Data;
using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ILOWLearningSystem.Web.Controllers;

[Authorize]
public class LessonController : Controller
{
    private readonly AppDbContext _db;

    public LessonController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ByCourse(int courseId)
    {
        var course = await _db.Courses
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var lesson = await _db.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.LessonId == id);

        if (lesson == null)
        {
            return NotFound();
        }

        return View(lesson);
    }

    [HttpGet]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Lecturer}")]
    public IActionResult Create(int courseId)
    {
        return View(new LessonCreateViewModel { CourseId = courseId });
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Lecturer}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var lesson = new Lesson
        {
            CourseId = model.CourseId,
            Title = model.Title,
            Content = model.Content,
            CreatedAt = DateTime.UtcNow
        };

        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(ByCourse), new { courseId = model.CourseId });
    }
}
