using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlucoPilot.Nutrition.Data.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductQuantityUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductQuantity = table.Column<double>(type: "float", nullable: true),
                    NutritionDataPer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nutriments_EnergyUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nutriments_FatUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nutriments_CarbohydratesUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nutriments_EnergyKcalUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nutriments_EnergyKcalValue = table.Column<double>(type: "float", nullable: true),
                    Nutriments_EnergyValue = table.Column<double>(type: "float", nullable: true),
                    Nutriments_CarbohydratesValue = table.Column<double>(type: "float", nullable: true),
                    Nutriments_Proteins = table.Column<float>(type: "real", nullable: true),
                    Nutriments_EnergyKcalValueComputed = table.Column<double>(type: "float", nullable: true),
                    Nutriments_ProteinsValue = table.Column<float>(type: "real", nullable: true),
                    Nutriments_EnergyKcal = table.Column<double>(type: "float", nullable: true),
                    Nutriments_ProteinsUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nutriments_Carbohydrates = table.Column<double>(type: "float", nullable: true),
                    Nutriments_Energy = table.Column<float>(type: "real", nullable: true),
                    Nutriments_Fat = table.Column<float>(type: "real", nullable: true),
                    Nutriments_FatValue = table.Column<float>(type: "real", nullable: true),
                    NutritionDataPreparedPer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServingQuantity = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
