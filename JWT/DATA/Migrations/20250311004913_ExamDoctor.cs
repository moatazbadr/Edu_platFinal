using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JWT.Migrations
{
    /// <inheritdoc />
    public partial class ExamDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Exams_DoctorId",
                table: "Exams",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Doctors_DoctorId",
                table: "Exams",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Doctors_DoctorId",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_DoctorId",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Exams");
        }
    }
}
