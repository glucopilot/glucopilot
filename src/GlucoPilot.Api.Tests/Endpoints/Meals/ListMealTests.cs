using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Meals.List;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Endpoints.Meals;

[TestFixture]
public class ListMealTests
{
    private static readonly Guid _userId = Guid.NewGuid();
    private Mock<IValidator<ListMealsRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Meal>> _repositoryMock;

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(c => c.GetUserId()).Returns(_userId);
        _validatorMock = new Mock<IValidator<ListMealsRequest>>();
        _repositoryMock = new Mock<IRepository<Meal>>();
    }

    [Test]
    public void HandleAsync_Returns_Unauthorized_When_User_Is_Not_Authenticated()
    {
        var mockValidator = new Mock<IValidator<ListMealsRequest>>();
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ListMealsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _currentUserMock.Setup(x => x.GetUserId()).Throws(new UnauthorizedException("USER_NOT_LOGGED_IN"));

        var mockRepository = new Mock<IRepository<Meal>>();

        var request = new ListMealsRequest { Page = 0, PageSize = 10 };

        Assert.That(() => Endpoint.HandleAsync(request, mockValidator.Object, _currentUserMock.Object, mockRepository.Object, CancellationToken.None),
            Throws.InstanceOf<UnauthorizedException>().With.Message.EqualTo("USER_NOT_LOGGED_IN"));
    }

    [Test]
    public async Task HandleAsync_Returns_Validation_Problem_When_Request_Is_Invalid()
    {
        var request = new ListMealsRequest { Page = 0, PageSize = 10 };
        var validationResult = new ValidationResult(new List<ValidationFailure>
        {
            new ValidationFailure("Page", "Page is required")
        });

        _validatorMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(validationResult);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.Multiple(() =>
        {
            var validationProblem = result.Result as ValidationProblem;
            Assert.That(validationProblem, Is.InstanceOf<ValidationProblem>());
            Assert.That(validationProblem!.ProblemDetails.Errors, Contains.Key(nameof(ListMealsRequest.Page)));
        });
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_Meals_List()
    {
        var request = new ListMealsRequest { Page = 0, PageSize = 10 };

        var meals = new List<Meal>
        {
            new Meal { Id = Guid.NewGuid(), UserId = _userId, Name = "Meal1", Created = DateTimeOffset.UtcNow },
            new Meal { Id = Guid.NewGuid(), UserId = _userId, Name = "Meal2", Created = DateTimeOffset.UtcNow.AddDays(-1) }
        };

        _validatorMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(_userId);
        _repositoryMock.Setup(r => r.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>())).Returns(meals.AsQueryable());
        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Meal, bool>>>(), default)).ReturnsAsync(meals.Count);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        var okResult = result.Result as Ok<ListMealsResponse>;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.InstanceOf<Ok<ListMealsResponse>>());
            Assert.That(okResult.Value.Meals, Has.Count.EqualTo(2));
            Assert.That(okResult.Value.Meals.ElementAt(0).Id, Is.EqualTo(meals[0].Id));
            Assert.That(okResult.Value.Meals.ElementAt(1).Id, Is.EqualTo(meals[1].Id));
            Assert.That(okResult.Value.NumberOfPages, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_Correct_Meals_And_Nutrition()
    {
        var userId = Guid.NewGuid();
        var request = new ListMealsRequest { Page = 0, PageSize = 2 };

        var ingredient1 = new Ingredient
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
            Name = "Ingredient1",
            Carbs = 10,
            Protein = 5,
            Fat = 2,
            Calories = 100,
            Uom = UnitOfMeasurement.Grams,
        };
        var ingredient2 = new Ingredient
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
            Name = "Ingredient2",
            Carbs = 20,
            Protein = 10,
            Fat = 4,
            Calories = 200,
            Uom = UnitOfMeasurement.Grams,
        };

        var meal1Id = Guid.NewGuid();
        var meal2Id = Guid.NewGuid();

        var meals = new List<Meal>
            {
                new Meal
                {
                    Id = meal1Id,
                    UserId = userId,
                    Name = "Meal1",
                    Created = DateTimeOffset.UtcNow,
                    MealIngredients = new List<MealIngredient>
                    {
                        new MealIngredient { Id = Guid.NewGuid(), Ingredient = ingredient1, Quantity = 2, IngredientId = ingredient1.Id, MealId = meal1Id },
                        new MealIngredient { Id = Guid.NewGuid(), Ingredient = ingredient2, Quantity = 1, IngredientId = ingredient2.Id, MealId = meal1Id }
                    }
                },
                new Meal
                {
                    Id = meal2Id,
                    UserId = userId,
                    Name = "Meal2",
                    Created = DateTimeOffset.UtcNow.AddDays(-1),
                    MealIngredients = new List<MealIngredient>
                    {
                        new MealIngredient {Id = Guid.NewGuid(), Ingredient = ingredient1, Quantity = 1, IngredientId = ingredient1.Id, MealId = meal2Id }
                    }
                }
            };

        _validatorMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _repositoryMock.Setup(r => r.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>())).Returns(meals.AsQueryable());
        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(meals.Count);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        var okResult = result.Result as Ok<ListMealsResponse>;
        Assert.That(okResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(okResult.Value.Meals, Has.Count.EqualTo(2));
            Assert.That(okResult.Value.Meals.ElementAt(0).Name, Is.EqualTo("Meal1"));
            Assert.That(okResult.Value.Meals.ElementAt(0).TotalCalories, Is.EqualTo(400));
            Assert.That(okResult.Value.Meals.ElementAt(0).TotalCarbs, Is.EqualTo(40));
            Assert.That(okResult.Value.Meals.ElementAt(0).TotalProtein, Is.EqualTo(20));
            Assert.That(okResult.Value.Meals.ElementAt(0).TotalFat, Is.EqualTo(8));

            Assert.That(okResult.Value.Meals.ElementAt(1).Name, Is.EqualTo("Meal2"));
            Assert.That(okResult.Value.Meals.ElementAt(1).TotalCalories, Is.EqualTo(100));
            Assert.That(okResult.Value.Meals.ElementAt(1).TotalCarbs, Is.EqualTo(10));
            Assert.That(okResult.Value.Meals.ElementAt(1).TotalProtein, Is.EqualTo(5));
            Assert.That(okResult.Value.Meals.ElementAt(1).TotalFat, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task HandleAsync_Should_Handle_Null_Ingredient()
    {
        var mockValidator = new Mock<IValidator<ListMealsRequest>>();
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ListMealsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var mockCurrentUser = new Mock<ICurrentUser>();
        var userId = Guid.NewGuid();
        mockCurrentUser.Setup(cu => cu.GetUserId()).Returns(userId);

        var mockRepository = new Mock<IRepository<Meal>>();
        var mealId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        mockRepository
            .Setup(r => r.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new List<Meal>
            {
                    new Meal
                    {
                        Id = mealId,
                        UserId = userId,
                        Created = DateTimeOffset.UtcNow,
                        Name = "Test Meal",
                        MealIngredients = new List<MealIngredient>
                        {
                            new MealIngredient
                            {
                                Id = Guid.NewGuid(),
                                Quantity = 2,
                                Ingredient = null,
                                IngredientId = ingredientId,
                                MealId = mealId
                            }
                        }
                    }
            }.AsQueryable());

        mockRepository
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new ListMealsRequest
        {
            Page = 0,
            PageSize = 10
        };

        var result = await Endpoint.HandleAsync(
            request,
            mockValidator.Object,
            mockCurrentUser.Object,
            mockRepository.Object,
            CancellationToken.None);

        var okResult = result.Result as Ok<ListMealsResponse>;
        var response = okResult.Value;

        Assert.That(response, Is.Not.Null);
        var meal = response.Meals.First();
        Assert.Multiple(() =>
        {
            Assert.That(meal.Name, Is.EqualTo("Test Meal"));
            Assert.That(meal.MealIngredients, Has.Count.EqualTo(1));
        });
        var ingredientResponse = meal.MealIngredients.First().Ingredient;
        Assert.That(ingredientResponse, Is.Null);
    }

    [Test]
    public async Task HandleAsync_Should_Filter_Meals_When_Search_Is_Provided()
    {
        var userId = Guid.NewGuid();
        var request = new ListMealsRequest
        {
            Search = "Lunch",
            Page = 0,
            PageSize = 10
        };

        var meals = new List<Meal>
        {
            new Meal { Id = Guid.NewGuid(), UserId = userId, Name = "Breakfast", Created = DateTimeOffset.UtcNow },
            new Meal { Id = Guid.NewGuid(), UserId = userId, Name = "Lunch", Created = DateTimeOffset.UtcNow }
        }.AsQueryable();

        _validatorMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _repositoryMock.Setup(r => r.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>()))
                       .Returns((Expression<Func<Meal, bool>> predicate, FindOptions _) => meals.Where(predicate));

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        _repositoryMock.Verify(r => r.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>()), Times.Once);

        var okResult = result.Result as Ok<ListMealsResponse>;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value.Meals, Has.Count.EqualTo(1));
        Assert.That(okResult.Value.Meals.First().Name, Is.EqualTo("Lunch"));
    }
}
