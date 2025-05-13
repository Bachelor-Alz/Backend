using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class ArduinoMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Distance");

            migrationBuilder.DropTable(
                name: "FallInfo");

            migrationBuilder.DropTable(
                name: "GPSData");

            migrationBuilder.DropTable(
                name: "Heartrate");

            migrationBuilder.DropTable(
                name: "SpO2");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Steps",
                table: "Steps");

            migrationBuilder.RenameTable(
                name: "Steps",
                newName: "Sensor");

            migrationBuilder.AlterColumn<int>(
                name: "StepsCount",
                table: "Sensor",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Sensor",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sensor",
                table: "Sensor",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Sensor_LocationId",
                table: "Sensor",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensor_MacAddress",
                table: "Sensor",
                column: "MacAddress");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "IX_Sensor_MacAddress",
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

            migrationBuilder.RenameTable(
                name: "Sensor",
                newName: "Steps");

            migrationBuilder.AlterColumn<int>(
                name: "StepsCount",
                table: "Steps",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MacAddress",
                table: "Steps",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Steps",
                table: "Steps",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Distance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Distance = table.Column<float>(type: "real", nullable: false),
                    MacAddress = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FallInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocationId = table.Column<int>(type: "integer", nullable: true),
                    MacAddress = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FallInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FallInfo_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GPSData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    MacAddress = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GPSData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Heartrate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Avgrate = table.Column<int>(type: "integer", nullable: false),
                    Lastrate = table.Column<int>(type: "integer", nullable: false),
                    MacAddress = table.Column<string>(type: "text", nullable: false),
                    Maxrate = table.Column<int>(type: "integer", nullable: false),
                    Minrate = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heartrate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpO2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AvgSpO2 = table.Column<float>(type: "real", nullable: false),
                    LastSpO2 = table.Column<float>(type: "real", nullable: false),
                    MacAddress = table.Column<string>(type: "text", nullable: false),
                    MaxSpO2 = table.Column<float>(type: "real", nullable: false),
                    MinSpO2 = table.Column<float>(type: "real", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpO2", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FallInfo_LocationId",
                table: "FallInfo",
                column: "LocationId");
        }
    }
}
