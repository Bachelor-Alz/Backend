using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class InvitesUpdate : Migration
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

            migrationBuilder.DropIndex(
                name: "IX_Elder_CaregiverId",
                table: "Elder");

            migrationBuilder.DropIndex(
                name: "IX_Elder_CaregiverId1",
                table: "Elder");

            migrationBuilder.DropColumn(
                name: "CaregiverId",
                table: "Elder");

            migrationBuilder.DropColumn(
                name: "CaregiverId1",
                table: "Elder");

            migrationBuilder.CreateTable(
                name: "CaregiverElder",
                columns: table => new
                {
                    CaregiversId = table.Column<string>(type: "text", nullable: false),
                    EldersId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaregiverElder", x => new { x.CaregiversId, x.EldersId });
                    table.ForeignKey(
                        name: "FK_CaregiverElder_Caregiver_CaregiversId",
                        column: x => x.CaregiversId,
                        principalTable: "Caregiver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaregiverElder_Elder_EldersId",
                        column: x => x.EldersId,
                        principalTable: "Elder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaregiverElder1",
                columns: table => new
                {
                    InvitedCaregiversId = table.Column<string>(type: "text", nullable: false),
                    InvitesId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaregiverElder1", x => new { x.InvitedCaregiversId, x.InvitesId });
                    table.ForeignKey(
                        name: "FK_CaregiverElder1_Caregiver_InvitedCaregiversId",
                        column: x => x.InvitedCaregiversId,
                        principalTable: "Caregiver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaregiverElder1_Elder_InvitesId",
                        column: x => x.InvitesId,
                        principalTable: "Elder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaregiverElder_EldersId",
                table: "CaregiverElder",
                column: "EldersId");

            migrationBuilder.CreateIndex(
                name: "IX_CaregiverElder1_InvitesId",
                table: "CaregiverElder1",
                column: "InvitesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaregiverElder");

            migrationBuilder.DropTable(
                name: "CaregiverElder1");

            migrationBuilder.AddColumn<string>(
                name: "CaregiverId",
                table: "Elder",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaregiverId1",
                table: "Elder",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Elder_CaregiverId",
                table: "Elder",
                column: "CaregiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Elder_CaregiverId1",
                table: "Elder",
                column: "CaregiverId1");

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
