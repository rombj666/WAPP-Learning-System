using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ILOWLearningSystem.Web.Models;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Lecturer = "Lecturer";
    public const string Student = "Student";
}

// Member 1: User Account Module + Smart Assist Learning Companion
public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = UserRoles.Student;

    public DateTime CreatedAt { get; set; }

    public string? ResetOtp { get; set; }

    public DateTime? ResetOtpExpiry { get; set; }

    public string? ProfileThemeColor { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public ICollection<DailyTask> DailyTasks { get; set; } = new List<DailyTask>();
    public ICollection<StressRecord> StressRecords { get; set; } = new List<StressRecord>();
    public ICollection<Announcement> AnnouncementsCreated { get; set; } = new List<Announcement>();
}

// Member 2: Course Module + Daily Tasks System

public class Course
{
    public int CourseId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public string? LecturerName { get; set; }

    public int? LecturerId { get; set; }

    public User? Lecturer { get; set; }

    public string? ImagePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}


public class Enrollment
{
    [Key]
    public int EnrollmentId { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public string Status { get; set; } = "Pending";

    // Navigation Properties
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("CourseId")]
    public Course? Course { get; set; }
}

public class DailyTask
{
    [Key]
    public int TaskId { get; set; }

    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}

// Member 3: Lesson & Assignment Module + AI-Generated Flashcard Revision Tool
public class Lesson
{
    [Key]
    public int LessonId { get; set; }

    [ForeignKey(nameof(Course))]
    public int CourseId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    [MaxLength(1000)]
    public string? SlidePath { get; set; }

    [MaxLength(1000)]
    public string? VideoPath { get; set; }

    public DateTime CreatedAt { get; set; }

    public Course? Course { get; set; }
    public ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
}

public class Assignment
{
    [Key]
    public int AssignmentId { get; set; }

    [ForeignKey(nameof(Course))]
    public int CourseId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public int TotalMarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public Course? Course { get; set; }
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}

public class Submission
{
    [Key]
    public int SubmissionId { get; set; }

    [ForeignKey(nameof(Assignment))]
    public int AssignmentId { get; set; }

    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    [MaxLength(1000)]
    public string? FilePath { get; set; }

    [MaxLength(4000)]
    public string? SubmissionText { get; set; }

    public DateTime SubmittedAt { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Submitted";

    public int? Score { get; set; }

    [MaxLength(2000)]
    public string? Feedback { get; set; }

    public Assignment? Assignment { get; set; }
    public User? User { get; set; }
}

public class Flashcard
{
    [Key]
    public int FlashcardId { get; set; }

    [ForeignKey(nameof(Lesson))]
    public int LessonId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Question { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Answer { get; set; } = string.Empty;

    public int DifficultyLevel { get; set; }

    public DateTime CreatedAt { get; set; }

    public Lesson? Lesson { get; set; }
}

// Member 4: Admin / Lecturer Module + Stress Tracker Feature
public class StressRecord
{
    [Key]
    public int StressRecordId { get; set; }

    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    public int StressLevel { get; set; }

    [MaxLength(2000)]
    public string? Note { get; set; }

    public DateTime RecordedAt { get; set; }

    public User? User { get; set; }
}

public class Announcement
{
    [Key]
    public int AnnouncementId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    [ForeignKey(nameof(CreatedByUser))]
    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public User? CreatedByUser { get; set; }
}


