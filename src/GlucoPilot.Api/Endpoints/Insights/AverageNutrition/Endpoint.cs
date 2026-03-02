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

        var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

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
                    DailyNutrition AS (
                        SELECT 
                            CAST(t.[Created] AS DATE) AS day_date,
                            SUM(TN.TotalCalories) AS DayCalories,
                            SUM(TN.TotalCarbs) AS DayCarbs,
                            SUM(TN.TotalProtein) AS DayProtein,
                            SUM(TN.TotalFat) AS DayFat
                        FROM 
                            [treatments] t
                        JOIN
                            TreatmentNutrition TN ON t.Id = TN.TreatmentId
                        WHERE 
                            t.[Created] BETWEEN {0} AND {1}
                            AND t.[UserId] = {2}
                        GROUP BY 
                            CAST(t.[Created] AS DATE)
                    )

                    SELECT 
                        COALESCE(AVG(DayCalories), 0) AS Calories,
                        COALESCE(AVG(DayCarbs), 0) AS Carbs,
                        COALESCE(AVG(DayProtein), 0) AS Protein,
                        COALESCE(AVG(DayFat), 0) AS Fat
                    FROM 
                        DailyNutrition;
                    """;

        var nutrition = repository.FromSqlRaw<AverageNutrition>
            (query, new FindOptions { IsAsNoTracking = true }, from, to, userId)
            .AsEnumerable()
            .FirstOrDefault();

        return TypedResults.Ok(new AverageNutritionResponse
        {
            Calories = nutrition?.Calories ?? 0,
            Carbs = nutrition?.Carbs ?? 0,
            Protein = nutrition?.Protein ?? 0,
            Fat = nutrition?.Fat ?? 0
        });
    }

    internal record AverageNutrition
    {
        public decimal Calories { get; init; }
        public decimal Carbs { get; init; }
        public decimal Protein { get; init; }
        public decimal Fat { get; init; }
    }
}