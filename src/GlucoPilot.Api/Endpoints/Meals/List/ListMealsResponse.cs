using GlucoPilot.Api.Endpoints.Meals.GetMeal;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Meals.List
{
    public sealed record ListMealsResponse
    {
        public required int NumberOfPages { get; set; }
        public required ICollection<MealResponse> Meals { get; set; }
    }
}
