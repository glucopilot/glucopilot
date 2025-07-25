using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlucoPilot.Data.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientsAndMealsToTreatment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_treatments_meals_MealId",
                table: "treatments");

            migrationBuilder.DropIndex(
                name: "IX_treatments_MealId",
                table: "treatments");

            migrationBuilder.DropColumn(
                name: "MealId",
                table: "treatments");

            migrationBuilder.CreateTable(
                name: "treatment_ingredient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_treatment_ingredient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_treatment_ingredient_ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_treatment_ingredient_treatments_TreatmentId",
                        column: x => x.TreatmentId,
                        principalTable: "treatments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "treatment_meal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MealId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_treatment_meal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_treatment_meal_meals_MealId",
                        column: x => x.MealId,
                        principalTable: "meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_treatment_meal_treatments_TreatmentId",
                        column: x => x.TreatmentId,
                        principalTable: "treatments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_treatment_ingredient_IngredientId",
                table: "treatment_ingredient",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_treatment_ingredient_TreatmentId",
                table: "treatment_ingredient",
                column: "TreatmentId");

            migrationBuilder.CreateIndex(
                name: "IX_treatment_meal_MealId",
                table: "treatment_meal",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_treatment_meal_TreatmentId",
                table: "treatment_meal",
                column: "TreatmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "treatment_ingredient");

            migrationBuilder.DropTable(
                name: "treatment_meal");

            migrationBuilder.AddColumn<Guid>(
                name: "MealId",
                table: "treatments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_treatments_MealId",
                table: "treatments",
                column: "MealId");

            migrationBuilder.AddForeignKey(
                name: "FK_treatments_meals_MealId",
                table: "treatments",
                column: "MealId",
                principalTable: "meals",
                principalColumn: "Id");
        }
    }
}
