using GlucoPilot.Api.Endpoints.Injections.RemoveInjection;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Endpoints.Injections.RemoveInjection.Tests;

[TestFixture]
public class RemoveInjectionTests
{
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Injection>> _repositoryMock;

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _repositoryMock = new Mock<IRepository<Injection>>();
    }

    [Test]
    public async Task HandleAsync_Should_Return_No_Content_When_Deletion_Is_Successful()
    {
        var userId = Guid.NewGuid();
        var injectionId = Guid.NewGuid();
        var injection = new Injection { Id = injectionId, UserId = userId, InsulinId = Guid.NewGuid(), Units = 10 };

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _repositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Injection, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(injection);

        var result = await Endpoint.HandleAsync(injectionId, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<NoContent>());
        _repositoryMock.Verify(r => r.DeleteAsync(injection, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void HandleAsync_Should_Throw_Not_Found_Exception_When_Injection_Does_Not_Exist()
    {
        var userId = Guid.NewGuid();
        var injectionId = Guid.NewGuid();

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _repositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Injection, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Injection?)null);

        Assert.That(async () => await Endpoint.HandleAsync(injectionId, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("INJECTION_NOT_FOUND"));
    }

    [Test]
    public void HandleAsync_Should_Throw_Unauthorized_Exception_When_User_Is_Not_Authenticated()
    {
        _currentUserMock.Setup(c => c.GetUserId()).Throws<UnauthorizedAccessException>();

        Assert.That(async () => await Endpoint.HandleAsync(Guid.NewGuid(), _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<UnauthorizedAccessException>());
    }
}