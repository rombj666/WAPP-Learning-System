using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ILOWLearningSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLecturerSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "CourseId", "Category", "CreatedAt", "Description", "ImagePath", "LecturerName", "Title" },
                values: new object[] { 2, "Programming", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "This course belongs to Lecturer One for testing.", null, "Lecturer One", "Lecturer's First Course" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "FullName", "Password", "ProfileThemeColor", "ResetOtp", "ResetOtpExpiry", "Role" },
                values: new object[] { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "lecturer1@ilow.local", "Lecturer One", "lecturer123", null, null, null, "Lecturer" });

            migrationBuilder.InsertData(
                table: "Enrollments",
                columns: new[] { "EnrollmentId", "CourseId", "EnrolledAt", "Status", "UserId" },
                values: new object[] { 2, 2, new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), "Active", 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Enrollments",
                keyColumn: "EnrollmentId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: 2);
        }
    }
}
