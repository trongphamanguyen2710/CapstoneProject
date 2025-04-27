using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrowsinessDetectionServer.Migrations
{
    /// <inheritdoc />
    public partial class DetectionNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SupervisorId = table.Column<long>(type: "bigint", nullable: false),
                    DetectionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationLogs_DetectionLogs_DetectionId",
                        column: x => x.DetectionId,
                        principalTable: "DetectionLogs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotificationLogs_Supervisors_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_DetectionId",
                table: "NotificationLogs",
                column: "DetectionId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_SupervisorId",
                table: "NotificationLogs",
                column: "SupervisorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationLogs");
        }
    }
}
