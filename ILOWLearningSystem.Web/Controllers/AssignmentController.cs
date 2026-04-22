using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ILOWLearningSystem.Web.Controllers;

// Member 3: Lesson & Assignment Module + AI-Generated Flashcard Revision Tool
[Authorize]
public class AssignmentController : Controller
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
        ViewData["AssignmentId"] = id;
        return View();
    }

    [HttpGet]
    public IActionResult Submit(int id = 1)
    {
        return View(new SubmitAssignmentViewModel { AssignmentId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Submit(SubmitAssignmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(nameof(Details), new { id = model.AssignmentId });
    }

    [HttpGet]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Lecturer}")]
    public IActionResult Create(int courseId = 1)
    {
        return View(new AssignmentCreateViewModel { CourseId = courseId });
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Lecturer}")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(AssignmentCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
    }
}
