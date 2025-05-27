using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlucoPilot.Data.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class SmartPen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsulinId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Model = table.Column<int>(type: "int", nullable: false),
                    Serial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pens_insulin_InsulinId",
                        column: x => x.InsulinId,
                        principalTable: "insulin",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pens_InsulinId",
                table: "pens",
                column: "InsulinId");

            migrationBuilder.CreateIndex(
                name: "IX_pens_UserId",
                table: "pens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pens");
        }
    }
}
