using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class CleanUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Elder_DashBoard_dashBoardId",
                table: "Elder");

            migrationBuilder.DropTable(
                name: "DashBoard");

            migrationBuilder.DropIndex(
                name: "IX_Elder_dashBoardId",
                table: "Elder");

            migrationBuilder.DropColumn(
                name: "dashBoardId",
                table: "Elder");

            migrationBuilder.RenameColumn(
                name: "outOfPerimeter",
                table: "Elder",
                newName: "OutOfPerimeter");

            migrationBuilder.RenameColumn(
                name: "longitude",
                table: "Elder",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "latitude",
                table: "Elder",
                newName: "Latitude");

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Steps",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "SpO2",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Perimeter",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "MAX30102",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Location",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Heartrate",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "GPSData",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "FallInfo",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Elder",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Elder",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Distance",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OutOfPerimeter",
                table: "Elder",
                newName: "outOfPerimeter");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "Elder",
                newName: "longitude");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "Elder",
                newName: "latitude");

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Steps",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "SpO2",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Perimeter",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(18)",
                oldMaxLength: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "MAX30102",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Location",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(18)",
                oldMaxLength: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Heartrate",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "GPSData",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "FallInfo",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Elder",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Elder",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(18)",
                oldMaxLength: 18,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dashBoardId",
                table: "Elder",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Distance",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "DashBoard",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HeartRate = table.Column<int>(type: "integer", nullable: false),
                    SpO2 = table.Column<float>(type: "real", nullable: false),
                    Steps = table.Column<int>(type: "integer", nullable: false),
                    allFall = table.Column<int>(type: "integer", nullable: false),
                    distance = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashBoard", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Elder_dashBoardId",
                table: "Elder",
                column: "dashBoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Elder_DashBoard_dashBoardId",
                table: "Elder",
                column: "dashBoardId",
                principalTable: "DashBoard",
                principalColumn: "Id");
        }
    }
}
