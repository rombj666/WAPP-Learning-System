using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ILOWLearningSystem.Web.Controllers;

// Member 1: User Account Module + Smart Assist Learning Companion
[Authorize]
public class StudentController : Controller
{
    [HttpGet]
    public IActionResult Dashboard()
    {
        return View();
    }
}
