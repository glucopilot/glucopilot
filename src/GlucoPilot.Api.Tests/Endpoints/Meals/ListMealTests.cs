﻿using FluentValidation;
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
    private Mock<IValidator<ListIngredientsRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Meal>> _repositoryMock;

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(c => c.GetUserId()).Returns(_userId);
        _validatorMock = new Mock<IValidator<ListIngredientsRequest>>();
        _repositoryMock = new Mock<IRepository<Meal>>();
    }

    [Test]
    public void HandleAsync_Returns_Unauthorized_When_User_Is_Not_Authenticated()
    {
        var mockValidator = new Mock<IValidator<ListIngredientsRequest>>();
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ListIngredientsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _currentUserMock.Setup(x => x.GetUserId()).Throws(new UnauthorizedException("USER_NOT_LOGGED_IN"));

        var mockRepository = new Mock<IRepository<Meal>>();

        var request = new ListIngredientsRequest { Page = 0, PageSize = 10 };

        Assert.That(() => Endpoint.HandleAsync(request, mockValidator.Object, _currentUserMock.Object, mockRepository.Object, CancellationToken.None),
            Throws.InstanceOf<UnauthorizedException>().With.Message.EqualTo("USER_NOT_LOGGED_IN"));
    }

    [Test]
    public async Task HandleAsync_Returns_Validation_Problem_When_Request_Is_Invalid()
    {
        var request = new ListIngredientsRequest { Page = 0, PageSize = 10 };
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
            Assert.That(validationProblem!.ProblemDetails.Errors, Contains.Key(nameof(ListIngredientsRequest.Page)));
        });
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_Meals_List()
    {
        var request = new ListIngredientsRequest { Page = 0, PageSize = 10 };

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
        Assert.That(okResult, Is.InstanceOf<Ok<ListMealsResponse>>());
        Assert.That(okResult.Value.Meals.Count, Is.EqualTo(2));
        Assert.That(okResult.Value.Meals.ElementAt(0).Id, Is.EqualTo(meals[0].Id));
        Assert.That(okResult.Value.Meals.ElementAt(1).Id, Is.EqualTo(meals[1].Id));
        Assert.That(okResult.Value.NumberOfPages, Is.EqualTo(1));
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_Correct_Meals_And_Nutrition()
    {
        var userId = Guid.NewGuid();
        var request = new ListIngredientsRequest { Page = 0, PageSize = 2 };

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
            Assert.That(okResult.Value.Meals.Count, Is.EqualTo(2));
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
}
