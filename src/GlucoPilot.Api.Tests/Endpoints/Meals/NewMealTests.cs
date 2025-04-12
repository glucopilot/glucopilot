using NUnit.Framework;
using Moq;
using GlucoPilot.Api.Endpoints.Meals.NewMeal;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Api.Models;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

[TestFixture]
public class NewMealTests
{
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Meal>> _repositoryMock;
    private Mock<IValidator<NewMealRequest>> _validatorMock;

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _repositoryMock = new Mock<IRepository<Meal>>();
        _validatorMock = new Mock<IValidator<NewMealRequest>>();
    }

    [Test]
    public async Task HandleAsync_Should_Throw_ValidationProblem_When_Validation_Fails()
    {
        var request = new NewMealRequest { Name = "", MealIngredients = new List<NewMealIngredientRequest>() };
        var validationResult = new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") });

        _validatorMock.Setup(x => x.ValidateAsync(request, default)).ReturnsAsync(validationResult);
        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object);

        Assert.Multiple(() =>
        {
            var validationProblem = result.Result as ValidationProblem;
            Assert.That(validationProblem, Is.InstanceOf<ValidationProblem>());
        });
    }

    [Test]
    public void HandleAsync_Should_Throw_UnauthorizedException_When_User_Not_Logged_In()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Throws(new UnauthorizedException("USER_NOT_LOGGED_IN"));
        var request = new NewMealRequest { Name = "Test Meal", MealIngredients = new List<NewMealIngredientRequest>() };

        Assert.That(async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object),
            Throws.TypeOf<UnauthorizedException>().With.Message.EqualTo("USER_NOT_LOGGED_IN"));
    }

    [Test]
    public async Task HandleAsync_Should_Return_Created_When_Meal_Is_Successfully_Created()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        var request = new NewMealRequest { Name = "Test Meal", MealIngredients = new List<NewMealIngredientRequest>() };

        _repositoryMock.Setup(x => x.Add(It.IsAny<Meal>()));

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object);

        Assert.That(result.Result, Is.InstanceOf<Created<NewMealResponse>>());
        var createdResult = result.Result as Created<NewMealResponse>;
        Assert.That(createdResult?.Value.Name, Is.EqualTo("Test Meal"));
    }

    [Test]
    public async Task Should_Add_MealIngredients_To_NewMeal()
    {
        var mockCurrentUser = new Mock<ICurrentUser>();
        mockCurrentUser.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());

        var mockRepository = new Mock<IRepository<Meal>>();

        var request = new NewMealRequest
        {
            Name = "Test Meal",
            MealIngredients = new List<NewMealIngredientRequest>
        {
            new NewMealIngredientRequest
            {
                Id = Guid.NewGuid(),
                MealId = Guid.NewGuid(),
                IngredientId = Guid.NewGuid(),
                Quantity = 2
            },
            new NewMealIngredientRequest
            {
                Id = Guid.NewGuid(),
                MealId = Guid.NewGuid(),
                IngredientId = Guid.NewGuid(),
                Quantity = 3
            }
        }
        };

        await Endpoint.HandleAsync(request, _validatorMock.Object, mockCurrentUser.Object, mockRepository.Object);

        mockRepository.Verify(x => x.Add(It.Is<Meal>(meal =>
            meal.MealIngredients.Count == 2 &&
            meal.MealIngredients.Any(mi => mi.Quantity == 2) &&
            meal.MealIngredients.Any(mi => mi.Quantity == 3)
        )), Times.Once);
    }
}