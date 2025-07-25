using GlucoPilot.Nutrition.Data.Entities;
using GlucoPilot.Nutrition.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GlucoPilot.Nutrition.Endpoints.GetProduct;

internal static class Endpoint
{
    internal static async Task<Results<Ok<ProductResponse>, NotFound, UnauthorizedHttpResult>> HandleAsync(
        [FromRoute] string code,
        [FromServices] IRepository<Product> repository,
        CancellationToken cancellationToken)
    {
        var product = await repository.FindOneAsync(p => p.Code == code,
                new FindOptions() { IsAsNoTracking = true, IsIgnoreAutoIncludes = true }, cancellationToken)
            .ConfigureAwait(false);

        if (product is null)
        {
            return TypedResults.NotFound();
        }
        
        var response = new ProductResponse()
        {
            Id = product.Id,
            ProductType = product.ProductType,
            Quantity = product.Quantity,
            ProductQuantityUnit = product.ProductQuantityUnit,
            ProductName = product.ProductName,
            ProductQuantity = product.ProductQuantity,
            NutritionDataPer = product.NutritionDataPer,
            Nutriments = new NutrimentsResponse()
            {
                EnergyUnit = product.Nutriments?.EnergyUnit,
                FatUnit = product.Nutriments?.FatUnit,
                CarbohydratesUnit = product.Nutriments?.CarbohydratesUnit,
                EnergyKcalUnit = product.Nutriments?.EnergyKcalUnit,
                EnergyKcalValue = product.Nutriments?.EnergyKcalValue,
                EnergyValue = product.Nutriments?.EnergyValue,
                CarbohydratesValue = product.Nutriments?.CarbohydratesValue,
                Proteins = product.Nutriments?.Proteins,
                EnergyKcalValueComputed = product.Nutriments?.EnergyKcalValueComputed,
                ProteinsValue = product.Nutriments?.ProteinsValue,
                EnergyKcal = product.Nutriments?.EnergyKcal,
                ProteinsUnit = product.Nutriments?.ProteinsUnit,
                Carbohydrates = product.Nutriments?.Carbohydrates,
                Energy = product.Nutriments?.Energy,
                Fat = product.Nutriments?.Fat,
                FatValue = product.Nutriments?.FatValue,
            },
            NutritionDataPreparedPer = product.NutritionDataPreparedPer,
            Code = product.Code,
            ServingQuantity = product.ServingQuantity,
        };
        
        return TypedResults.Ok(response);
    }
}