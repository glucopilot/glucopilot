using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Meals.NewMeal;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Meals;

[TestFixture]
public class NewMealTests
{
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Meal>> _mealRepositoryMock;
    private Mock<IRepository<Ingredient>> _ingredientRepositoryMock;
    private Mock<IValidator<NewMealRequest>> _validatorMock;

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _mealRepositoryMock = new Mock<IRepository<Meal>>();
        _ingredientRepositoryMock = new Mock<IRepository<Ingredient>>();
        _validatorMock = new Mock<IValidator<NewMealRequest>>();
    }

    [Test]
    public async Task HandleAsync_Should_Throw_ValidationProblem_When_Validation_Fails()
    {
        var request = new NewMealRequest { Name = "", MealIngredients = new List<NewMealIngredientRequest>() };
        var validationResult = new ValidationResult([new ValidationFailure("Name", "Name is required")]);

        _validatorMock.Setup(x => x.ValidateAsync(request, default)).ReturnsAsync(validationResult);
        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _mealRepositoryMock.Object, _ingredientRepositoryMock.Object);

        Assert.Multiple(() =>
        {
            var validationProblem = result.Result as ValidationProblem;
            Assert.That(validationProblem, Is.InstanceOf<ValidationProblem>());
        });
    }

    [Test]
    public void HandleAsync_Should_Throw_UnauthorizedException_When_User_Not_Logged_In()
    {
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<NewMealRequest>(), default)).ReturnsAsync(new ValidationResult());
        _currentUserMock.Setup(x => x.GetUserId()).Throws(new UnauthorizedException("USER_NOT_LOGGED_IN"));
        var request = new NewMealRequest { Name = "Test Meal", MealIngredients = new List<NewMealIngredientRequest>() };

        Assert.That(async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _mealRepositoryMock.Object, _ingredientRepositoryMock.Object),
            Throws.TypeOf<UnauthorizedException>().With.Message.EqualTo("USER_NOT_LOGGED_IN"));
    }

    [Test]
    public async Task HandleAsync_Should_Return_Created_When_Meal_Is_Successfully_Created()
    {
        var userId = Guid.NewGuid();
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<NewMealRequest>(), default)).ReturnsAsync(new ValidationResult());
        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        var request = new NewMealRequest { Name = "Test Meal", MealIngredients = new List<NewMealIngredientRequest>() };

        _mealRepositoryMock.Setup(x => x.Add(It.IsAny<Meal>()));
        _ingredientRepositoryMock.Setup(x => x.Find(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new List<Ingredient>().AsQueryable());

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _mealRepositoryMock.Object, _ingredientRepositoryMock.Object);

        Assert.That(result.Result, Is.InstanceOf<Created<NewMealResponse>>());
        var createdResult = result.Result as Created<NewMealResponse>;
        Assert.That(createdResult?.Value.Name, Is.EqualTo("Test Meal"));
    }

    [Test]
    public async Task Should_Add_MealIngredients_To_NewMeal()
    {
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<NewMealRequest>(), default)).ReturnsAsync(new ValidationResult());
        var mockCurrentUser = new Mock<ICurrentUser>();
        mockCurrentUser.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());

        var ingredientIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var mockRepository = new Mock<IRepository<Meal>>();
        _ingredientRepositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(ingredientIds.Select(id => new Ingredient { Id = id, Uom = UnitOfMeasurement.Unit, Created = DateTimeOffset.UtcNow, Name = "Ingredient" }).AsQueryable());

        var request = new NewMealRequest
        {
            Name = "Test Meal",
            MealIngredients = new List<NewMealIngredientRequest>
            {
                new NewMealIngredientRequest
                {
                    IngredientId = ingredientIds[0],
                    Quantity = 2
                },
                new NewMealIngredientRequest
                {
                    IngredientId = ingredientIds[1],
                    Quantity = 3
                }
            }
        };

        await Endpoint.HandleAsync(request, _validatorMock.Object, mockCurrentUser.Object, mockRepository.Object, _ingredientRepositoryMock.Object);

        mockRepository.Verify(x => x.AddAsync(It.Is<Meal>(meal =>
            meal.MealIngredients.Count == 2 &&
            meal.MealIngredients.Any(mi => mi.Quantity == 2) &&
            meal.MealIngredients.Any(mi => mi.Quantity == 3)
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void HandleAsync_Should_Throw_BadRequestException_When_Invalid_Ingredient_Ids_Provided()
    {
        var invalidIngredientId = Guid.NewGuid();
        var request = new NewMealRequest
        {
            Name = "Test Meal",
            MealIngredients = new List<NewMealIngredientRequest>
            {
                new NewMealIngredientRequest
                {
                    IngredientId = invalidIngredientId,
                    Quantity = 1
                }
            }
        };

        var validatorMock = new Mock<IValidator<NewMealRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(Guid.NewGuid());

        _ingredientRepositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(Enumerable.Empty<Ingredient>().AsQueryable());

        var exception = Assert.ThrowsAsync<BadRequestException>(async () =>
            await Endpoint.HandleAsync(
                request,
                validatorMock.Object,
                currentUserMock.Object,
                _mealRepositoryMock.Object,
                _ingredientRepositoryMock.Object));

        Assert.That(exception.Message, Is.EqualTo("INGREDIENT_ID_INVALID"));
    }
}