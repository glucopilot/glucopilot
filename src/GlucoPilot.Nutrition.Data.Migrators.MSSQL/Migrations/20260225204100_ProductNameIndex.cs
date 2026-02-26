using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlucoPilot.Nutrition.Data.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class ProductNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = """
                      CREATE FULLTEXT CATALOG FTCProducts AS DEFAULT;

                      CREATE FULLTEXT INDEX ON dbo.Products(ProductName)
                        KEY INDEX PK_Products ON FTCProducts
                        WITH STOPLIST = OFF, CHANGE_TRACKING AUTO;
                      """;
            migrationBuilder.Sql(sql, true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = """
                      DROP FULLTEXT INDEX on dbo.Products;
                      DROP FULLTEXT CATALOG FTCProducts;
                      """;
            migrationBuilder.Sql(sql, true);
        }
    }
}
