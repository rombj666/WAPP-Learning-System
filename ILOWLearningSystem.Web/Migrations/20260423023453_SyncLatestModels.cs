using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ILOWLearningSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class SyncLatestModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubmissionText",
                table: "Submissions",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmissionText",
                table: "Submissions");
        }
    }
}
