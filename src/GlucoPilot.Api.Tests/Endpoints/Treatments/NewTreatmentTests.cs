using FluentValidation;
using GlucoPilot.Api.Endpoints.Treatments.NewTreatment;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Endpoints.Treatments;

[TestFixture]
public class NewTreatmentTests
{
    private Mock<IValidator<NewTreatmentRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Treatment>> _treatmentRepositoryMock;
    private Mock<IRepository<Reading>> _readingRepositoryMock;
    private Mock<IRepository<Meal>> _mealRepositoryMock;
    private Mock<IRepository<Injection>> _injectionRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<NewTreatmentRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _treatmentRepositoryMock = new Mock<IRepository<Treatment>>();
        _readingRepositoryMock = new Mock<IRepository<Reading>>();
        _mealRepositoryMock = new Mock<IRepository<Meal>>();
        _injectionRepositoryMock = new Mock<IRepository<Injection>>();
    }

    [Test]
    public void HandleAsync_Should_Throw_INJECTION_NOT_FOUND_When_Injection_Is_Not_Found()
    {
        var request = new NewTreatmentRequest { InjectionId = Guid.NewGuid() };
        var userId = Guid.NewGuid();

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _injectionRepositoryMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Injection, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Injection)null);

        Assert.That(async () => await Endpoint.HandleAsync(
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _treatmentRepositoryMock.Object,
            _readingRepositoryMock.Object,
            _mealRepositoryMock.Object,
            _injectionRepositoryMock.Object,
            CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("INJECTION_NOT_FOUND"));
    }

    [Test]
    public void HandleAsync_Should_Throw_MEAL_NOT_FOUND_When_Meal_Is_Not_Found()
    {
        var request = new NewTreatmentRequest { MealId = Guid.NewGuid() };
        var userId = Guid.NewGuid();

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _mealRepositoryMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meal)null);

        Assert.That(async () => await Endpoint.HandleAsync(
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _treatmentRepositoryMock.Object,
            _readingRepositoryMock.Object,
            _mealRepositoryMock.Object,
            _injectionRepositoryMock.Object,
            CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("MEAL_NOT_FOUND"));
    }

    [Test]
    public void HandleAsync_Should_Throw_READING_NOT_FOUND_When_Reading_Is_Not_Found()
    {
        var request = new NewTreatmentRequest { ReadingId = Guid.NewGuid() };
        var userId = Guid.NewGuid();

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _readingRepositoryMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Reading, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reading)null);

        Assert.That(async () => await Endpoint.HandleAsync(
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _treatmentRepositoryMock.Object,
            _readingRepositoryMock.Object,
            _mealRepositoryMock.Object,
            _injectionRepositoryMock.Object,
            CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("READING_NOT_FOUND"));
    }

    [Test]
    public async Task HandleAsync_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        var request = new NewTreatmentRequest();
        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Throws(new UnauthorizedAccessException());

        Assert.That(async () => await Endpoint.HandleAsync(
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _treatmentRepositoryMock.Object,
            _readingRepositoryMock.Object,
            _mealRepositoryMock.Object,
            _injectionRepositoryMock.Object,
            CancellationToken.None), Throws.InstanceOf<UnauthorizedAccessException>());
    }

    [Test]
    public async Task HandleAsync_Should_Return_Ok_When_Treatment_Is_Created_Successfully()
    {
        var request = new NewTreatmentRequest
        {
            Created = DateTimeOffset.UtcNow,
            MealId = Guid.NewGuid(),
            InjectionId = Guid.NewGuid(),
            ReadingId = Guid.NewGuid()
        };
        var userId = Guid.NewGuid();

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.IsAuthenticated()).Returns(true);
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);

        _mealRepositoryMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Meal { Id = request.MealId.Value, UserId = userId, Created = DateTimeOffset.UtcNow, Name = "Sugar on Toast" });
        _injectionRepositoryMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Injection, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Injection { Id = request.InjectionId.Value, UserId = userId, InsulinId = Guid.NewGuid(), Units = 5, Insulin = new Insulin() { Name = "Fiasp", Type = InsulinType.Bolus } });
        _readingRepositoryMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Reading, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Reading { Id = request.ReadingId.Value, UserId = userId, Created = DateTimeOffset.UtcNow, Direction = ReadingDirection.Steady, GlucoseLevel = 5.0 });

        _treatmentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Treatment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Endpoint.HandleAsync(
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _treatmentRepositoryMock.Object,
            _readingRepositoryMock.Object,
            _mealRepositoryMock.Object,
            _injectionRepositoryMock.Object,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            var okResult = result.Result as Ok<NewTreatmentResponse>;
            Assert.That(okResult, Is.InstanceOf<Ok<NewTreatmentResponse>>());
            Assert.That(okResult!.Value.MealId, Is.EqualTo(request.MealId));
            Assert.That(okResult.Value.InjectionId, Is.EqualTo(request.InjectionId));
            Assert.That(okResult.Value.ReadingId, Is.EqualTo(request.ReadingId));
            Assert.That(okResult.Value.InsulinName, Is.EqualTo("Fiasp"));
            Assert.That(okResult.Value.InsulinUnits, Is.EqualTo(5));
            Assert.That(okResult.Value.MealName, Is.EqualTo("Sugar on Toast"));
            Assert.That(okResult.Value.ReadingGlucoseLevel, Is.EqualTo(5.0));
        });
    }
}
