using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrowsinessDetectionServer.Migrations
{
    /// <inheritdoc />
    public partial class SessionDetectionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DetectionLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<long>(type: "bigint", nullable: false),
                    SessionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetectionLogs_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DetectionLogs_MonitorSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "MonitorSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetectionLogs_DriverId",
                table: "DetectionLogs",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DetectionLogs_SessionId",
                table: "DetectionLogs",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetectionLogs");
        }
    }
}
