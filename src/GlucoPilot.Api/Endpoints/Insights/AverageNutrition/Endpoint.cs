using System;
using System.Linq;
using System.Threading;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GlucoPilot.Api.Endpoints.Insights.AverageNutrition;

internal static class Endpoint
{
    internal static Results<Ok<AverageNutritionResponse>, UnauthorizedHttpResult> Handle(
        [AsParameters] AverageNutritionRequest request,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IRepository<Treatment> repository,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetUserId();

        var to = request.To ?? DateTimeOffset.UtcNow;
        var from = request.From ?? to.AddDays(-7);

        // Query currently lacks the ability to treat ranges without any meals in as a zero,
        // so averages may be a little off... But if this defaults to a 24-hour range and goes back 
        // the last 7 days, it should suffice for now...
        var query = """
                    WITH MealCalories AS (
                        SELECT 
                            mi.MealId,
                            SUM(i.Calories) AS TotalCalories,
                            SUM(i.Carbs) AS TotalCarbs,
                            SUM(i.Protein) AS TotalProtein,
                            SUM(i.Fat) AS TotalFat
                        FROM 
                            meals_ingredients mi
                        JOIN 
                            ingredients i ON mi.IngredientId = i.Id
                        GROUP BY 
                            mi.MealId
                    ),
                    Intervals AS (
                        SELECT 
                            DATEADD(HOUR, DATEDIFF(HOUR, 0, [Created]), 0) AS hour_start,
                            [Id],
                            [UserId],
                            [Created],
                            [MealId]
                        FROM 
                            [treatments]
                        WHERE 
                            [MealId] is not NULL
                            AND [Created] BETWEEN {1} AND {2}
                            AND [UserId] = {3}
                    )

                    SELECT 
                        I.hour_start AS Time,
                        SUM(MC.TotalCalories) AS Calories,
                        SUM(MC.TotalCarbs) AS Carbs,
                        SUM(MC.TotalProtein) AS Protein,
                        SUM(MC.TotalFat) AS Fat
                    FROM 
                        Intervals I
                    JOIN 
                        MealCalories MC ON I.MealId = MC.MealId
                    GROUP BY 
                        I.hour_start
                    ORDER BY 
                        I.hour_start;
                    """;

        var nutirtion = repository.FromSqlRaw<Nutrition>
            (query, new FindOptions { IsAsNoTracking = true }, request.Range.Hours, request.From, request.To,
                userId)
            .AsEnumerable()
            .DefaultIfEmpty()
            .ToList();

        return TypedResults.Ok(new AverageNutritionResponse
        {
            Calories = nutirtion.Average(n => n?.Calories ?? 0),
            Carbs = nutirtion.Average(n => n?.Carbs ?? 0),
            Protein = nutirtion.Average(n => n?.Protein ?? 0),
            Fat = nutirtion.Average(n => n?.Fat ?? 0)
        });
    }

    private record Nutrition
    {
        public decimal Calories { get; init; }
        public decimal Carbs { get; init; }
        public decimal Protein { get; init; }
        public decimal Fat { get; init; }
        public DateTime Time { get; init; }
    }
}