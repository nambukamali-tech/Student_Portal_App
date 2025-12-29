using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Students_Portal_App.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDataFields1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPresent_New",
                table: "StudentsPortalInfos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPresent_New",
                table: "StudentsPortalInfos",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
