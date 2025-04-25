using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class DashBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "locationAdress",
                table: "DashBoard");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "locationAdress",
                table: "DashBoard",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
