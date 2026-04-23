using ILOWLearningSystem.Web.Data;
using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ILOWLearningSystem.Web.Controllers;

[Authorize]
public class SubmissionController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public SubmissionController(AppDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    private int GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdStr, out var id) ? id : 0;
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Student)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitAssignmentViewModel model)
    {
        var userId = GetCurrentUserId();
        
        // Validation: at least one of them should be provided
        if (string.IsNullOrWhiteSpace(model.Notes) && model.File == null)
        {
            TempData["ErrorMessage"] = "Please provide either notes or a file for your submission.";
            return RedirectToAction("Details", "Assignment", new { id = model.AssignmentId });
        }

        var existingSubmission = await _db.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == model.AssignmentId && s.UserId == userId);

        string? filePath = null;
        if (model.File != null)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "submissions");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{model.File.FileName}";
            filePath = Path.Combine("uploads", "submissions", uniqueFileName);
            var fullPath = Path.Combine(_environment.WebRootPath, filePath);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await model.File.CopyToAsync(fileStream);
            }
        }

        if (existingSubmission != null)
        {
            existingSubmission.SubmissionText = model.Notes;
            if (filePath != null)
            {
                // Delete old file if exists
                if (!string.IsNullOrEmpty(existingSubmission.FilePath))
                {
                    var oldFullPath = Path.Combine(_environment.WebRootPath, existingSubmission.FilePath);
                    if (System.IO.File.Exists(oldFullPath))
                    {
                        System.IO.File.Delete(oldFullPath);
                    }
                }
                existingSubmission.FilePath = filePath;
            }
            existingSubmission.SubmittedAt = DateTime.UtcNow;
            existingSubmission.Status = "Pending Review"; // As requested: after student submits
        }
        else
        {
            var submission = new Submission
            {
                AssignmentId = model.AssignmentId,
                UserId = userId,
                SubmissionText = model.Notes,
                FilePath = filePath,
                SubmittedAt = DateTime.UtcNow,
                Status = "Pending Review" // As requested: after student submits
            };

            _db.Submissions.Add(submission);
        }

        try
        {
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Assignment submitted successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while saving your submission. Please try again.";
        }

        return RedirectToAction("Details", "Assignment", new { id = model.AssignmentId });
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Student)]
    public async Task<IActionResult> MyStatus()
    {
        var userId = GetCurrentUserId();

        var submissions = await _db.Submissions
            .Where(s => s.UserId == userId)
            .Include(s => s.Assignment)
                .ThenInclude(a => a!.Course)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();

        return View(submissions);
    }
}
