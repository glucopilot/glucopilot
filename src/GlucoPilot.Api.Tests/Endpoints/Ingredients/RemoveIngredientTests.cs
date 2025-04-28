﻿using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Api.Endpoints.Ingredients.RemoveIngredient;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Ingredients;

[TestFixture]
public class RemoveIngredientTests
{
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Ingredient>> _ingredientRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _ingredientRepositoryMock = new Mock<IRepository<Ingredient>>();
    }

    [Test]
    public void HandleAsync_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        var ingredientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ingredient = new Ingredient { Id = ingredientId, Created = DateTimeOffset.UtcNow, Name = "ingredient", Uom = UnitOfMeasurement.Unit, UserId = userId };
        _currentUserMock.Setup(c => c.GetUserId()).Throws(new UnauthorizedException("INGREDIENT_NOT_FOUND"));
        _ingredientRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredient);

        Assert.That(() => Endpoint.HandleAsync(ingredientId, _currentUserMock.Object, _ingredientRepositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public async Task HandleAsync_Should_Return_NoContent_When_Ingredient_Is_Removed()
    {
        var ingredientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ingredient = new Ingredient { Id = ingredientId, Created = DateTimeOffset.UtcNow, Name = "ingredient", Uom = UnitOfMeasurement.Unit, UserId = userId };

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _ingredientRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredient);

        _ingredientRepositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<Ingredient>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Endpoint.HandleAsync(ingredientId, _currentUserMock.Object, _ingredientRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NoContent>());
    }

    [Test]
    public void HandleAsync_Should_Throw_NotFoundException_When_Ingredient_Is_Not_Found()
    {
        var ingredientId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _ingredientRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingredient)null);

        Assert.That(() => Endpoint.HandleAsync(ingredientId, _currentUserMock.Object, _ingredientRepositoryMock.Object, CancellationToken.None), Throws.InstanceOf<NotFoundException>());
    }
}