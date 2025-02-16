using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JWT.Migrations
{
    public partial class RecreateRelationshipBetweenDoctorMaterialCourses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_AspNetUsers_ApplicationUserId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ApplicationUserId",
                table: "Courses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseDoctors",
                table: "CourseDoctors");

            migrationBuilder.DropIndex(
                name: "IX_CourseDoctors_DoctorId",
                table: "CourseDoctors");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseDoctors",
                table: "CourseDoctors",
                columns: new[] { "DoctorId", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseDoctors_CourseId",
                table: "CourseDoctors",
                column: "CourseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseDoctors",
                table: "CourseDoctors");

            migrationBuilder.DropIndex(
                name: "IX_CourseDoctors_CourseId",
                table: "CourseDoctors");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "Courses",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseDoctors",
                table: "CourseDoctors",
                columns: new[] { "CourseId", "DoctorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ApplicationUserId",
                table: "Courses",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseDoctors_DoctorId",
                table: "CourseDoctors",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_AspNetUsers_ApplicationUserId",
                table: "Courses",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
