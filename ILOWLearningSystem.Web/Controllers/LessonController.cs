using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ILOWLearningSystem.Web.Controllers;

// Member 3: Lesson & Assignment Module + AI-Generated Flashcard Revision Tool
[Authorize]
public class LessonController : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index(int? courseId = null)
    {
        ViewData["CourseId"] = courseId;
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Details(int id = 1)
    {
        ViewData["LessonId"] = id;
        return View();
    }

    [HttpGet]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Lecturer}")]
    public IActionResult Create(int courseId = 1)
    {
        return View(new LessonCreateViewModel { CourseId = courseId });
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Lecturer}")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(LessonCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
    }
}
