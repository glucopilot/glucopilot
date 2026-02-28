using GlucoPilot.Nutrition.Data.Entities;
using GlucoPilot.Nutrition.Data.Repository;
using GlucoPilot.Nutrition.Endpoints.GetProduct;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Nutrition.Endpoints.SearchProduct;

internal static class Endpoint
{
    private const int DefaultMaxResults = 50;

    internal static async Task<Results<Ok<IEnumerable<ProductResponse>>, UnauthorizedHttpResult>> HandleAsync(
        [FromRoute] string term,
        [FromQuery] int? max,
        [FromServices] IRepository<Product> repository,
        CancellationToken cancellationToken)
    {
        var maxResults = max is null or <= 0 ? DefaultMaxResults :  max.Value;
        var searchTerm = term.ToLowerInvariant();

        var products =
            await repository
                .Find(
                    p => p.ProductName != null && EF.Functions.Contains(p.ProductName, $"\"{searchTerm.Replace("\"", "")}\""),
                    new FindOptions { IsAsNoTracking = true, IsIgnoreAutoIncludes = true, })
                .Take(maxResults)
                .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        var response = products.Select(product => new ProductResponse()
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
        });

        return TypedResults.Ok(response);
    }
}