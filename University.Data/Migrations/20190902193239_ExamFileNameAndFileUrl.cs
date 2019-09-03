namespace University.Data.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class ExamFileNameAndFileUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "ExamSubmissions",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "ExamSubmissions",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "ExamSubmissions");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "ExamSubmissions");
        }
    }
}
