using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.Api.Endpoints.Injections.UpdateInjection;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Injections;

[TestFixture]
public class UpdateInjectionTests
{
    private Mock<IValidator<UpdateInjectionRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Injection>> _injectionRepositoryMock;
    private Mock<IRepository<Insulin>> _insulinRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<UpdateInjectionRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _injectionRepositoryMock = new Mock<IRepository<Injection>>();
        _insulinRepositoryMock = new Mock<IRepository<Insulin>>();
    }

    [Test]
    public async Task HandleAsync_Returns_ValidationProblem_When_Request_Is_Invalid()
    {
        var request = new UpdateInjectionRequest() { InsulinId = Guid.NewGuid(), Type = (Api.Models.InsulinType)(-1), Units = 1 };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult
            {
                Errors = { new FluentValidation.Results.ValidationFailure("Units", "Units must be greater than 0") }
            });

        var result = await Endpoint.HandleAsync(
            Guid.NewGuid(),
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _injectionRepositoryMock.Object,
            _insulinRepositoryMock.Object,
            CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
    }

    [Test]
    public void HandleAsync_Throws_NotFoundException_When_Injection_Not_Found()
    {
        var request = new UpdateInjectionRequest() { InsulinId = Guid.NewGuid(), Type = (Api.Models.InsulinType)(-1), Units = 1 };
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _injectionRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Injection, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Injection)null);

        Assert.That(() => Endpoint.HandleAsync(
                Guid.NewGuid(),
                request,
                _validatorMock.Object,
                _currentUserMock.Object,
                _injectionRepositoryMock.Object,
                _insulinRepositoryMock.Object,
                CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("INJECTION_NOT_FOUND"));
    }

    [Test]
    public void HandleAsync_Throws_NotFoundException_When_Insulin_Not_Found()
    {
        var request = new UpdateInjectionRequest() { InsulinId = Guid.NewGuid(), Type = (Api.Models.InsulinType)(-1), Units = 1 };
        var userId = Guid.NewGuid();
        var injection = new Injection
        { Id = Guid.NewGuid(), UserId = userId, InsulinId = request.InsulinId, Units = 1 };
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _injectionRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Injection, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(injection);
        _insulinRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Insulin, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Insulin)null);

        Assert.That(() => Endpoint.HandleAsync(
                Guid.NewGuid(),
                request,
                _validatorMock.Object,
                _currentUserMock.Object,
                _injectionRepositoryMock.Object,
                _insulinRepositoryMock.Object,
                CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("INSULIN_NOT_FOUND"));
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_Response_When_Successful()
    {
        var userId = Guid.NewGuid();
        var injectionId = Guid.NewGuid();
        var insulinId = Guid.NewGuid();
        var injection = new Injection { Id = injectionId, UserId = userId, Units = 10, InsulinId = insulinId };
        var insulin = new Insulin { Id = insulinId, UserId = userId, Name = "Test Insulin", Type = InsulinType.Bolus };
        var request = new UpdateInjectionRequest() { InsulinId = Guid.NewGuid(), Type = (Api.Models.InsulinType)(-1), Units = 1 };

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _injectionRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Injection, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(injection);
        _insulinRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Insulin, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(insulin);

        var result = await Endpoint.HandleAsync(
            injectionId,
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _injectionRepositoryMock.Object,
            _insulinRepositoryMock.Object,
            CancellationToken.None);

        var okResult = result.Result as Ok<UpdateInjectionResponse>;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.TypeOf<Ok<UpdateInjectionResponse>>());
            Assert.That(okResult!.Value.Id, Is.EqualTo(injectionId));
            Assert.That(okResult.Value.InsulinId, Is.EqualTo(request.InsulinId));
            Assert.That(okResult.Value.InsulinName, Is.EqualTo("Test Insulin"));
            Assert.That(okResult.Value.Units, Is.EqualTo(1));
            Assert.That(okResult.Value.Updated, Is.EqualTo(DateTimeOffset.UtcNow).Within(TimeSpan.FromMinutes(1)));
        });
    }
}