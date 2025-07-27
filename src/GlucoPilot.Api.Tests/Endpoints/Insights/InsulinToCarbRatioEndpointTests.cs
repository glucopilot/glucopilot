using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.Api.Endpoints.Insights.InsulinToCarbRatio;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Insights;

[TestFixture]
internal sealed class InsulinToCarbRatioEndpointTests
{
    private Mock<IValidator<InsulinToCarbRatioRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Treatment>> _repositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<InsulinToCarbRatioRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _repositoryMock = new Mock<IRepository<Treatment>>();
    }

    [Test]
    public async Task HandleAsync_With_Valid_Request_Returns_Average_Insulin_To_Carb_Ratio()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(userId);
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<InsulinToCarbRatioRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var treatments = new[]
        {
            new Treatment
            {
                UserId = userId,
                Injection = new Injection { InsulinId = Guid.NewGuid(), Units = 10 },
                Meals = [new TreatmentMeal
                {
                    TreatmentId = Guid.NewGuid(),
                    MealId = Guid.NewGuid(),
                    Meal = new Meal
                    {
                        Name = "Breakfast",
                        Created = DateTimeOffset.UtcNow.AddHours(-1),
                        MealIngredients = new List<MealIngredient>
                        {
                            new MealIngredient { Id = Guid.NewGuid(), IngredientId = Guid.NewGuid(), MealId = Guid.NewGuid(), Quantity = 1, Ingredient = new Ingredient { Created = DateTime.UtcNow.AddHours(-12), Name = "Test Ingredient 2", Uom = UnitOfMeasurement.Grams,Carbs = 50 } }
                        }
                    },
                    Quantity = 1
                }],
                Ingredients = [
                    new TreatmentIngredient
                    {
                        TreatmentId = Guid.NewGuid(),
                        IngredientId = Guid.NewGuid(),
                        Ingredient = new Ingredient { Created = DateTime.UtcNow.AddHours(-12), Name = "Test Ingredient 2", Uom = UnitOfMeasurement.Grams, Carbs = 50 },
                        Quantity = 2
                    }
                ]
            },
            new Treatment
            {
                UserId = userId,
                Injection = new Injection { InsulinId = Guid.NewGuid(), Units = 20 },
                Meals = [new TreatmentMeal
                {
                    TreatmentId = Guid.NewGuid(),
                    MealId = Guid.NewGuid(),
                    Meal = new Meal
                    {
                        Name = "Breakfast 2",
                        Created = DateTimeOffset.UtcNow,
                        MealIngredients = new List<MealIngredient>
                        {
                            new MealIngredient { Id = Guid.NewGuid(), IngredientId = Guid.NewGuid(), MealId = Guid.NewGuid(), Quantity = 1, Ingredient = new Ingredient { Created = DateTime.UtcNow.AddHours(-12), Name = "Test Ingredient", Uom = UnitOfMeasurement.Grams, Carbs = 100 } }
                        }
                    },
                    Quantity = 1
                }]
            }
        }.AsQueryable();

        _repositoryMock.Setup(repo => repo.Find(It.IsAny<Expression<Func<Treatment, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(treatments);

        var request = new InsulinToCarbRatioRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-7),
            To = DateTimeOffset.UtcNow
        };

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<decimal?>>());
            Assert.That(((Ok<decimal?>)result.Result).Value, Is.EqualTo(10)); // Average insulin to carb ratio: (150/10 + 100/20) / 2 = 10
        });
    }

    [Test]
    public async Task HandleAsync_With_Invalid_Request_Returns_Validation_Problem()
    {
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<InsulinToCarbRatioRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult([
                new FluentValidation.Results.ValidationFailure("From", "From date is required")
            ]));

        var request = new InsulinToCarbRatioRequest();

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
    }

    [Test]
    public void HandleAsync_With_Unauthorized_User_Returns_Unauthorized_Result()
    {
        var exception = new UnauthorizedException("test");
        _currentUserMock.Setup(cu => cu.GetUserId()).Throws(exception);

        var request = new InsulinToCarbRatioRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-7),
            To = DateTimeOffset.UtcNow
        };

        Assert.That(() => Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None), Throws.Exception.SameAs(exception));
    }

    [Test]
    public async Task HandleAsync_With_No_Treatments_Returns_Null_Average()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(userId);
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<InsulinToCarbRatioRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _repositoryMock.Setup(repo => repo.Find(It.IsAny<Expression<Func<Treatment, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(Enumerable.Empty<Treatment>().AsQueryable());

        var request = new InsulinToCarbRatioRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-7),
            To = DateTimeOffset.UtcNow
        };

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<decimal?>>());
            Assert.That(((Ok<decimal?>)result.Result).Value, Is.Null);
        });
    }
}