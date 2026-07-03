using System;
using HomeControllerHUB.Infra.DatabaseContext;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeControllerHUB.Api.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260702201715_AddSensorReadingMessageId")]
    public partial class AddSensorReadingMessageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MessageId",
                table: "SensorReadings",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_SensorId_MessageId",
                table: "SensorReadings",
                columns: new[] { "SensorId", "MessageId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SensorReadings_SensorId_MessageId",
                table: "SensorReadings");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "SensorReadings");
        }
    }
}
