using ILOWLearningSystem.Web.Data;
using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ILOWLearningSystem.Web.Controllers;

[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Lecturer}")]
public class EnrollmentController : Controller
{
    private readonly AppDbContext _db;

    public EnrollmentController(AppDbContext db)
    {
        _db = db;
    }

    // =========================
    // PENDING ENROLLMENTS
    // =========================
    [HttpGet]
    public async Task<IActionResult> Pending()
    {
        var pendingEnrollments = await _db.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
            .Where(e => e.Status == "Pending")
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        return View(pendingEnrollments);
    }

    // =========================
    // APPROVE
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.EnrollmentId == id);

        if (enrollment == null)
        {
            return NotFound();
        }

        enrollment.Status = "Active";

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Enrollment approved successfully.";

        return RedirectToAction("Pending");
    }

    // =========================
    // DECLINE
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decline(int id)
    {
        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.EnrollmentId == id);

        if (enrollment == null)
        {
            return NotFound();
        }

        enrollment.Status = "Declined";

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Enrollment declined.";

        return RedirectToAction("Pending");
    }
}