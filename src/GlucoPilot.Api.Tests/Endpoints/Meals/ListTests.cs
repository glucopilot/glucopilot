using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Meals;
using GlucoPilot.Api.Models;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
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
public class ListTests
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
    public async Task HandleAsync_ReturnsValidationProblem_WhenRequestIsInvalid()
    {
        var request = new ListMealsRequest { Page = 0, PageSize = 10 };
        var validationResult = new ValidationResult(new List<ValidationFailure>
        {
            new ValidationFailure("Page", "Page is required")
        });

        _validatorMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(validationResult);

        var result = await List.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.Multiple(() =>
        {
            var validationProblem = result.Result as ValidationProblem;
            Assert.That(validationProblem, Is.InstanceOf<ValidationProblem>());
            Assert.That(validationProblem!.ProblemDetails.Errors, Contains.Key(nameof(ListMealsRequest.Page)));
        });
    }

    [Test]
    public async Task HandleAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        var request = new ListMealsRequest { Page = 0, PageSize = 10 };
        _validatorMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns((Guid?)null);

        Assert.That(() => List.HandleAsync(
                request,
                _validatorMock.Object,
                _currentUserMock.Object,
                _repositoryMock.Object,
                CancellationToken.None), Throws.TypeOf<UnauthorizedException>().With.Message.EqualTo("USER_NOT_LOGGED_IN"));
    }

    [Test]
    public async Task HandleAsync_ReturnsOk_WithMealsList()
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

        var result = await List.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        var okResult = result.Result as Ok<ListMealsResponse>;
        Assert.That(okResult, Is.InstanceOf<Ok<ListMealsResponse>>());
        Assert.That(okResult.Value.Meals.Count, Is.EqualTo(2));
        Assert.That(okResult.Value.Meals.ElementAt(0).Id, Is.EqualTo(meals[0].Id));
        Assert.That(okResult.Value.Meals.ElementAt(1).Id, Is.EqualTo(meals[1].Id));
        Assert.That(okResult.Value.NumberOfPages, Is.EqualTo(1));
    }
}
