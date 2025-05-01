using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSpO2HeartrateAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "SpO2Id",
                table: "MAX30102",
                newName: "MinHeartrate");

            migrationBuilder.RenameColumn(
                name: "HeartrateId",
                table: "MAX30102",
                newName: "MaxHeartrate");

            migrationBuilder.AddColumn<int>(
                name: "AvgHeartrate",
                table: "MAX30102",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "AvgSpO2",
                table: "MAX30102",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "MaxSpO2",
                table: "MAX30102",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "MinSpO2",
                table: "MAX30102",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "SpO2",
                table: "MAX30102",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvgHeartrate",
                table: "MAX30102");

            migrationBuilder.DropColumn(
                name: "AvgSpO2",
                table: "MAX30102");

            migrationBuilder.DropColumn(
                name: "MaxSpO2",
                table: "MAX30102");

            migrationBuilder.DropColumn(
                name: "MinSpO2",
                table: "MAX30102");

            migrationBuilder.DropColumn(
                name: "SpO2",
                table: "MAX30102");

            migrationBuilder.RenameColumn(
                name: "MinHeartrate",
                table: "MAX30102",
                newName: "SpO2Id");

            migrationBuilder.RenameColumn(
                name: "MaxHeartrate",
                table: "MAX30102",
                newName: "HeartrateId");

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
    }
}
