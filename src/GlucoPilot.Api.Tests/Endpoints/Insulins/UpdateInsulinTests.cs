using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.Api.Endpoints.Insulins.UpdateInsulin;
using GlucoPilot.Api.Models;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Insulins;

[TestFixture]
public class UpdateInsulinTests
{
    private Mock<IValidator<UpdateInsulinRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Insulin>> _insulinRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<UpdateInsulinRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _insulinRepositoryMock = new Mock<IRepository<Insulin>>();
    }

    [Test]
    public async Task HandleAsync_Should_Return_ValidationProblem_When_Request_Is_Invalid()
    {
        var request = new UpdateInsulinRequest { Name = "", Type = InsulinType.Bolus };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult([new FluentValidation.Results.ValidationFailure("Name", "Name is required")
            ]));

        var result = await Endpoint.HandleAsync(
            Guid.NewGuid(),
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _insulinRepositoryMock.Object,
            CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
    }

    [Test]
    public void HandleAsync_Should_Return_Unauthorized_When_User_Is_Not_Logged_In()
    {
        var id = Guid.NewGuid();
        var request = new UpdateInsulinRequest { Name = "Test", Type = InsulinType.Bolus };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock
            .Setup(c => c.GetUserId())
            .Throws(new UnauthorizedAccessException("User is not logged in"));


        Assert.That(() => Endpoint.HandleAsync(id, request, _validatorMock.Object, _currentUserMock.Object, _insulinRepositoryMock.Object, CancellationToken.None),
            Throws.Exception.TypeOf<UnauthorizedAccessException>());
    }

    [Test]
    public void HandleAsync_Should_Return_NotFound_When_Insulin_Does_Not_Exist()
    {
        var id = Guid.NewGuid();
        var request = new UpdateInsulinRequest { Name = "Test", Type = InsulinType.Bolus };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(Guid.NewGuid());
        _insulinRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Insulin, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Insulin)null);


        Assert.That(() => Endpoint.HandleAsync(id, request, _validatorMock.Object, _currentUserMock.Object, _insulinRepositoryMock.Object, CancellationToken.None),
            Throws.Exception.TypeOf<NotFoundException>());
    }

    [Test]
    public async Task HandleAsync_Should_Return_Ok_When_Insulin_Is_Updated_Successfully()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateInsulinRequest { Name = "Updated Name", Type = InsulinType.Basal, Duration = 24, Scale = 1.5, PeakTime = 12 };
        var insulin = new Insulin { Id = id, UserId = userId, Name = "Old Name", Type = GlucoPilot.Data.Enums.InsulinType.Bolus };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(userId);
        _insulinRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Insulin, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insulin);
        _insulinRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Insulin>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Endpoint.HandleAsync(
            id,
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _insulinRepositoryMock.Object,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            var okResult = result.Result as Ok<UpdateInsulinResponse>;
            Assert.That(okResult, Is.TypeOf<Ok<UpdateInsulinResponse>>());
            Assert.That(okResult?.Value.Name, Is.EqualTo("Updated Name"));
            Assert.That(okResult?.Value.Type, Is.EqualTo(InsulinType.Basal));
            Assert.That(okResult?.Value.Duration, Is.EqualTo(24));
            Assert.That(okResult?.Value.Scale, Is.EqualTo(1.5));
            Assert.That(okResult?.Value.PeakTime, Is.EqualTo(12));
            Assert.That(okResult?.Value.Updated, Is.EqualTo(DateTimeOffset.UtcNow).Within(TimeSpan.FromMinutes(1)));
        });
    }
}