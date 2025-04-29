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

namespace GlucoPilot.Api.Tests.Endpoints.Meals;

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
            Assert.That(okResult.Value.MealIngredients, Has.Count.EqualTo(1));
            var ingredient = okResult.Value.MealIngredients[0].Ingredient;
            Assert.That(ingredient.Name, Is.EqualTo("Test Ingredient"));
            Assert.That(ingredient.Calories, Is.EqualTo(80));
            Assert.That(ingredient.Carbs, Is.EqualTo(10));
            Assert.That(ingredient.Protein, Is.EqualTo(5));
            Assert.That(ingredient.Fat, Is.EqualTo(2));
            Assert.That(ingredient.Uom, Is.EqualTo(UnitOfMeasurement.Grams));
        });
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_Correct_Total_Nutrition_And_Ingredients()
    {
        var userId = Guid.NewGuid();
        var mealId = Guid.NewGuid();
        var ingredient1 = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = "Ingredient1",
            Created = DateTimeOffset.UtcNow,
            Carbs = 10,
            Protein = 5,
            Fat = 2,
            Calories = 100,
            Uom = UnitOfMeasurement.Grams,
        };
        var ingredient2 = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = "Ingredient2",
            Created = DateTimeOffset.UtcNow,
            Carbs = 20,
            Protein = 10,
            Fat = 4,
            Calories = 200,
            Uom = UnitOfMeasurement.Grams,
        };
        var meal = new Meal
        {
            Id = mealId,
            UserId = userId,
            Name = "Test Meal",
            Created = DateTimeOffset.UtcNow,
            MealIngredients = new List<MealIngredient>
            {
                new MealIngredient { Id = Guid.NewGuid(), Ingredient = ingredient1, Quantity = 2, MealId = mealId, IngredientId = ingredient1.Id },
                new MealIngredient { Id = Guid.NewGuid(), Ingredient = ingredient2, Quantity = 1, MealId = mealId, IngredientId = ingredient2.Id  }
            }
        };

        _mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        _mockRepository
            .Setup(r => r.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new List<Meal> { meal }.AsQueryable());

        var result = await Endpoint.HandleAsync(mealId, _mockCurrentUser.Object, _mockRepository.Object);

        var okResult = result.Result as Ok<GetMealResponse>;
        Assert.That(okResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(okResult.Value.TotalCalories, Is.EqualTo(400));
            Assert.That(okResult.Value.TotalCarbs, Is.EqualTo(40));
            Assert.That(okResult.Value.TotalProtein, Is.EqualTo(20));
            Assert.That(okResult.Value.TotalFat, Is.EqualTo(8));
            Assert.That(okResult.Value.MealIngredients, Has.Count.EqualTo(2));
            Assert.That(okResult.Value.MealIngredients[0].Ingredient.Name, Is.EqualTo("Ingredient1"));
            Assert.That(okResult.Value.MealIngredients[1].Ingredient.Name, Is.EqualTo("Ingredient2"));
        });
    }

    [Test]
    public async Task HandleAsync_Should_Handle_Null_Ingredient()
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
                    Quantity = 2,
                    Ingredient = null
                }
            }
        };

        _mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        _mockRepository
            .Setup(r => r.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new List<Meal> { meal }.AsQueryable());

        var result = await Endpoint.HandleAsync(mealId, _mockCurrentUser.Object, _mockRepository.Object);

        var okResult = result.Result as Ok<GetMealResponse>;
        Assert.That(okResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(okResult.Value.Id, Is.EqualTo(mealId));
            Assert.That(okResult.Value.Name, Is.EqualTo("Test Meal"));
            Assert.That(okResult.Value.MealIngredients, Has.Count.EqualTo(1));
            var ingredientResponse = okResult.Value.MealIngredients.First().Ingredient;
            Assert.That(ingredientResponse, Is.Null);
        });
    }
}