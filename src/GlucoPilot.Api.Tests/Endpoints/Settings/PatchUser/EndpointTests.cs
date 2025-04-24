using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Api.Endpoints.Settings.PatchUser;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Settings.PatchUser;

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
    public async Task HandleAsync_With_Valid_Request_Updates_User_Settings_And_Returns_NoContent()
    {
        var userId = Guid.NewGuid();
        var user = new Patient
        {
            Id = userId,
            Email = "test@nomail.com",
            PasswordHash = "password",
            Settings = new UserSettings()
        };
        var request = new UserSettingsPatchRequest
        {
            GlucoseUnitOfMeasurement = GlucoseUnitOfMeasurement.MgDl,
            LowSugarThreshold = 70,
            HighSugarThreshold = 180,
            DailyCalorieTarget = 2000,
            DailyCarbTarget = 250,
            DailyProteinTarget = 100,
            DailyFatTarget = 70
        };

        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<Data.Entities.User, bool>>>(),
                It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Data.Entities.User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Endpoint.HandleAsync(_currentUserMock.Object, _userRepositoryMock.Object, request,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<NoContent>());
            Assert.That(user.Settings.GlucoseUnitOfMeasurement, Is.EqualTo((Data.Enums.GlucoseUnitOfMeasurement)1));
            Assert.That(user.Settings.LowSugarThreshold, Is.EqualTo(70));
            Assert.That(user.Settings.HighSugarThreshold, Is.EqualTo(180));
            Assert.That(user.Settings.DailyCalorieTarget, Is.EqualTo(2000));
            Assert.That(user.Settings.DailyCarbTarget, Is.EqualTo(250));
            Assert.That(user.Settings.DailyProteinTarget, Is.EqualTo(100));
            Assert.That(user.Settings.DailyFatTarget, Is.EqualTo(70));
        });
    }

    [Test]
    public void HandleAsync_With_Null_User_Throws_Unauthorized_Exception()
    {
        var userId = Guid.NewGuid();
        var request = new UserSettingsPatchRequest();

        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<Data.Entities.User, bool>>>(),
                It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Data.Entities.User)null);

        Assert.That(() =>
                Endpoint.HandleAsync(_currentUserMock.Object, _userRepositoryMock.Object, request,
                    CancellationToken.None),
            Throws.InstanceOf<UnauthorizedException>().With.Message.EqualTo("USER_NOT_LOGGED_IN"));
    }

    [Test]
    public async Task HandleAsync_With_Null_User_Settings_Creates_Default_Settings_And_Returns_NoContent()
    {
        var userId = Guid.NewGuid();
        var user = new Patient
        {
            Id = userId,
            Email = "test@nomail.com",
            PasswordHash = "password",
            Settings = null
        };
        var request = new UserSettingsPatchRequest();

        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<Data.Entities.User, bool>>>(),
                It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Data.Entities.User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Endpoint.HandleAsync(_currentUserMock.Object, _userRepositoryMock.Object, request,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<NoContent>());
            Assert.That(user.Settings, Is.Not.Null);
        });
    }
}