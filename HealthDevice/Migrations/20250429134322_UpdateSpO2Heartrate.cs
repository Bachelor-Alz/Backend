using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSpO2Heartrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpO2",
                table: "MAX30102");

            migrationBuilder.RenameColumn(
                name: "Heartrate",
                table: "MAX30102",
                newName: "SpO2Id");

            migrationBuilder.AddColumn<int>(
                name: "HeartrateId",
                table: "MAX30102",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MAX30102_HeartrateId",
                table: "MAX30102",
                column: "HeartrateId");

            migrationBuilder.CreateIndex(
                name: "IX_MAX30102_SpO2Id",
                table: "MAX30102",
                column: "SpO2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MAX30102_Heartrate_HeartrateId",
                table: "MAX30102",
                column: "HeartrateId",
                principalTable: "Heartrate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MAX30102_SpO2_SpO2Id",
                table: "MAX30102",
                column: "SpO2Id",
                principalTable: "SpO2",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MAX30102_Heartrate_HeartrateId",
                table: "MAX30102");

            migrationBuilder.DropForeignKey(
                name: "FK_MAX30102_SpO2_SpO2Id",
                table: "MAX30102");

            migrationBuilder.DropIndex(
                name: "IX_MAX30102_HeartrateId",
                table: "MAX30102");

            migrationBuilder.DropIndex(
                name: "IX_MAX30102_SpO2Id",
                table: "MAX30102");

            migrationBuilder.DropColumn(
                name: "HeartrateId",
                table: "MAX30102");

            migrationBuilder.RenameColumn(
                name: "SpO2Id",
                table: "MAX30102",
                newName: "Heartrate");

            migrationBuilder.AddColumn<float>(
                name: "SpO2",
                table: "MAX30102",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
