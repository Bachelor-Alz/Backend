using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class NoBigTableSensor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensor_Arduino_MacAddress",
                table: "Sensor");

            migrationBuilder.DropForeignKey(
                name: "FK_Sensor_Location_LocationId",
                table: "Sensor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sensor",
                table: "Sensor");

            migrationBuilder.DropIndex(
                name: "IX_Sensor_LocationId",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "AvgSpO2",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "Avgrate",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "LastSpO2",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "Lastrate",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "MaxSpO2",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "Maxrate",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "MinSpO2",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "Minrate",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "StepsCount",
                table: "Sensor");

            migrationBuilder.RenameTable(
                name: "Sensor",
                newName: "Sensors");

            migrationBuilder.RenameIndex(
                name: "IX_Sensor_MacAddress",
                table: "Sensors",
                newName: "IX_Sensors_MacAddress");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sensors",
                table: "Sensors",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DistanceInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Distance = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistanceInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistanceInfo_Sensors_Id",
                        column: x => x.Id,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FallInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FallInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FallInfo_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FallInfo_Sensors_Id",
                        column: x => x.Id,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GPSData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GPSData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GPSData_Sensors_Id",
                        column: x => x.Id,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Heartrates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Maxrate = table.Column<int>(type: "integer", nullable: false),
                    Minrate = table.Column<int>(type: "integer", nullable: false),
                    Avgrate = table.Column<int>(type: "integer", nullable: false),
                    Lastrate = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heartrates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Heartrates_Sensors_Id",
                        column: x => x.Id,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpO2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    AvgSpO2 = table.Column<float>(type: "real", nullable: false),
                    MaxSpO2 = table.Column<float>(type: "real", nullable: false),
                    MinSpO2 = table.Column<float>(type: "real", nullable: false),
                    LastSpO2 = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpO2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpO2_Sensors_Id",
                        column: x => x.Id,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    StepsCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Steps_Sensors_Id",
                        column: x => x.Id,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FallInfo_LocationId",
                table: "FallInfo",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_Arduino_MacAddress",
                table: "Sensors",
                column: "MacAddress",
                principalTable: "Arduino",
                principalColumn: "MacAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_Arduino_MacAddress",
                table: "Sensors");

            migrationBuilder.DropTable(
                name: "DistanceInfo");

            migrationBuilder.DropTable(
                name: "FallInfo");

            migrationBuilder.DropTable(
                name: "GPSData");

            migrationBuilder.DropTable(
                name: "Heartrates");

            migrationBuilder.DropTable(
                name: "SpO2");

            migrationBuilder.DropTable(
                name: "Steps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sensors",
                table: "Sensors");

            migrationBuilder.RenameTable(
                name: "Sensors",
                newName: "Sensor");

            migrationBuilder.RenameIndex(
                name: "IX_Sensors_MacAddress",
                table: "Sensor",
                newName: "IX_Sensor_MacAddress");

            migrationBuilder.AddColumn<float>(
                name: "AvgSpO2",
                table: "Sensor",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Avgrate",
                table: "Sensor",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Sensor",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "Distance",
                table: "Sensor",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LastSpO2",
                table: "Sensor",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Lastrate",
                table: "Sensor",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Sensor",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Sensor",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Sensor",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "MaxSpO2",
                table: "Sensor",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Maxrate",
                table: "Sensor",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "MinSpO2",
                table: "Sensor",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Minrate",
                table: "Sensor",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StepsCount",
                table: "Sensor",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sensor",
                table: "Sensor",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Sensor_LocationId",
                table: "Sensor",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensor_Arduino_MacAddress",
                table: "Sensor",
                column: "MacAddress",
                principalTable: "Arduino",
                principalColumn: "MacAddress");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensor_Location_LocationId",
                table: "Sensor",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id");
        }
    }
}
