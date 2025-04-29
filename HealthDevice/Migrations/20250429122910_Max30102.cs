using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class Max30102 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MAX30102Data",
                table: "MAX30102Data");

            migrationBuilder.RenameTable(
                name: "MAX30102Data",
                newName: "MAX30102");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MAX30102",
                table: "MAX30102",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MAX30102",
                table: "MAX30102");

            migrationBuilder.RenameTable(
                name: "MAX30102",
                newName: "MAX30102Data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MAX30102Data",
                table: "MAX30102Data",
                column: "Id");
        }
    }
}
