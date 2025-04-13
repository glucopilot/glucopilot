using GlucoPilot.Api.Endpoints.Meals.GetMeal;
using GlucoPilot.Api.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Meals.List
{
    public sealed record ListMealsResponse : PagedResponse
    {
        public required ICollection<GetMealResponse> Meals { get; set; }
    }
}
