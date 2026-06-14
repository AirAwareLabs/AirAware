using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirAware.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Readings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Pm25 = table.Column<double>(type: "double precision", nullable: false),
                    Pm10 = table.Column<double>(type: "double precision", nullable: true),
                    RawPayload = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Readings_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AqiRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadingId = table.Column<Guid>(type: "uuid", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AqiValue = table.Column<int>(type: "integer", nullable: false),
                    Pm25Aqi = table.Column<int>(type: "integer", nullable: true),
                    Pm10Aqi = table.Column<int>(type: "integer", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Pm25Category = table.Column<string>(type: "text", nullable: true),
                    Pm10Category = table.Column<string>(type: "text", nullable: true),
                    ComputedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AqiRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AqiRecords_Readings_ReadingId",
                        column: x => x.ReadingId,
                        principalTable: "Readings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AqiRecords_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AqiRecords_ReadingId",
                table: "AqiRecords",
                column: "ReadingId");

            migrationBuilder.CreateIndex(
                name: "IX_AqiRecords_StationId",
                table: "AqiRecords",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Readings_StationId",
                table: "Readings",
                column: "StationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AqiRecords");

            migrationBuilder.DropTable(
                name: "Readings");

            migrationBuilder.DropTable(
                name: "Stations");
        }
    }
}
