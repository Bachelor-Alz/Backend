using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    latitude = table.Column<int>(type: "integer", nullable: false),
                    longitude = table.Column<int>(type: "integer", nullable: false),
                    altitude = table.Column<int>(type: "integer", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Max30102Datas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Red = table.Column<int>(type: "integer", nullable: false),
                    Infrared = table.Column<int>(type: "integer", nullable: false),
                    HeartRate = table.Column<float>(type: "real", nullable: true),
                    SpO2 = table.Column<float>(type: "real", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Max30102Datas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MPU6050Datas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccelerationX = table.Column<float>(type: "real", nullable: false),
                    AccelerationY = table.Column<float>(type: "real", nullable: false),
                    AccelerationZ = table.Column<float>(type: "real", nullable: false),
                    GyroscopeX = table.Column<float>(type: "real", nullable: false),
                    GyroscopeY = table.Column<float>(type: "real", nullable: false),
                    GyroscopeZ = table.Column<float>(type: "real", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    temperature = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MPU6050Datas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Neo_6mDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UtcTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Status = table.Column<char>(type: "character(1)", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    LatitudeDirection = table.Column<char>(type: "character(1)", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    LongitudeDirection = table.Column<char>(type: "character(1)", nullable: false),
                    SpeedKnots = table.Column<float>(type: "real", nullable: false),
                    Course = table.Column<float>(type: "real", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MagneticVariation = table.Column<float>(type: "real", nullable: true),
                    MagneticDirection = table.Column<char>(type: "character(1)", nullable: true),
                    Checksum = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Neo_6mDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "FallInfos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    locationid = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FallInfos", x => x.id);
                    table.ForeignKey(
                        name: "FK_FallInfos_Locations_locationid",
                        column: x => x.locationid,
                        principalTable: "Locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Elders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    locationsid = table.Column<int>(type: "integer", nullable: false),
                    Caregiverid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elders", x => x.id);
                    table.ForeignKey(
                        name: "FK_Elders_Locations_locationsid",
                        column: x => x.locationsid,
                        principalTable: "Locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Elders_Users_Caregiverid",
                        column: x => x.Caregiverid,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Heartrates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Rate = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Elderid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heartrates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Heartrates_Elders_Elderid",
                        column: x => x.Elderid,
                        principalTable: "Elders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Elders_Caregiverid",
                table: "Elders",
                column: "Caregiverid");

            migrationBuilder.CreateIndex(
                name: "IX_Elders_locationsid",
                table: "Elders",
                column: "locationsid");

            migrationBuilder.CreateIndex(
                name: "IX_FallInfos_locationid",
                table: "FallInfos",
                column: "locationid");

            migrationBuilder.CreateIndex(
                name: "IX_Heartrates_Elderid",
                table: "Heartrates",
                column: "Elderid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FallInfos");

            migrationBuilder.DropTable(
                name: "Heartrates");

            migrationBuilder.DropTable(
                name: "Max30102Datas");

            migrationBuilder.DropTable(
                name: "MPU6050Datas");

            migrationBuilder.DropTable(
                name: "Neo_6mDatas");

            migrationBuilder.DropTable(
                name: "Elders");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
