using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Ingredients.NewIngredient;
using GlucoPilot.Data.Enums;
using NUnit.Framework;
using static GlucoPilot.Api.Endpoints.Ingredients.NewIngredient.NewIngredientRequest;

namespace GlucoPilot.Api.Tests.Validators
{
    [TestFixture]
    public class NewIngredientRequestValidatorTests
    {
        private NewIngredientRequestValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new NewIngredientRequestValidator();
        }

        [Test]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            var model = new NewIngredientRequest
            {
                Name = string.Empty,
                Carbs = 10,
                Protein = 5,
                Fat = 2,
                Calories = 100,
                Uom = UnitOfMeasurement.Grams
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Test]
        public void Should_Have_Error_When_Carbs_Is_Negative()
        {
            var model = new NewIngredientRequest
            {
                Name = "Test Ingredient",
                Carbs = -1,
                Protein = 5,
                Fat = 2,
                Calories = 100,
                Uom = UnitOfMeasurement.Grams
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Carbs);
        }

        [Test]
        public void Should_Have_Error_When_Protein_Is_Negative()
        {
            var model = new NewIngredientRequest
            {
                Name = "Test Ingredient",
                Carbs = 10,
                Protein = -1,
                Fat = 2,
                Calories = 100,
                Uom = UnitOfMeasurement.Grams
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Protein);
        }

        [Test]
        public void Should_Have_Error_When_Fat_Is_Negative()
        {
            var model = new NewIngredientRequest
            {
                Name = "Test Ingredient",
                Carbs = 10,
                Protein = 5,
                Fat = -1,
                Calories = 100,
                Uom = UnitOfMeasurement.Grams
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Fat);
        }

        [Test]
        public void Should_Have_Error_When_Calories_Is_Negative()
        {
            var model = new NewIngredientRequest
            {
                Name = "Test Ingredient",
                Carbs = 10,
                Protein = 5,
                Fat = 2,
                Calories = -1,
                Uom = UnitOfMeasurement.Grams
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Calories);
        }

        [Test]
        public void Should_Not_Have_Error_When_Request_Is_Valid()
        {
            var model = new NewIngredientRequest
            {
                Name = "Test Ingredient",
                Carbs = 10,
                Protein = 5,
                Fat = 2,
                Calories = 100,
                Uom = UnitOfMeasurement.Grams
            };

            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}