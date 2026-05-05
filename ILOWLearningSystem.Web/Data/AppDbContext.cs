using ILOWLearningSystem.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ILOWLearningSystem.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public DbSet<DailyTask> DailyTasks => Set<DailyTask>();
    public DbSet<StressRecord> StressRecords => Set<StressRecord>();
    public DbSet<Announcement> Announcements => Set<Announcement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.UserId, e.CourseId })
            .IsUnique();

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.User)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Lesson>()
            .HasOne(l => l.Course)
            .WithMany(c => c.Lessons)
            .HasForeignKey(l => l.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Course)
            .WithMany(c => c.Assignments)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Assignment)
            .WithMany(a => a.Submissions)
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Submission>()
            .HasOne(s => s.User)
            .WithMany(u => u.Submissions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Flashcard>()
            .HasOne(f => f.Lesson)
            .WithMany(l => l.Flashcards)
            .HasForeignKey(f => f.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DailyTask>()
            .HasOne(t => t.User)
            .WithMany(u => u.DailyTasks)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StressRecord>()
            .HasOne(r => r.User)
            .WithMany(u => u.StressRecords)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Announcement>()
            .HasOne(a => a.CreatedByUser)
            .WithMany(u => u.AnnouncementsCreated)
            .HasForeignKey(a => a.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        Seed(modelBuilder);
    }

    private static void Seed(ModelBuilder modelBuilder)
    {
        var seedCreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var seedEnrolledAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var seedDueDate = new DateTime(2026, 2, 1, 23, 59, 0, DateTimeKind.Utc);

        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 1,
                FullName = "Admin User",
                Email = "admin@ilow.local",
                Password = "admin123",
                Role = UserRoles.Admin,
                CreatedAt = seedCreatedAt
            },
            new User
            {
                UserId = 2,
                FullName = "Student One",
                Email = "student1@ilow.local",
                Password = "student123",
                Role = UserRoles.Student,
                CreatedAt = seedCreatedAt
            },
            new User
            {
                UserId = 3,
                FullName = "Lecturer One",
                Email = "lecturer1@ilow.local",
                Password = "lecturer123",
                Role = UserRoles.Lecturer,
                CreatedAt = seedCreatedAt
            }
        );

        modelBuilder.Entity<Course>().HasData(
            new Course
            {
                CourseId = 1,
                Title = "Introduction to ILOW",
                Description = "Starter course used as seed data.",
                Category = "General",
                LecturerName = "Lecturer A",
                ImagePath = null,
                CreatedAt = seedCreatedAt
            },
            new Course
            {
                CourseId = 2,
                Title = "Lecturer's First Course",
                Description = "This course belongs to Lecturer One for testing.",
                Category = "Programming",
                LecturerName = "Lecturer One",
                ImagePath = null,
                CreatedAt = seedCreatedAt
            }
        );

        modelBuilder.Entity<Enrollment>().HasData(
            new Enrollment
            {
                EnrollmentId = 1,
                UserId = 2,
                CourseId = 1,
                EnrolledAt = seedEnrolledAt,
                Status = "Active"
            },
            new Enrollment
            {
                EnrollmentId = 2,
                UserId = 2,
                CourseId = 2,
                EnrolledAt = seedEnrolledAt,
                Status = "Active"
            }
        );

        modelBuilder.Entity<Lesson>().HasData(
            new Lesson
            {
                LessonId = 1,
                CourseId = 1,
                Title = "Welcome Lesson",
                Content = "Placeholder lesson content.",
                SlidePath = null,
                VideoPath = null,
                CreatedAt = seedCreatedAt
            }
        );

        modelBuilder.Entity<Assignment>().HasData(
            new Assignment
            {
                AssignmentId = 1,
                CourseId = 1,
                Title = "First Assignment",
                Description = "Placeholder assignment description.",
                DueDate = seedDueDate,
                TotalMarks = 100,
                CreatedAt = seedCreatedAt
            }
        );

        modelBuilder.Entity<Announcement>().HasData(
            new Announcement
            {
                AnnouncementId = 1,
                Title = "Welcome to I.L.O.W",
                Content = "This is a seeded announcement placeholder.",
                CreatedBy = 1,
                CreatedAt = seedCreatedAt
            }
        );
    }
}
