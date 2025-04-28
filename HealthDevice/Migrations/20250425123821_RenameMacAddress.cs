using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class RenameMacAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "MAX30102Data",
                newName: "MacAddress");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "GPSData",
                newName: "MacAddress");

            migrationBuilder.RenameColumn(
                name: "Arduino",
                table: "Elder",
                newName: "MacAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MacAddress",
                table: "MAX30102Data",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "MacAddress",
                table: "GPSData",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "MacAddress",
                table: "Elder",
                newName: "Arduino");
        }
    }
}
