using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlucoPilot.Data.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class M2MTreatmentMealsIngredients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "treatments_ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_treatments_ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_treatments_ingredients_ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_treatments_ingredients_treatments_TreatmentId",
                        column: x => x.TreatmentId,
                        principalTable: "treatments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "treatments_meals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MealId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_treatments_meals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_treatments_meals_meals_MealId",
                        column: x => x.MealId,
                        principalTable: "meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_treatments_meals_treatments_TreatmentId",
                        column: x => x.TreatmentId,
                        principalTable: "treatments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_treatments_ingredients_IngredientId",
                table: "treatments_ingredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_treatments_ingredients_TreatmentId",
                table: "treatments_ingredients",
                column: "TreatmentId");

            migrationBuilder.CreateIndex(
                name: "IX_treatments_meals_MealId",
                table: "treatments_meals",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_treatments_meals_TreatmentId",
                table: "treatments_meals",
                column: "TreatmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "treatments_ingredients");

            migrationBuilder.DropTable(
                name: "treatments_meals");
        }
    }
}
