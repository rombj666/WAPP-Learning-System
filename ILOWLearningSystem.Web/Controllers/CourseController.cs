using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ILOWLearningSystem.Web.Controllers;

// Member 2: Course Module + Daily Tasks System
[Authorize]
public class CourseController : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Details(int id = 1)
    {
        ViewData["CourseId"] = id;
        return View();
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
    public IActionResult Create(CourseCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}
