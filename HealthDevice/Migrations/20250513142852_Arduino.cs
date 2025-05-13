using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class Arduino : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arduino",
                columns: table => new
                {
                    MacAddress = table.Column<string>(type: "text", nullable: false),
                    isClaim = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arduino", x => x.MacAddress);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Elder_MacAddress",
                table: "Elder",
                column: "MacAddress",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Elder_Arduino_MacAddress",
                table: "Elder",
                column: "MacAddress",
                principalTable: "Arduino",
                principalColumn: "MacAddress",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Elder_Arduino_MacAddress",
                table: "Elder");

            migrationBuilder.DropTable(
                name: "Arduino");

            migrationBuilder.DropIndex(
                name: "IX_Elder_MacAddress",
                table: "Elder");
        }
    }
}
