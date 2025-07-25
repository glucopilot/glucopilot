using System.Text.Json;
using System.Text.Json.Serialization;
using EFCore.BulkExtensions;
using GlucoPilot.Nutrition.Data;
using GlucoPilot.Nutrition.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(configuration => configuration.AddUserSecrets<Program>())
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<GlucoPilotNutritionDbContext>((_, options) =>
        {
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection"),
                e => e.MigrationsAssembly("GlucoPilot.Nutrition.Data.Migrators.MSSQL"));
        });
    })
    .Build();

await host.Services.GetRequiredService<GlucoPilotNutritionDbContext>().Database.MigrateAsync();

const string path = "/Volumes/Data/Test/Open Food Facts Products.jsonl";


using var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<GlucoPilotNutritionDbContext>();

// var test = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == "3270491000590") ?? throw new Exception("Product not found");
// Console.WriteLine("FOUND");

var products = new List<Product>();
long lineNo = 0;
using var reader = new StreamReader(path);
while (reader.Peek() >= 0)
{
    var line = reader.ReadLine();
    lineNo++;
    if (string.IsNullOrWhiteSpace(line)) continue;

    var product = JsonConvert.DeserializeObject<QuickType.Untitled>(line);

    if (product?.Nutriments == null) continue;

    products.Add(new GlucoPilot.Nutrition.Data.Entities.Product
    {
        Id = Guid.NewGuid().ToString(),
        ProductType = product.ProductType,
        Quantity = product.Quantity,
        ProductQuantityUnit = product.ProductQuantityUnit,
        ProductName = product.ProductName,
        ProductQuantity = product.ProductQuantity,
        NutritionDataPer = product.NutritionDataPer,
        Nutriments = new GlucoPilot.Nutrition.Data.Entities.Nutriments
        {
            EnergyUnit = product.Nutriments.EnergyUnit,
            FatUnit = product.Nutriments.FatUnit,
            CarbohydratesUnit = product.Nutriments.CarbohydratesUnit,
            EnergyKcalUnit = product.Nutriments.EnergyKcalUnit,
            EnergyKcalValue = product.Nutriments.EnergyKcalValue,
            EnergyValue = product.Nutriments.EnergyValue,
            CarbohydratesValue = product.Nutriments.CarbohydratesValue,
            Proteins = product.Nutriments.Proteins,
            EnergyKcalValueComputed = product.Nutriments.EnergyKcalValueComputed,
            ProteinsValue = product.Nutriments.ProteinsValue,
            EnergyKcal = product.Nutriments.EnergyKcal,
            ProteinsUnit = product.Nutriments.ProteinsUnit,
            Carbohydrates = product.Nutriments.Carbohydrates,
            Energy = product.Nutriments.Energy,
            Fat = product.Nutriments.Fat,
            FatValue = product.Nutriments.FatValue
        },
        NutritionDataPreparedPer = product.NutritionDataPreparedPer,
        Code = product.Code,
        ServingQuantity = product.ServingQuantity
    });

    if (products.Count >= 10000)
    {
        await dbContext.BulkInsertAsync(products);
        products.Clear();
    }
}

if (products.Count > 0)
{
    await dbContext.BulkInsertAsync(products);
    products.Clear();
}

Console.WriteLine($"Items: {lineNo}");


namespace QuickType
{
    public partial class Untitled
    {
        [JsonProperty("_id")] public string Id { get; set; }

        [JsonProperty("product_type")]
        public string ProductType { get; set; }

        [JsonProperty("quantity")] public string Quantity { get; set; }

        [JsonProperty("product_quantity_unit")]
        public string ProductQuantityUnit { get; set; }

        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        [JsonProperty("product_quantity")]
        public double? ProductQuantity { get; set; }

        [JsonProperty("nutrition_data_per")]
        public string NutritionDataPer { get; set; }

        [JsonProperty("nutriments")] public Nutriments Nutriments { get; set; }

        [JsonProperty("nutrition_data_prepared_per")]
        public string NutritionDataPreparedPer { get; set; }

        [JsonProperty("code")] public string Code { get; set; }

        [JsonProperty("serving_quantity")]
        public double? ServingQuantity { get; set; }
    }

    public partial class Nutriments
    {
        [JsonProperty("potassium_unit")]
        public string PotassiumUnit { get; set; }

        [JsonProperty("energy_unit")] public string EnergyUnit { get; set; }

        [JsonProperty("fat_unit")] public string FatUnit { get; set; }

        [JsonProperty("carbohydrates_unit")]
        public string CarbohydratesUnit { get; set; }

        [JsonProperty("energy-kcal_unit")]
        public string EnergyKcalUnit { get; set; }

        [JsonProperty("energy-kcal_value")]
        public double? EnergyKcalValue { get; set; }

        [JsonProperty("energy_value")]
        public double? EnergyValue { get; set; }

        [JsonProperty("carbohydrates_value")]
        public double? CarbohydratesValue { get; set; }

        [JsonProperty("proteins")] public float Proteins { get; set; }

        [JsonProperty("energy-kcal_value_computed")]
        public double? EnergyKcalValueComputed { get; set; }

        [JsonProperty("proteins_value")]
        public float ProteinsValue { get; set; }

        [JsonProperty("energy-kcal")]
        public double? EnergyKcal { get; set; }

        [JsonProperty("proteins_unit")]
        public string ProteinsUnit { get; set; }

        [JsonProperty("carbohydrates")]
        public double? Carbohydrates { get; set; }

        [JsonProperty("energy")] public float Energy { get; set; }

        [JsonProperty("fat")] public float Fat { get; set; }

        [JsonProperty("fat_value")] public float FatValue { get; set; }
    }
}