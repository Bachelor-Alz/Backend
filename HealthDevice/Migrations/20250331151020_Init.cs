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
                name: "FallInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FallInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FallInfo_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Perimeter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    Radius = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perimeter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Perimeter_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Elder",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    PerimeterId = table.Column<int>(type: "integer", nullable: true),
                    Arduino = table.Column<string>(type: "text", nullable: true),
                    FallInfoId = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_Elder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Elder_Caregiver_CaregiverId",
                        column: x => x.CaregiverId,
                        principalTable: "Caregiver",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Elder_FallInfo_FallInfoId",
                        column: x => x.FallInfoId,
                        principalTable: "FallInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Elder_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Elder_Perimeter_PerimeterId",
                        column: x => x.PerimeterId,
                        principalTable: "Perimeter",
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
                    Address = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    Address = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                name: "IX_Elder_FallInfoId",
                table: "Elder",
                column: "FallInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Elder_LocationId",
                table: "Elder",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Elder_PerimeterId",
                table: "Elder",
                column: "PerimeterId");

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
                name: "IX_Perimeter_LocationId",
                table: "Perimeter",
                column: "LocationId");

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
                name: "FallInfo");

            migrationBuilder.DropTable(
                name: "Perimeter");

            migrationBuilder.DropTable(
                name: "Location");
        }
    }
}
