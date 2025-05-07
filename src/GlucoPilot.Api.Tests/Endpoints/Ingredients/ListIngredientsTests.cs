using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Ingredients.List;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using MailKit.Search;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Ingredients;

[TestFixture]
public class ListIngredientsTests
{
    private Mock<IValidator<ListIngredientsRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Ingredient>> _repositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<ListIngredientsRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _repositoryMock = new Mock<IRepository<Ingredient>>();
    }

    [Test]
    public async Task HandleAsync_Returns_ValidationProblem_When_Request_Is_Invalid()
    {
        var request = new ListIngredientsRequest { Page = 0, PageSize = 10 };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure("Page", "Page must be greater than 0")
            ]));

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_Ingredients_When_Request_Is_Valid()
    {
        var userId = Guid.NewGuid();
        var request = new ListIngredientsRequest { Page = 0, PageSize = 2 };
        var ingredients = new List<Ingredient>
        {
            new Ingredient { Id = Guid.NewGuid(), UserId = userId, Name = "Ingredient1", Created = DateTimeOffset.UtcNow, Uom = UnitOfMeasurement.Grams },
            new Ingredient { Id = Guid.NewGuid(), UserId = userId, Name = "Ingredient2", Created = DateTimeOffset.UtcNow, Uom = UnitOfMeasurement.Grams }
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _repositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(ingredients.AsQueryable());
        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredients.Count);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<Ok<ListIngredientsResponse>>());
        var okResult = result.Result as Ok<ListIngredientsResponse>;
        Assert.That(okResult?.Value.Ingredients.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task HandleAsync_ReturnsFilteredIngredients_WhenSearchIsProvided()
    {
        var userId = Guid.NewGuid();
        var ingredients = new List<Ingredient>
            {
                new Ingredient { Id = Guid.NewGuid(), UserId = userId, Name = "Apple Pie", Uom = UnitOfMeasurement.Grams, Created = DateTimeOffset.UtcNow },
                new Ingredient { Id = Guid.NewGuid(), UserId = userId, Name = "Banana Bread", Uom = UnitOfMeasurement.Grams, Created = DateTimeOffset.UtcNow },
                new Ingredient { Id = Guid.NewGuid(), UserId = userId, Name = "Green Apple", Uom = UnitOfMeasurement.Grams, Created = DateTimeOffset.UtcNow }
            }.AsQueryable();

        var request = new ListIngredientsRequest
        {
            Search = "apple",
            Page = 0,
            PageSize = 10
        };

        _validatorMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _repositoryMock.Setup(r => r.Find(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>()))
                       .Returns((Expression<Func<Ingredient, bool>> predicate, FindOptions _) => ingredients.Where(predicate));

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        _repositoryMock.Verify(r => r.Find(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>()), Times.Once);

        Assert.That(result.Result, Is.InstanceOf<Ok<ListIngredientsResponse>>());
        var response = (result.Result as Ok<ListIngredientsResponse>).Value;

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Ingredients.Count, Is.EqualTo(2));
        Assert.That(response.Ingredients.All(i => i.Name.Contains("apple", StringComparison.OrdinalIgnoreCase)), Is.True);
    }
}