using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JWT.Migrations
{
    /// <inheritdoc />
    public partial class CourseUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Midterm",
                table: "Courses",
                newName: "MidTerm");

            migrationBuilder.RenameColumn(
                name: "Course_degree",
                table: "Courses",
                newName: "TotalMark");

            migrationBuilder.AddColumn<int>(
                name: "Lab",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "has_Lab",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lab",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "has_Lab",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "MidTerm",
                table: "Courses",
                newName: "Midterm");

            migrationBuilder.RenameColumn(
                name: "TotalMark",
                table: "Courses",
                newName: "Course_degree");
        }
    }
}
