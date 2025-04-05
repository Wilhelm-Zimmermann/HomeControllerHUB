using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeControllerHUB.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSensorStatusUpdateAndMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "Metadata",
                table: "SensorReadings",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SensorStatusUpdates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SensorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BatteryLevel = table.Column<double>(type: "double precision", nullable: true),
                    SignalStrength = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Metadata = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorStatusUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorStatusUpdates_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SensorStatusUpdates_SensorId",
                table: "SensorStatusUpdates",
                column: "SensorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorStatusUpdates");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "SensorReadings");
        }
    }
}
