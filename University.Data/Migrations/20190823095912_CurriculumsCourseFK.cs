namespace University.Data.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class CurriculumsCourseFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurriculumCourse_Curriculums_CourseId",
                table: "CurriculumCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_CurriculumCourse_Courses_CurriculumId",
                table: "CurriculumCourse");

            migrationBuilder.AddForeignKey(
                name: "FK_CurriculumCourse_Courses_CourseId",
                table: "CurriculumCourse",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CurriculumCourse_Curriculums_CurriculumId",
                table: "CurriculumCourse",
                column: "CurriculumId",
                principalTable: "Curriculums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurriculumCourse_Courses_CourseId",
                table: "CurriculumCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_CurriculumCourse_Curriculums_CurriculumId",
                table: "CurriculumCourse");

            migrationBuilder.AddForeignKey(
                name: "FK_CurriculumCourse_Curriculums_CourseId",
                table: "CurriculumCourse",
                column: "CourseId",
                principalTable: "Curriculums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CurriculumCourse_Courses_CurriculumId",
                table: "CurriculumCourse",
                column: "CurriculumId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
