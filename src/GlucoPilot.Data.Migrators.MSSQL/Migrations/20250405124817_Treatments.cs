using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlucoPilot.Data.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class Treatments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "insulin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<double>(type: "float", nullable: true),
                    Scale = table.Column<double>(type: "float", nullable: true),
                    PeakTime = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_insulin", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "injections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    InsulinId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Units = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_injections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_injections_insulin_InsulinId",
                        column: x => x.InsulinId,
                        principalTable: "insulin",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "treatments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReadingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MealId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InjectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_treatments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_treatments_injections_InjectionId",
                        column: x => x.InjectionId,
                        principalTable: "injections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_treatments_meals_MealId",
                        column: x => x.MealId,
                        principalTable: "meals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_treatments_readings_ReadingId",
                        column: x => x.ReadingId,
                        principalTable: "readings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_injections_InsulinId",
                table: "injections",
                column: "InsulinId");

            migrationBuilder.CreateIndex(
                name: "IX_treatments_InjectionId",
                table: "treatments",
                column: "InjectionId");

            migrationBuilder.CreateIndex(
                name: "IX_treatments_MealId",
                table: "treatments",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_treatments_ReadingId",
                table: "treatments",
                column: "ReadingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "treatments");

            migrationBuilder.DropTable(
                name: "injections");

            migrationBuilder.DropTable(
                name: "insulin");
        }
    }
}
