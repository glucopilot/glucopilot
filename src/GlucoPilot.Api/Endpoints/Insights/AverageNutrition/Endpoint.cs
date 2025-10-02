using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GlucoPilot.Api.Endpoints.Insights.AverageNutrition;

internal static class Endpoint
{
    internal static async Task<Results<Ok<AverageNutritionResponse>, ValidationProblem, UnauthorizedHttpResult>> HandleAsync(
        [AsParameters] AverageNutritionRequest request,
        [FromServices] IValidator<AverageNutritionRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Treatment> repository,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetUserId();

        var to = request.To ?? DateTimeOffset.UtcNow;
        var from = request.From ?? to.AddDays(-7);
        var range = request.Range ?? TimeSpan.FromDays(1);

        var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        // Query currently lacks the ability to treat ranges without any meals in as a zero,
        // so averages may be a little off... But if this defaults to a 24-hour range and goes back 
        // the last 7 days, it should suffice for now...
        var query = """
                    WITH MealNutrition AS (
                        SELECT 
                            tm.TreatmentId,
                            SUM(mi.Quantity * tm.Quantity * i.Calories) AS TotalCalories,
                            SUM(mi.Quantity * tm.Quantity * i.Carbs) AS TotalCarbs,
                            SUM(mi.Quantity * tm.Quantity * i.Protein) AS TotalProtein,
                            SUM(mi.Quantity * tm.Quantity * i.Fat) AS TotalFat
                        FROM 
                            treatment_meal tm
                        JOIN 
                            meals_ingredients mi ON tm.MealId = mi.MealId
                        JOIN 
                            ingredients i ON mi.IngredientId = i.Id
                        GROUP BY 
                            tm.TreatmentId
                    ),
                    IngredientNutrition AS (
                        SELECT 
                            ti.TreatmentId,
                            SUM(ti.Quantity * i.Calories) AS TotalCalories,
                            SUM(ti.Quantity * i.Carbs) AS TotalCarbs,
                            SUM(ti.Quantity * i.Protein) AS TotalProtein,
                            SUM(ti.Quantity * i.Fat) AS TotalFat
                        FROM 
                            treatment_ingredient ti
                        JOIN 
                            ingredients i ON ti.IngredientId = i.Id
                        GROUP BY 
                            ti.TreatmentId
                    ),
                    CombinedNutrition AS (
                        SELECT 
                            TreatmentId,
                            TotalCalories,
                            TotalCarbs,
                            TotalProtein,
                            TotalFat
                        FROM MealNutrition
                        UNION ALL
                        SELECT 
                            TreatmentId,
                            TotalCalories,
                            TotalCarbs,
                            TotalProtein,
                            TotalFat
                        FROM IngredientNutrition
                    ),
                    TreatmentNutrition AS (
                        SELECT 
                            TreatmentId,
                            SUM(TotalCalories) AS TotalCalories,
                            SUM(TotalCarbs) AS TotalCarbs,
                            SUM(TotalProtein) AS TotalProtein,
                            SUM(TotalFat) AS TotalFat
                        FROM CombinedNutrition
                        GROUP BY TreatmentId
                    ),
                    Intervals AS (
                        SELECT 
                            DATEADD(HOUR, DATEDIFF(HOUR, 0, [Created]), 0) AS hour_start,
                            [Id],
                            [UserId],
                            [Created]
                        FROM 
                            [treatments]
                        WHERE 
                            [Created] BETWEEN {1} AND {2}
                            AND [UserId] = {3}
                            AND ([Id] IN (SELECT DISTINCT TreatmentId FROM treatment_meal) 
                                 OR [Id] IN (SELECT DISTINCT TreatmentId FROM treatment_ingredient))
                    )

                    SELECT 
                        I.hour_start AS Time,
                        SUM(TN.TotalCalories) AS Calories,
                        SUM(TN.TotalCarbs) AS Carbs,
                        SUM(TN.TotalProtein) AS Protein,
                        SUM(TN.TotalFat) AS Fat
                    FROM 
                        Intervals I
                    JOIN
                        TreatmentNutrition TN ON I.Id = TN.TreatmentId
                    GROUP BY 
                        I.hour_start
                    ORDER BY 
                        I.hour_start;
                    """;

        var nutrition = repository.FromSqlRaw<AverageNutrition>
            (query, new FindOptions { IsAsNoTracking = true }, range.Hours, from, to,
                userId)
            .AsEnumerable()
            .DefaultIfEmpty()
            .ToList();

        return TypedResults.Ok(new AverageNutritionResponse
        {
            Calories = nutrition.Average(n => n?.Calories ?? 0),
            Carbs = nutrition.Average(n => n?.Carbs ?? 0),
            Protein = nutrition.Average(n => n?.Protein ?? 0),
            Fat = nutrition.Average(n => n?.Fat ?? 0)
        });
    }

    internal record AverageNutrition
    {
        public decimal Calories { get; init; }
        public decimal Carbs { get; init; }
        public decimal Protein { get; init; }
        public decimal Fat { get; init; }
        public DateTime Time { get; init; }
    }
}