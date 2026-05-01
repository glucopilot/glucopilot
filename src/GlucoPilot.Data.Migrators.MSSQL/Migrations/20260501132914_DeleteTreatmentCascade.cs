using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlucoPilot.Data.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class DeleteTreatmentCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_treatment_ingredient_ingredients_IngredientId",
                table: "treatment_ingredient");

            migrationBuilder.DropForeignKey(
                name: "FK_treatment_ingredient_treatments_TreatmentId",
                table: "treatment_ingredient");

            migrationBuilder.DropForeignKey(
                name: "FK_treatment_meal_meals_MealId",
                table: "treatment_meal");

            migrationBuilder.DropForeignKey(
                name: "FK_treatment_meal_treatments_TreatmentId",
                table: "treatment_meal");

            migrationBuilder.AddForeignKey(
                name: "FK_treatment_ingredient_ingredients_IngredientId",
                table: "treatment_ingredient",
                column: "IngredientId",
                principalTable: "ingredients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_treatment_ingredient_treatments_TreatmentId",
                table: "treatment_ingredient",
                column: "TreatmentId",
                principalTable: "treatments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_treatment_meal_meals_MealId",
                table: "treatment_meal",
                column: "MealId",
                principalTable: "meals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_treatment_meal_treatments_TreatmentId",
                table: "treatment_meal",
                column: "TreatmentId",
                principalTable: "treatments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_treatment_ingredient_ingredients_IngredientId",
                table: "treatment_ingredient");

            migrationBuilder.DropForeignKey(
                name: "FK_treatment_ingredient_treatments_TreatmentId",
                table: "treatment_ingredient");

            migrationBuilder.DropForeignKey(
                name: "FK_treatment_meal_meals_MealId",
                table: "treatment_meal");

            migrationBuilder.DropForeignKey(
                name: "FK_treatment_meal_treatments_TreatmentId",
                table: "treatment_meal");

            migrationBuilder.AddForeignKey(
                name: "FK_treatment_ingredient_ingredients_IngredientId",
                table: "treatment_ingredient",
                column: "IngredientId",
                principalTable: "ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_treatment_ingredient_treatments_TreatmentId",
                table: "treatment_ingredient",
                column: "TreatmentId",
                principalTable: "treatments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_treatment_meal_meals_MealId",
                table: "treatment_meal",
                column: "MealId",
                principalTable: "meals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_treatment_meal_treatments_TreatmentId",
                table: "treatment_meal",
                column: "TreatmentId",
                principalTable: "treatments",
                principalColumn: "Id");
        }
    }
}
