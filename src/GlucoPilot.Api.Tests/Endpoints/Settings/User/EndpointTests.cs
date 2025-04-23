using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Api.Endpoints.Settings.User;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Settings.User;

[TestFixture]
public class EndpointTests
{
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Data.Entities.User>> _userRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _userRepositoryMock = new Mock<IRepository<Data.Entities.User>>();
    }

    [Test]
    public async Task HandleAsync_With_Valid_User_Returns_Ok_With_User_Settings_Response()
    {
        var userId = Guid.NewGuid();
        var user = new Patient
        {
            Id = userId,
            Email = "test@nomail.com",
            PasswordHash = "password",
            Settings = new UserSettings
            {
                GlucoseUnitOfMeasurement = Data.Enums.GlucoseUnitOfMeasurement.MmolL,
                LowSugarThreshold = 70,
                HighSugarThreshold = 180,
                DailyCalorieTarget = 2000,
                DailyCarbTarget = 250,
                DailyProteinTarget = 100,
                DailyFatTarget = 70
            }
        };

        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<Data.Entities.User, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(user);

        var result = await Endpoint.HandleAsync(_currentUserMock.Object, _userRepositoryMock.Object, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<UserSettingsResponse>>());
            var response = ((Ok<UserSettingsResponse>)result.Result).Value;
            Assert.That(response.GlucoseUnitOfMeasurement, Is.EqualTo(GlucoseUnitOfMeasurement.MmolL));
            Assert.That(response.LowSugarThreshold, Is.EqualTo(70));
            Assert.That(response.HighSugarThreshold, Is.EqualTo(180));
            Assert.That(response.DailyCalorieTarget, Is.EqualTo(2000));
            Assert.That(response.DailyCarbTarget, Is.EqualTo(250));
            Assert.That(response.DailyProteinTarget, Is.EqualTo(100));
            Assert.That(response.DailyFatTarget, Is.EqualTo(70));
        });
    }

    [Test]
    public void HandleAsync_With_Null_User_Throws_Unauthorized_Exception()
    {
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<Data.Entities.User, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync((Data.Entities.User)null);

        var exception = Assert.ThrowsAsync<UnauthorizedException>(() => Endpoint.HandleAsync(_currentUserMock.Object, _userRepositoryMock.Object, CancellationToken.None));

        Assert.That(exception.Message, Is.EqualTo("USER_NOT_LOGGED_IN"));
    }

    [Test]
    public async Task HandleAsync_With_Null_User_Settings_Creates_Default_Settings_And_Returns_Ok()
    {
        var userId = Guid.NewGuid();
        var user = new Patient { Id = userId, Email = "user@nomail.com", PasswordHash = "password", Settings = null };

        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<Data.Entities.User, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Data.Entities.User>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

        var result = await Endpoint.HandleAsync(_currentUserMock.Object, _userRepositoryMock.Object, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<UserSettingsResponse>>());
            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Data.Entities.User>(u => u.Settings != null), It.IsAny<CancellationToken>()), Times.Once);
        });
    }
}