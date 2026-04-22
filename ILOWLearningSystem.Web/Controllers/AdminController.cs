using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ILOWLearningSystem.Web.Controllers;

// Member 4: Admin / Lecturer Module + Stress Tracker Feature
[Authorize(Roles = UserRoles.Admin + "," + UserRoles.Lecturer)]
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult Dashboard()
    {
        return View();
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Admin)]
    public IActionResult ManageUsers()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ManageCourses()
    {
        return View(new CourseCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ManageCourses(CourseCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(nameof(ManageCourses));
    }

    [HttpGet]
    public IActionResult ManageLessons(int courseId = 1)
    {
        return View(new LessonCreateViewModel { CourseId = courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ManageLessons(LessonCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(nameof(ManageLessons), new { courseId = model.CourseId });
    }

    [HttpGet]
    public IActionResult ManageAssignments(int courseId = 1)
    {
        return View(new AssignmentCreateViewModel { CourseId = courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ManageAssignments(AssignmentCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(nameof(ManageAssignments), new { courseId = model.CourseId });
    }
}