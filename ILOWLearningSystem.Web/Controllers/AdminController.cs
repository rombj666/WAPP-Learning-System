using ILOWLearningSystem.Web.Data;
using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ILOWLearningSystem.Web.Controllers;

// Member 4: Admin / Lecturer Module
[Authorize(Roles = UserRoles.Admin + "," + UserRoles.Lecturer)]
public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    // ==================== 获取当前用户信息 ====================
    private int GetCurrentUserId()
    {
        var email = User.Identity?.Name;
        return _db.Users.Where(u => u.Email == email).Select(u => u.UserId).FirstOrDefault();
    }

    // ==================== DASHBOARD ====================
    [HttpGet]
    public IActionResult Dashboard()
    {
        ViewBag.IsAdmin = User.IsInRole(UserRoles.Admin);

        ViewBag.TotalAdmins = _db.Users.Count(u => u.Role == UserRoles.Admin);
        ViewBag.TotalLecturers = _db.Users.Count(u => u.Role == UserRoles.Lecturer);
        ViewBag.TotalStudents = _db.Users.Count(u => u.Role == UserRoles.Student);
        ViewBag.TotalCourses = _db.Courses.Count();
        ViewBag.TotalAnnouncements = _db.Announcements.Count();

        return View();
    }

    // ==================== MANAGE USERS (仅 Admin) ====================
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

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    [ValidateAntiForgeryToken]
    public IActionResult CreateLecturer(string FullName, string Email, string Password)
    {
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            TempData["Error"] = "All fields are required.";
            return RedirectToAction(nameof(ManageUsers));
        }

        if (_db.Users.Any(u => u.Email == Email))
        {
            TempData["Error"] = "A user with this email already exists.";
            return RedirectToAction(nameof(ManageUsers));
        }

        var user = new User
        {
            FullName = FullName,
            Email = Email,
            Password = Password,
            Role = UserRoles.Lecturer,
            CreatedAt = DateTime.Now
        };

        _db.Users.Add(user);
        _db.SaveChanges();
        TempData["Message"] = $"Lecturer '{FullName}' created successfully.";
        return RedirectToAction(nameof(ManageUsers));
    }

    // ==================== MANAGE COURSES ====================
    [HttpGet]
    public IActionResult ManageCourses()
    {
        List<Course> courses;

        if (User.IsInRole(UserRoles.Admin))
        {
            courses = _db.Courses.OrderBy(c => c.Title).ToList();
        }
        else
        {
            var lecturerName = _db.Users.Where(u => u.UserId == GetCurrentUserId()).Select(u => u.FullName).FirstOrDefault();
            courses = _db.Courses.Where(c => c.LecturerName == lecturerName).OrderBy(c => c.Title).ToList();
        }

        ViewBag.IsAdmin = User.IsInRole(UserRoles.Admin);
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
    public async Task<IActionResult> EditCourse(Course course, IFormFile? ImageFile, bool RemoveImage = false)
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

        // 删除旧图片
        if (RemoveImage && !string.IsNullOrEmpty(existing.ImagePath))
        {
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existing.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }
            existing.ImagePath = null;
        }

        // 上传新图片
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
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existing.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
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

    // ==================== MANAGE LESSONS ====================
    [HttpGet]
    public IActionResult ManageLessons(int courseId = 0)
    {
        var lecturerName = _db.Users.Where(u => u.UserId == GetCurrentUserId()).Select(u => u.FullName).FirstOrDefault();
        var myCourseIds = _db.Courses.Where(c => c.LecturerName == lecturerName).Select(c => c.CourseId).ToList();

        List<Course> courses;
        List<Lesson> lessons;

        if (User.IsInRole(UserRoles.Admin))
        {
            courses = _db.Courses.OrderBy(c => c.Title).ToList();
            lessons = courseId == 0
                ? _db.Lessons.Include(l => l.Course).OrderBy(l => l.Title).ToList()
                : _db.Lessons.Include(l => l.Course).Where(l => l.CourseId == courseId).OrderBy(l => l.Title).ToList();
        }
        else
        {
            courses = _db.Courses.Where(c => c.LecturerName == lecturerName).OrderBy(c => c.Title).ToList();
            var filteredIds = courseId == 0 ? myCourseIds : new List<int> { courseId };
            lessons = _db.Lessons.Include(l => l.Course).Where(l => filteredIds.Contains(l.CourseId)).OrderBy(l => l.Title).ToList();
        }

        ViewBag.Courses = courses;
        ViewBag.IsAdmin = User.IsInRole(UserRoles.Admin);
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
    public IActionResult DeleteLesson(int id)
    {
        var lesson = _db.Lessons.Find(id);
        if (lesson != null)
        {
            _db.Lessons.Remove(lesson);
            _db.SaveChanges();
            TempData["Message"] = $"Lesson '{lesson.Title}' deleted successfully.";
        }
        return RedirectToAction(nameof(ManageLessons));
    }

    // ==================== MANAGE ASSIGNMENTS ====================
    [HttpGet]
    public IActionResult ManageAssignments(int courseId = 0)
    {
        var lecturerName = _db.Users.Where(u => u.UserId == GetCurrentUserId()).Select(u => u.FullName).FirstOrDefault();
        var myCourseIds = _db.Courses.Where(c => c.LecturerName == lecturerName).Select(c => c.CourseId).ToList();

        List<Course> courses;
        List<Assignment> assignments;

        if (User.IsInRole(UserRoles.Admin))
        {
            courses = _db.Courses.OrderBy(c => c.Title).ToList();
            assignments = courseId == 0
                ? _db.Assignments.Include(a => a.Course).OrderBy(a => a.Title).ToList()
                : _db.Assignments.Include(a => a.Course).Where(a => a.CourseId == courseId).OrderBy(a => a.Title).ToList();
        }
        else
        {
            courses = _db.Courses.Where(c => c.LecturerName == lecturerName).OrderBy(c => c.Title).ToList();
            var filteredIds = courseId == 0 ? myCourseIds : new List<int> { courseId };
            assignments = _db.Assignments.Include(a => a.Course).Where(a => filteredIds.Contains(a.CourseId)).OrderBy(a => a.Title).ToList();
        }

        ViewBag.Courses = courses;
        ViewBag.IsAdmin = User.IsInRole(UserRoles.Admin);
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

    // ==================== MANAGE ANNOUNCEMENTS (仅 Admin) ====================
    [HttpGet]
    [Authorize(Roles = UserRoles.Admin)]
    public IActionResult ManageAnnouncements()
    {
        var announcements = _db.Announcements.Include(a => a.CreatedByUser).OrderByDescending(a => a.CreatedAt).ToList();
        return View(announcements);
    }

    // ==================== HELPER: 文件上传 ====================
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
}