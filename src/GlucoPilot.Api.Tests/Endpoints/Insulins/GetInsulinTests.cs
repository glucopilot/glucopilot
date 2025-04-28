using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Api.Endpoints.Insulins.GetInsulin;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Insulins;

[TestFixture]
public class GetInsulinTests
{
    private Mock<IRepository<Insulin>> _insulinRepositoryMock;
    private Mock<ICurrentUser> _currentUserMock;

    [SetUp]
    public void SetUp()
    {
        _insulinRepositoryMock = new Mock<IRepository<Insulin>>();
        _currentUserMock = new Mock<ICurrentUser>();
    }

    [Test]
    public async Task HandleAsync_Should_Return_Ok_When_Insulin_Found()
    {
        var insulinId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var insulin = new Insulin
        {
            Id = insulinId,
            UserId = userId,
            Name = "Test Insulin",
            Type = InsulinType.Bolus,
            Duration = 4.5,
            Scale = 1.2,
            PeakTime = 2.0,
            Created = DateTimeOffset.UtcNow
        };

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _insulinRepositoryMock
            .Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<Insulin, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(insulin);

        var result = await Endpoint.HandleAsync(insulinId, _currentUserMock.Object, _insulinRepositoryMock.Object,
            CancellationToken.None);

        var okResult = result.Result as Ok<GetInsulinResponse>;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.TypeOf<Ok<GetInsulinResponse>>());
            Assert.That(okResult!.Value.Id, Is.EqualTo(insulinId));
            Assert.That(okResult!.Value.Name, Is.EqualTo("Test Insulin"));
        });
    }

    [Test]
    public void HandleAsync_Should_Throw_NotFoundException_When_Insulin_Not_Found()
    {
        var insulinId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _insulinRepositoryMock
            .Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<Insulin, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Insulin)null);

        Assert.That(
            async () => await Endpoint.HandleAsync(insulinId, _currentUserMock.Object, _insulinRepositoryMock.Object,
                CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("INSULIN_NOT_FOUND"));
    }

    [Test]
    public void HandleAsync_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        var insulinId = Guid.NewGuid();
        _currentUserMock
            .Setup(c => c.GetUserId())
            .Throws<UnauthorizedAccessException>();

        Assert.That(
            async () => await Endpoint.HandleAsync(insulinId, _currentUserMock.Object, _insulinRepositoryMock.Object,
                CancellationToken.None),
            Throws.InstanceOf<UnauthorizedAccessException>());
    }
}