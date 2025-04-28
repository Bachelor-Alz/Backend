using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class ElderCaregiverRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Elder_Caregiver_CaregiverId",
                table: "Elder");

            migrationBuilder.DropForeignKey(
                name: "FK_Elder_Caregiver_CaregiverId1",
                table: "Elder");

            migrationBuilder.RenameColumn(
                name: "CaregiverId1",
                table: "Elder",
                newName: "InvitedCaregiverId");

            migrationBuilder.RenameIndex(
                name: "IX_Elder_CaregiverId1",
                table: "Elder",
                newName: "IX_Elder_InvitedCaregiverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Elder_Caregiver_CaregiverId",
                table: "Elder",
                column: "CaregiverId",
                principalTable: "Caregiver",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Elder_Caregiver_InvitedCaregiverId",
                table: "Elder",
                column: "InvitedCaregiverId",
                principalTable: "Caregiver",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Elder_Caregiver_CaregiverId",
                table: "Elder");

            migrationBuilder.DropForeignKey(
                name: "FK_Elder_Caregiver_InvitedCaregiverId",
                table: "Elder");

            migrationBuilder.RenameColumn(
                name: "InvitedCaregiverId",
                table: "Elder",
                newName: "CaregiverId1");

            migrationBuilder.RenameIndex(
                name: "IX_Elder_InvitedCaregiverId",
                table: "Elder",
                newName: "IX_Elder_CaregiverId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Elder_Caregiver_CaregiverId",
                table: "Elder",
                column: "CaregiverId",
                principalTable: "Caregiver",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Elder_Caregiver_CaregiverId1",
                table: "Elder",
                column: "CaregiverId1",
                principalTable: "Caregiver",
                principalColumn: "Id");
        }
    }
}
