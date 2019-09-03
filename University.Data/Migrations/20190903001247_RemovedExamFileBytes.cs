namespace University.Data.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class RemovedExamFileBytes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) 
            => migrationBuilder.DropColumn(
                name: "FileSubmission",
                table: "ExamSubmissions");

        protected override void Down(MigrationBuilder migrationBuilder) 
            => migrationBuilder.AddColumn<byte[]>(
                name: "FileSubmission",
                table: "ExamSubmissions",
                maxLength: 2097152,
                nullable: true);
    }
}
