using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GlucoPilot.Api.Endpoints.Meals.GetMeal;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Meals
{
    [TestFixture]
    public class GetMealTests
    {
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IRepository<Meal>> _mockRepository;

        public GetMealTests()
        {
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockRepository = new Mock<IRepository<Meal>>();
        }

        [Test]
        public async Task HandleAsync_Returns_Not_Found_When_Meal_Does_Not_Exist()
        {
            var userId = Guid.NewGuid();
            _mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
            _mockRepository
                .Setup(r => r.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>()))
                .Returns(Enumerable.Empty<Meal>().AsQueryable());

            var result = await Endpoint.HandleAsync(Guid.NewGuid(), _mockCurrentUser.Object, _mockRepository.Object);

            Assert.That(result.Result, Is.InstanceOf<NotFound>());
        }

        [Test]
        public async Task HandleAsync_Returns_Ok_When_Meal_Exists()
        {
            var userId = Guid.NewGuid();
            var mealId = Guid.NewGuid();
            var ingredientId = Guid.NewGuid();
            var meal = new Meal
            {
                Id = mealId,
                UserId = userId,
                Name = "Test Meal",
                Created = DateTimeOffset.UtcNow,
                MealIngredients = new List<MealIngredient>
                {
                    new MealIngredient
                    {
                        Id = Guid.NewGuid(),
                        MealId = mealId,
                        IngredientId = ingredientId,
                        Quantity = 100,
                        Ingredient = new Ingredient
                        {
                            Id = ingredientId,
                            Created = DateTimeOffset.UtcNow,
                            Name = "Test Ingredient",
                            Carbs = 10,
                            Protein = 5,
                            Fat = 2,
                            Calories = 80,
                            Uom = UnitOfMeasurement.Grams,
                        }
                    }
                }
            };

            _mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
            _mockRepository
                .Setup(r => r.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>()))
                .Returns(new List<Meal> { meal }.AsQueryable());

            var result = await Endpoint.HandleAsync(mealId, _mockCurrentUser.Object, _mockRepository.Object);

            var okResult = result.Result as Ok<GetMealResponse>;
            Assert.Multiple(() =>
            {
                Assert.That(okResult, Is.InstanceOf<Ok<GetMealResponse>>());
                Assert.That(mealId, Is.EqualTo(okResult.Value.Id));
                Assert.That(okResult.Value.Name, Is.EqualTo("Test Meal"));
                Assert.That(okResult.Value.MealIngredients.Count, Is.EqualTo(1));
                var ingredient = okResult.Value.MealIngredients[0].Ingredient;
                Assert.That(ingredient.Name, Is.EqualTo("Test Ingredient"));
                Assert.That(ingredient.Calories, Is.EqualTo(80));
                Assert.That(ingredient.Carbs, Is.EqualTo(10));
                Assert.That(ingredient.Protein, Is.EqualTo(5));
                Assert.That(ingredient.Fat, Is.EqualTo(2));
                Assert.That(ingredient.Uom, Is.EqualTo(UnitOfMeasurement.Grams));
            });
        }
    }
}