namespace University.Data.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class GradeBG : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GradeBg",
                table: "StudentCourse",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GradeBg",
                table: "Certificates",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GradeBg",
                table: "StudentCourse");

            migrationBuilder.DropColumn(
                name: "GradeBg",
                table: "Certificates");
        }
    }
}
