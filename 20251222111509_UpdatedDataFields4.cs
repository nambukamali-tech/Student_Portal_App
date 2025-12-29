using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Students_Portal_App.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDataFields4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegisterNumber",
                table: "StudentsPortalInfos",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegisterNumber",
                table: "StudentsPortalInfos");
        }
    }
}
