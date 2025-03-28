using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Caregivers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Caregivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    latitude = table.Column<int>(type: "integer", nullable: false),
                    longitude = table.Column<int>(type: "integer", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "FallInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    locationid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FallInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FallInfos_Locations_locationid",
                        column: x => x.locationid,
                        principalTable: "Locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Perimeter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    locationid = table.Column<int>(type: "integer", nullable: false),
                    radius = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perimeter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Perimeter_Locations_locationid",
                        column: x => x.locationid,
                        principalTable: "Locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Elders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    locationid = table.Column<int>(type: "integer", nullable: false),
                    perimeterId = table.Column<int>(type: "integer", nullable: false),
                    arduino = table.Column<string>(type: "text", nullable: true),
                    fallInfoId = table.Column<int>(type: "integer", nullable: true),
                    CaregiverId = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Elders_Caregivers_CaregiverId",
                        column: x => x.CaregiverId,
                        principalTable: "Caregivers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Elders_FallInfos_fallInfoId",
                        column: x => x.fallInfoId,
                        principalTable: "FallInfos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Elders_Locations_locationid",
                        column: x => x.locationid,
                        principalTable: "Locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Elders_Perimeter_perimeterId",
                        column: x => x.perimeterId,
                        principalTable: "Perimeter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GPS_Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Course = table.Column<float>(type: "real", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    ElderId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GPS_Data", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GPS_Data_Elders_ElderId",
                        column: x => x.ElderId,
                        principalTable: "Elders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Heartrates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaxRate = table.Column<int>(type: "integer", nullable: false),
                    MinRate = table.Column<int>(type: "integer", nullable: false),
                    AvgRate = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ElderId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heartrates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Heartrates_Elders_ElderId",
                        column: x => x.ElderId,
                        principalTable: "Elders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Max30102Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HeartRate = table.Column<int>(type: "integer", nullable: false),
                    SpO2 = table.Column<float>(type: "real", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    ElderId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Max30102Data", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Max30102Data_Elders_ElderId",
                        column: x => x.ElderId,
                        principalTable: "Elders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SpO2s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    spO2 = table.Column<float>(type: "real", nullable: false),
                    MaxSpO2 = table.Column<float>(type: "real", nullable: false),
                    MinSpO2 = table.Column<float>(type: "real", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ElderId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpO2s", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpO2s_Elders_ElderId",
                        column: x => x.ElderId,
                        principalTable: "Elders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Elders_CaregiverId",
                table: "Elders",
                column: "CaregiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Elders_fallInfoId",
                table: "Elders",
                column: "fallInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Elders_locationid",
                table: "Elders",
                column: "locationid");

            migrationBuilder.CreateIndex(
                name: "IX_Elders_perimeterId",
                table: "Elders",
                column: "perimeterId");

            migrationBuilder.CreateIndex(
                name: "IX_FallInfos_locationid",
                table: "FallInfos",
                column: "locationid");

            migrationBuilder.CreateIndex(
                name: "IX_GPS_Data_ElderId",
                table: "GPS_Data",
                column: "ElderId");

            migrationBuilder.CreateIndex(
                name: "IX_Heartrates_ElderId",
                table: "Heartrates",
                column: "ElderId");

            migrationBuilder.CreateIndex(
                name: "IX_Max30102Data_ElderId",
                table: "Max30102Data",
                column: "ElderId");

            migrationBuilder.CreateIndex(
                name: "IX_Perimeter_locationid",
                table: "Perimeter",
                column: "locationid");

            migrationBuilder.CreateIndex(
                name: "IX_SpO2s_ElderId",
                table: "SpO2s",
                column: "ElderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GPS_Data");

            migrationBuilder.DropTable(
                name: "Heartrates");

            migrationBuilder.DropTable(
                name: "Max30102Data");

            migrationBuilder.DropTable(
                name: "SpO2s");

            migrationBuilder.DropTable(
                name: "Elders");

            migrationBuilder.DropTable(
                name: "Caregivers");

            migrationBuilder.DropTable(
                name: "FallInfos");

            migrationBuilder.DropTable(
                name: "Perimeter");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
