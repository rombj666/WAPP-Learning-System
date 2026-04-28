using ILOWLearningSystem.Web.Data;
using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ILOWLearningSystem.Web.Controllers;

// Member 4: Admin / Lecturer Module + Stress Tracker Feature
[Authorize(Roles = UserRoles.Admin + "," + UserRoles.Lecturer)]
public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Dashboard()
    {
        ViewBag.TotalUsers = _db.Users.Count();
        ViewBag.TotalCourses = _db.Courses.Count();
        ViewBag.TotalLessons = _db.Lessons.Count();
        ViewBag.TotalAssignments = _db.Assignments.Count();
        return View();
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Admin)]
    public IActionResult ManageUsers()
    {
        var users = _db.Users.OrderBy(u => u.FullName).ToList();
        return View(users);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteUser(int id)
    {
        var user = _db.Users.Find(id);
        if (user != null)
        {
            _db.Users.Remove(user);
            _db.SaveChanges();
            TempData["Message"] = $"{user.FullName} deleted successfully.";
        }
        return RedirectToAction(nameof(ManageUsers));
    }

    [HttpGet]
    public IActionResult ManageCourses()
    {
        var courses = _db.Courses.OrderBy(c => c.Title).ToList();
        return View(courses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(Course course, IFormFile? ImageFile)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill in all required fields.";
            return RedirectToAction(nameof(ManageCourses));
        }

        if (ImageFile != null && ImageFile.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(ImageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["Error"] = "Only .jpg, .jpeg, .png, .gif, .webp files are allowed.";
                return RedirectToAction(nameof(ManageCourses));
            }

            if (ImageFile.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "Image file size must be less than 5MB.";
                return RedirectToAction(nameof(ManageCourses));
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "courses");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            course.ImagePath = "/uploads/courses/" + fileName;
        }

        course.CreatedAt = DateTime.Now;
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();
        TempData["Message"] = $"Course '{course.Title}' created successfully.";
        return RedirectToAction(nameof(ManageCourses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(Course course, IFormFile? ImageFile)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid course data.";
            return RedirectToAction(nameof(ManageCourses));
        }

        var existing = _db.Courses.Find(course.CourseId);
        if (existing == null)
        {
            TempData["Error"] = "Course not found.";
            return RedirectToAction(nameof(ManageCourses));
        }

        bool RemoveImage = false;

        if (RemoveImage && !string.IsNullOrEmpty(existing.ImagePath))
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existing.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
            existing.ImagePath = null;
        }

        if (ImageFile != null && ImageFile.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(ImageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["Error"] = "Only .jpg, .jpeg, .png, .gif, .webp files are allowed.";
                return RedirectToAction(nameof(ManageCourses));
            }

            if (ImageFile.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "Image file size must be less than 5MB.";
                return RedirectToAction(nameof(ManageCourses));
            }

            if (!string.IsNullOrEmpty(existing.ImagePath))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existing.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "courses");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            existing.ImagePath = "/uploads/courses/" + fileName;
        }

        existing.Title = course.Title;
        existing.Description = course.Description;
        existing.Category = course.Category;
        existing.LecturerName = course.LecturerName;

        await _db.SaveChangesAsync();
        TempData["Message"] = $"Course '{course.Title}' updated successfully.";
        return RedirectToAction(nameof(ManageCourses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteCourse(int id)
    {
        var course = _db.Courses.Find(id);
        if (course != null)
        {
            _db.Courses.Remove(course);
            _db.SaveChanges();
            TempData["Message"] = $"Course '{course.Title}' deleted successfully.";
        }
        return RedirectToAction(nameof(ManageCourses));
    }

    [HttpGet]
    public IActionResult ManageLessons(int courseId = 0)
    {
        ViewBag.Courses = _db.Courses.OrderBy(c => c.Title).ToList();
        var lessons = courseId == 0
            ? _db.Lessons.Include(l => l.Course).OrderBy(l => l.Title).ToList()
            : _db.Lessons.Include(l => l.Course).Where(l => l.CourseId == courseId).OrderBy(l => l.Title).ToList();
        return View(lessons);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLesson(Lesson lesson, IFormFile? SlideFile, IFormFile? VideoFile)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill in all required fields.";
            return RedirectToAction(nameof(ManageLessons));
        }

        if (SlideFile != null && SlideFile.Length > 0)
        {
            lesson.SlidePath = await SaveUploadedFile(SlideFile, "slides", new[] { ".pdf", ".pptx", ".ppt", ".docx", ".doc" }, 20);
        }

        if (VideoFile != null && VideoFile.Length > 0)
        {
            lesson.VideoPath = await SaveUploadedFile(VideoFile, "videos", new[] { ".mp4", ".webm", ".mov", ".avi" }, 100);
        }

        lesson.CreatedAt = DateTime.Now;
        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();
        TempData["Message"] = $"Lesson '{lesson.Title}' created successfully.";
        return RedirectToAction(nameof(ManageLessons), new { courseId = lesson.CourseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLesson(Lesson lesson, IFormFile? SlideFile, IFormFile? VideoFile)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid lesson data.";
            return RedirectToAction(nameof(ManageLessons));
        }

        var existing = _db.Lessons.Find(lesson.LessonId);
        if (existing == null)
        {
            TempData["Error"] = "Lesson not found.";
            return RedirectToAction(nameof(ManageLessons));
        }

        if (SlideFile != null && SlideFile.Length > 0)
        {
            DeleteOldFile(existing.SlidePath);
            existing.SlidePath = await SaveUploadedFile(SlideFile, "slides", new[] { ".pdf", ".pptx", ".ppt", ".docx", ".doc" }, 20);
        }

        if (VideoFile != null && VideoFile.Length > 0)
        {
            DeleteOldFile(existing.VideoPath);
            existing.VideoPath = await SaveUploadedFile(VideoFile, "videos", new[] { ".mp4", ".webm", ".mov", ".avi" }, 100);
        }

        existing.Title = lesson.Title;
        existing.Content = lesson.Content;
        existing.CourseId = lesson.CourseId;

        await _db.SaveChangesAsync();
        TempData["Message"] = $"Lesson '{lesson.Title}' updated successfully.";
        return RedirectToAction(nameof(ManageLessons), new { courseId = lesson.CourseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveCourseImage(int id)
    {
        var course = _db.Courses.Find(id);
        if (course == null)
        {
            TempData["Error"] = "Course not found.";
            return RedirectToAction(nameof(ManageCourses));
        }

        if (!string.IsNullOrEmpty(course.ImagePath))
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        course.ImagePath = null;
        _db.SaveChanges();
        TempData["Message"] = $"Image removed from '{course.Title}'.";
        return RedirectToAction(nameof(ManageCourses));
    }

    private async Task<string> SaveUploadedFile(IFormFile file, string subfolder, string[] allowedExtensions, int maxSizeMB)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"File type '{extension}' is not allowed.");
        }

        if (file.Length > maxSizeMB * 1024 * 1024)
        {
            throw new InvalidOperationException($"File size exceeds {maxSizeMB}MB limit.");
        }

        var fileName = Guid.NewGuid().ToString() + extension;
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subfolder);

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{subfolder}/" + fileName;
    }

    private void DeleteOldFile(string? filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }

    [HttpGet]
    public IActionResult ManageAssignments(int courseId = 0)
    {
        ViewBag.Courses = _db.Courses.OrderBy(c => c.Title).ToList();
        var assignments = courseId == 0
            ? _db.Assignments.Include(a => a.Course).OrderBy(a => a.Title).ToList()
            : _db.Assignments.Include(a => a.Course).Where(a => a.CourseId == courseId).OrderBy(a => a.Title).ToList();
        return View(assignments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateAssignment(Assignment assignment)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill in all required fields.";
            return RedirectToAction(nameof(ManageAssignments));
        }

        assignment.CreatedAt = DateTime.Now;
        _db.Assignments.Add(assignment);
        _db.SaveChanges();
        TempData["Message"] = $"Assignment '{assignment.Title}' created successfully.";
        return RedirectToAction(nameof(ManageAssignments), new { courseId = assignment.CourseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditAssignment(Assignment assignment)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid assignment data.";
            return RedirectToAction(nameof(ManageAssignments));
        }

        var existing = _db.Assignments.Find(assignment.AssignmentId);
        if (existing == null)
        {
            TempData["Error"] = "Assignment not found.";
            return RedirectToAction(nameof(ManageAssignments));
        }

        existing.Title = assignment.Title;
        existing.Description = assignment.Description;
        existing.DueDate = assignment.DueDate;
        existing.TotalMarks = assignment.TotalMarks;
        existing.CourseId = assignment.CourseId;
        _db.SaveChanges();
        TempData["Message"] = $"Assignment '{assignment.Title}' updated successfully.";
        return RedirectToAction(nameof(ManageAssignments), new { courseId = assignment.CourseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteAssignment(int id)
    {
        var assignment = _db.Assignments.Find(id);
        if (assignment != null)
        {
            _db.Assignments.Remove(assignment);
            _db.SaveChanges();
            TempData["Message"] = $"Assignment '{assignment.Title}' deleted successfully.";
        }
        return RedirectToAction(nameof(ManageAssignments));
    }
}