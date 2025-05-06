using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class CurrentHRSPO2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SpO2",
                table: "MAX30102",
                newName: "LastSpO2");

            migrationBuilder.AddColumn<int>(
                name: "LastHeartrate",
                table: "MAX30102",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastHeartrate",
                table: "MAX30102");

            migrationBuilder.RenameColumn(
                name: "LastSpO2",
                table: "MAX30102",
                newName: "SpO2");
        }
    }
}
