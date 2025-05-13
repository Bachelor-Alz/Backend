using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HealthDevice.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMax30102Data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MAX30102");

            migrationBuilder.AddColumn<float>(
                name: "LastSpO2",
                table: "SpO2",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "Lastrate",
                table: "Heartrate",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSpO2",
                table: "SpO2");

            migrationBuilder.DropColumn(
                name: "Lastrate",
                table: "Heartrate");

            migrationBuilder.CreateTable(
                name: "MAX30102",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AvgHeartrate = table.Column<int>(type: "integer", nullable: false),
                    AvgSpO2 = table.Column<float>(type: "real", nullable: false),
                    LastHeartrate = table.Column<int>(type: "integer", nullable: false),
                    LastSpO2 = table.Column<float>(type: "real", nullable: false),
                    MacAddress = table.Column<string>(type: "text", nullable: false),
                    MaxHeartrate = table.Column<int>(type: "integer", nullable: false),
                    MaxSpO2 = table.Column<float>(type: "real", nullable: false),
                    MinHeartrate = table.Column<int>(type: "integer", nullable: false),
                    MinSpO2 = table.Column<float>(type: "real", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MAX30102", x => x.Id);
                });
        }
    }
}
