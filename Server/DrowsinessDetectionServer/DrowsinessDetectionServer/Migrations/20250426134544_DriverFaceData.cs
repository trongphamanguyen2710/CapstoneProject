using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrowsinessDetectionServer.Migrations
{
    /// <inheritdoc />
    public partial class DriverFaceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FaceDatas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data1 = table.Column<int>(type: "int", nullable: false),
                    Data2 = table.Column<int>(type: "int", nullable: false),
                    Data3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DriverId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaceDatas_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaceDatas_DriverId",
                table: "FaceDatas",
                column: "DriverId",
                unique: true,
                filter: "[DriverId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaceDatas");
        }
    }
}
