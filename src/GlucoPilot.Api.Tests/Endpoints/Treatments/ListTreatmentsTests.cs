using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Treatments.ListTreatments;
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

namespace GlucoPilot.Api.Tests.Endpoints.Treatments;

[TestFixture]
public class ListTreatmentsTests
{
    private Mock<IValidator<ListTreatmentsRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Treatment>> _treatmentRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<ListTreatmentsRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _treatmentRepositoryMock = new Mock<IRepository<Treatment>>();
    }

    [Test]
    public async Task HandleAsync_Returns_ValidationProblem_When_Request_Is_Invalid()
    {
        var request = new ListTreatmentsRequest { Page = 1, PageSize = 10 };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult(new[]
            {
                    new ValidationFailure("Page", "Page is required.")
            }));

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _treatmentRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
    }

    [Test]
    public async Task HandleAsync_Returns_Unauthorized_When_User_Is_Not_Authenticated()
    {
        var request = new ListTreatmentsRequest { Page = 1, PageSize = 10 };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _currentUserMock
            .Setup(c => c.GetUserId())
            .Throws<UnauthorizedAccessException>();

        Assert.That(async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _treatmentRepositoryMock.Object, CancellationToken.None),
            Throws.InstanceOf<UnauthorizedAccessException>());
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_Valid_Response()
    {
        var userId = Guid.NewGuid();
        var request = new ListTreatmentsRequest { Page = 1, PageSize = 10 };
        var mealId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var treatments = new List<Treatment>
            {
                new Treatment
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Created = DateTimeOffset.UtcNow,
                    MealId = mealId,
                    InjectionId = null,
                    Meal = new Meal
                    {
                        Id = mealId,
                        Name = "Lunch",
                        Created = DateTimeOffset.UtcNow,
                        MealIngredients = new List<MealIngredient>
                        {
                            new MealIngredient
                            {
                                Ingredient = new Ingredient { Id = ingredientId, Created = DateTimeOffset.UtcNow, Name = "Sugar", Uom = UnitOfMeasurement.Grams, Calories = 100, Carbs = 20, Protein = 10, Fat = 5 },
                                Quantity = 1,
                                Id = Guid.NewGuid(),
                                IngredientId = ingredientId,
                                MealId = mealId
                            }
                        }
                    }
                }
            };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(userId);
        _treatmentRepositoryMock
            .Setup(r => r.GetAll(It.IsAny<FindOptions>()))
            .Returns(treatments.AsQueryable());
        _treatmentRepositoryMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Treatment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(treatments.Count);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _treatmentRepositoryMock.Object, CancellationToken.None);

        Assert.Multiple(() =>
        {
            var okResult = result.Result as Ok<ListTreatmentsResponse>;
            Assert.That(okResult, Is.TypeOf<Ok<ListTreatmentsResponse>>());
            Assert.That(okResult?.Value.Treatments.Count, Is.EqualTo(1));
            Assert.That(okResult?.Value.NumberOfPages, Is.EqualTo(1));
        });
    }
}
