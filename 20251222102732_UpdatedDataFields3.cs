using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Students_Portal_App.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDataFields3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actions",
                table: "StudentsPortalInfos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Actions",
                table: "StudentsPortalInfos",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
