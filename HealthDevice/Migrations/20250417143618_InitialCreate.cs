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
                name: "Caregiver",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Caregiver", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DashBoard",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HeartRate = table.Column<int>(type: "integer", nullable: false),
                    SpO2 = table.Column<float>(type: "real", nullable: false),
                    steps = table.Column<int>(type: "integer", nullable: false),
                    distance = table.Column<double>(type: "double precision", nullable: false),
                    allFall = table.Column<int>(type: "integer", nullable: false),
                    locationAdress = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashBoard", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Perimeter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Radius = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perimeter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Elder",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    dashBoardId = table.Column<int>(type: "integer", nullable: true),
                    LocationId = table.Column<int>(type: "integer", nullable: true),
                    PerimeterId = table.Column<int>(type: "integer", nullable: true),
                    Arduino = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    CaregiverId = table.Column<string>(type: "text", nullable: true),
                    CaregiverId1 = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Elder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Elder_Caregiver_CaregiverId",
                        column: x => x.CaregiverId,
                        principalTable: "Caregiver",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Elder_Caregiver_CaregiverId1",
                        column: x => x.CaregiverId1,
                        principalTable: "Caregiver",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Elder_DashBoard_dashBoardId",
                        column: x => x.dashBoardId,
                        principalTable: "DashBoard",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Elder_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Elder_Perimeter_PerimeterId",
                        column: x => x.PerimeterId,
                        principalTable: "Perimeter",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FallInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: true),
                    ElderId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FallInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FallInfo_Elder_ElderId",
                        column: x => x.ElderId,
                        principalTable: "Elder",
                        principalColumn: "Id");
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
                    Course = table.Column<float>(type: "real", nullable: false),
                    ElderId = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GPSData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GPSData_Elder_ElderId",
                        column: x => x.ElderId,
                        principalTable: "Elder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Heartrate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Maxrate = table.Column<int>(type: "integer", nullable: false),
                    Minrate = table.Column<int>(type: "integer", nullable: false),
                    Avgrate = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ElderId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heartrate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Heartrate_Elder_ElderId",
                        column: x => x.ElderId,
                        principalTable: "Elder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Kilometer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Distance = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ElderId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kilometer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kilometer_Elder_ElderId",
                        column: x => x.ElderId,
                        principalTable: "Elder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MAX30102Data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Heartrate = table.Column<int>(type: "integer", nullable: false),
                    SpO2 = table.Column<float>(type: "real", nullable: false),
                    ElderId = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MAX30102Data", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MAX30102Data_Elder_ElderId",
                        column: x => x.ElderId,
                        principalTable: "Elder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SpO2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SpO2 = table.Column<float>(type: "real", nullable: false),
                    MaxSpO2 = table.Column<float>(type: "real", nullable: false),
                    MinSpO2 = table.Column<float>(type: "real", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ElderId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpO2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpO2_Elder_ElderId",
                        column: x => x.ElderId,
                        principalTable: "Elder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StepsCount = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ElderId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Steps_Elder_ElderId",
                        column: x => x.ElderId,
                        principalTable: "Elder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Elder_CaregiverId",
                table: "Elder",
                column: "CaregiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Elder_CaregiverId1",
                table: "Elder",
                column: "CaregiverId1");

            migrationBuilder.CreateIndex(
                name: "IX_Elder_dashBoardId",
                table: "Elder",
                column: "dashBoardId");

            migrationBuilder.CreateIndex(
                name: "IX_Elder_LocationId",
                table: "Elder",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Elder_PerimeterId",
                table: "Elder",
                column: "PerimeterId");

            migrationBuilder.CreateIndex(
                name: "IX_FallInfo_ElderId",
                table: "FallInfo",
                column: "ElderId");

            migrationBuilder.CreateIndex(
                name: "IX_FallInfo_LocationId",
                table: "FallInfo",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_GPSData_ElderId",
                table: "GPSData",
                column: "ElderId");

            migrationBuilder.CreateIndex(
                name: "IX_Heartrate_ElderId",
                table: "Heartrate",
                column: "ElderId");

            migrationBuilder.CreateIndex(
                name: "IX_Kilometer_ElderId",
                table: "Kilometer",
                column: "ElderId");

            migrationBuilder.CreateIndex(
                name: "IX_MAX30102Data_ElderId",
                table: "MAX30102Data",
                column: "ElderId");

            migrationBuilder.CreateIndex(
                name: "IX_SpO2_ElderId",
                table: "SpO2",
                column: "ElderId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_ElderId",
                table: "Steps",
                column: "ElderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FallInfo");

            migrationBuilder.DropTable(
                name: "GPSData");

            migrationBuilder.DropTable(
                name: "Heartrate");

            migrationBuilder.DropTable(
                name: "Kilometer");

            migrationBuilder.DropTable(
                name: "MAX30102Data");

            migrationBuilder.DropTable(
                name: "SpO2");

            migrationBuilder.DropTable(
                name: "Steps");

            migrationBuilder.DropTable(
                name: "Elder");

            migrationBuilder.DropTable(
                name: "Caregiver");

            migrationBuilder.DropTable(
                name: "DashBoard");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "Perimeter");
        }
    }
}
