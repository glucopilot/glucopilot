using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Api.Endpoints.Meals.RemoveMeal;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Meals;

[TestFixture]
public class RemoveMealTests
{
    private Mock<IRepository<Meal>> _mealRepositoryMock;
    private Mock<ICurrentUser> _currentUserMock;

    [SetUp]
    public void SetUp()
    {
        _mealRepositoryMock = new Mock<IRepository<Meal>>();
        _currentUserMock = new Mock<ICurrentUser>();
    }

    [Test]
    public async Task HandleAsync_Should_Return_NoContent_When_Meal_Is_Deleted()
    {
        var mealId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var meal = new Meal { Id = mealId, UserId = userId, Name = "Meal1", Created = DateTimeOffset.UtcNow };

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _mealRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(meal);

        var result = await Endpoint.HandleAsync(mealId, _mealRepositoryMock.Object, _currentUserMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NoContent>());
        _mealRepositoryMock.Verify(r => r.DeleteAsync(meal, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void HandleAsync_Should_Throw_NotFoundException_When_Meal_Does_Not_Exist()
    {
        var mealId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _mealRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meal)null);

        Assert.That(async () => await Endpoint.HandleAsync(mealId, _mealRepositoryMock.Object, _currentUserMock.Object, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("MEAL_NOT_FOUND"));
    }
}