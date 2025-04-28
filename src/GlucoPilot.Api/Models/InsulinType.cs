namespace GlucoPilot.Api.Models
{
    public enum InsulinType
    {
        /// <summary>
        /// A fast acting insulin, usually taken with a meal.
        /// </summary>
        Bolus = 0,

        /// <summary>
        /// A long acting insulin, usually taken once or twice a day.
        /// </summary>
        Basal = 1,
    }
}
