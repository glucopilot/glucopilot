namespace GlucoPilot.Nutrition.Endpoints.GetProduct;

public class ProductResponse
{
    public string Id { get; set; }

    public string? ProductType { get; set; }

    public string? Quantity { get; set; }

    public string? ProductQuantityUnit { get; set; }

    public string? ProductName { get; set; }

    public double? ProductQuantity { get; set; }

    public string? NutritionDataPer { get; set; }

    public NutrimentsResponse? Nutriments { get; set; }

    public string? NutritionDataPreparedPer { get; set; }

    public string? Code { get; set; }

    public double? ServingQuantity { get; set; }
}