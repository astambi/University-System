namespace University.Data.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class RemoveGradeUS : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "StudentCourse");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Certificates");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "StudentCourse",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "Certificates",
                nullable: false,
                defaultValue: 0);
        }
    }
}
