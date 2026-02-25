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
            var sql = Functions.ReadSql("CreateProductNameFullTextIndex");
            migrationBuilder.Sql(sql, true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = Functions.ReadSql("RevertProductNameFullTextIndex");
            migrationBuilder.Sql(sql, true);
        }
    }
}
