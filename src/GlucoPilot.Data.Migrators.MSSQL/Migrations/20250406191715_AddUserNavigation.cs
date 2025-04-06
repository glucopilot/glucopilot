using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlucoPilot.Data.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_treatments_UserId",
                table: "treatments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_readings_UserId",
                table: "readings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_meals_UserId",
                table: "meals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_insulin_UserId",
                table: "insulin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_injections_UserId",
                table: "injections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ingredients_UserId",
                table: "ingredients",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ingredients_users_UserId",
                table: "ingredients",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_injections_users_UserId",
                table: "injections",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_insulin_users_UserId",
                table: "insulin",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_meals_users_UserId",
                table: "meals",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_readings_users_UserId",
                table: "readings",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_treatments_users_UserId",
                table: "treatments",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ingredients_users_UserId",
                table: "ingredients");

            migrationBuilder.DropForeignKey(
                name: "FK_injections_users_UserId",
                table: "injections");

            migrationBuilder.DropForeignKey(
                name: "FK_insulin_users_UserId",
                table: "insulin");

            migrationBuilder.DropForeignKey(
                name: "FK_meals_users_UserId",
                table: "meals");

            migrationBuilder.DropForeignKey(
                name: "FK_readings_users_UserId",
                table: "readings");

            migrationBuilder.DropForeignKey(
                name: "FK_treatments_users_UserId",
                table: "treatments");

            migrationBuilder.DropIndex(
                name: "IX_treatments_UserId",
                table: "treatments");

            migrationBuilder.DropIndex(
                name: "IX_readings_UserId",
                table: "readings");

            migrationBuilder.DropIndex(
                name: "IX_meals_UserId",
                table: "meals");

            migrationBuilder.DropIndex(
                name: "IX_insulin_UserId",
                table: "insulin");

            migrationBuilder.DropIndex(
                name: "IX_injections_UserId",
                table: "injections");

            migrationBuilder.DropIndex(
                name: "IX_ingredients_UserId",
                table: "ingredients");
        }
    }
}
