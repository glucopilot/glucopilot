using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlucoPilot.Data.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AlarmRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetHigh",
                table: "users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetLow",
                table: "users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "alarm_rule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetValue = table.Column<int>(type: "int", nullable: false),
                    TargetDirection = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alarm_rule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_alarm_rule_users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alarm_rule_PatientId",
                table: "alarm_rule",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alarm_rule");

            migrationBuilder.DropColumn(
                name: "TargetHigh",
                table: "users");

            migrationBuilder.DropColumn(
                name: "TargetLow",
                table: "users");
        }
    }
}
