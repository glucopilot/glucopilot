using System;
using System.Collections;
using System.Collections.Generic;

namespace GlucoPilot.Api.Models
{
    public sealed record ListMealsResponse
    {
        public required int NumberOfPages { get; set; }
        public required ICollection<MealResponse> Meals { get; set; }
    }
}
