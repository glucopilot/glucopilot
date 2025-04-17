using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlucoPilot.Data.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class NonNullUserIdInSensor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensor_users_UserId",
                table: "Sensor");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Sensor",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sensor_users_UserId",
                table: "Sensor",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensor_users_UserId",
                table: "Sensor");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Sensor",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensor_users_UserId",
                table: "Sensor",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id");
        }
    }
}
