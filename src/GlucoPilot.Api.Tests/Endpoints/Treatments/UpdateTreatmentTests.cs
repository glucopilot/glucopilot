using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Treatments.UpdateTreatment;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static GlucoPilot.Api.Endpoints.Treatments.UpdateTreatment.UpdateTreatmentRequest;

namespace GlucoPilot.Api.Tests.Endpoints.Treatments;

[TestFixture]
public class UpdateTreatmentTests
{
    private Mock<IValidator<UpdateTreatmentRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Treatment>> _treatmentRepositoryMock;
    private Mock<IRepository<Reading>> _readingRepositoryMock;
    private Mock<IRepository<Meal>> _mealRepositoryMock;
    private Mock<IRepository<Ingredient>> _ingredientRepositoryMock;
    private Mock<IRepository<Injection>> _injectionRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<UpdateTreatmentRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _treatmentRepositoryMock = new Mock<IRepository<Treatment>>();
        _readingRepositoryMock = new Mock<IRepository<Reading>>();
        _mealRepositoryMock = new Mock<IRepository<Meal>>();
        _ingredientRepositoryMock = new Mock<IRepository<Ingredient>>();
        _injectionRepositoryMock = new Mock<IRepository<Injection>>();
    }

    [Test]
    public void HandleAsync_Should_Throw_NotFoundException_When_Treatment_Not_Found()
    {
        var id = Guid.NewGuid();
        var request = new UpdateTreatmentRequest();
        var userId = Guid.NewGuid();

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(userId);

        _treatmentRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Treatment, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Treatment)null);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await Endpoint.HandleAsync(
                id,
                request,
                _validatorMock.Object,
                _currentUserMock.Object,
                _treatmentRepositoryMock.Object,
                _readingRepositoryMock.Object,
                _mealRepositoryMock.Object,
                _ingredientRepositoryMock.Object,
                _injectionRepositoryMock.Object,
                CancellationToken.None));
    }

    [Test]
    public void HandleAsync_Should_Throw_NotFoundException_When_Reading_Not_Found()
    {
        var id = Guid.NewGuid();
        var request = new UpdateTreatmentRequest { ReadingId = Guid.NewGuid() };
        var userId = Guid.NewGuid();
        var treatment = new Treatment { Id = id, UserId = userId };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(userId);

        _treatmentRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Treatment, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(treatment);

        _readingRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Reading, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reading)null);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await Endpoint.HandleAsync(
                id,
                request,
                _validatorMock.Object,
                _currentUserMock.Object,
                _treatmentRepositoryMock.Object,
                _readingRepositoryMock.Object,
                _mealRepositoryMock.Object,
                _ingredientRepositoryMock.Object,
                _injectionRepositoryMock.Object,
                CancellationToken.None));
    }

    [Test]
    public void HandleAsync_Should_Throw_NotFoundException_When_Meal_Not_Found()
    {
        var id = Guid.NewGuid();
        var request = new UpdateTreatmentRequest { Meals = [new UpdateTreatmentMealRequest { Id = Guid.NewGuid(), Quantity = 1 }] };
        var userId = Guid.NewGuid();
        var treatment = new Treatment { Id = id, UserId = userId };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(userId);

        _treatmentRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Treatment, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(treatment);

        _mealRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meal)null);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await Endpoint.HandleAsync(
                id,
                request,
                _validatorMock.Object,
                _currentUserMock.Object,
                _treatmentRepositoryMock.Object,
                _readingRepositoryMock.Object,
                _mealRepositoryMock.Object,
                _ingredientRepositoryMock.Object,
                _injectionRepositoryMock.Object,
                CancellationToken.None));
    }

    [Test]
    public void HandleAsync_Should_Throw_NotFoundException_When_Injection_Not_Found()
    {
        var id = Guid.NewGuid();
        var request = new UpdateTreatmentRequest { InjectionId = Guid.NewGuid() };
        var userId = Guid.NewGuid();
        var treatment = new Treatment { Id = id, UserId = userId };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(userId);

        _treatmentRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Treatment, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(treatment);

        _injectionRepositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Injection, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new[] { (Injection)null }.AsQueryable());

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await Endpoint.HandleAsync(
                id,
                request,
                _validatorMock.Object,
                _currentUserMock.Object,
                _treatmentRepositoryMock.Object,
                _readingRepositoryMock.Object,
                _mealRepositoryMock.Object,
                _ingredientRepositoryMock.Object,
                _injectionRepositoryMock.Object,
                CancellationToken.None));
    }

    [Test]
    public async Task HandleAsync_Should_Return_Ok_When_Treatment_Updated_Successfully()
    {
        var id = Guid.NewGuid();
        var request = new UpdateTreatmentRequest();
        var userId = Guid.NewGuid();
        var treatment = new Treatment { Id = id, UserId = userId };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(userId);

        _treatmentRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Treatment, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(treatment);

        _treatmentRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Treatment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Endpoint.HandleAsync(
            id,
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _treatmentRepositoryMock.Object,
            _readingRepositoryMock.Object,
            _mealRepositoryMock.Object,
            _ingredientRepositoryMock.Object,
            _injectionRepositoryMock.Object,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<UpdateTreatmentResponse>>());
            var okResult = (Ok<UpdateTreatmentResponse>)result.Result;
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value.Id, Is.EqualTo(id));
            Assert.That(okResult.Value.Updated, Is.EqualTo(DateTimeOffset.UtcNow).Within(TimeSpan.FromMinutes(1)));
        });
    }

    [Test]
    public async Task HandleAsync_Should_Return_ValidationProblem_When_Request_Is_Invalid()
    {
        var id = Guid.NewGuid();
        var request = new UpdateTreatmentRequest();
        var userId = Guid.NewGuid();

        var validationResult = new ValidationResult([
            new ValidationFailure("", "Validation error message")
        ]);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(userId);

        var result = await Endpoint.HandleAsync(
            id,
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _treatmentRepositoryMock.Object,
            _readingRepositoryMock.Object,
            _mealRepositoryMock.Object,
            _ingredientRepositoryMock.Object,
            _injectionRepositoryMock.Object,
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
    }

    [Test]
    public void HandleAsync_Should_Throw_Exception_When_User_Is_Not_Authenticated()
    {
        var id = Guid.NewGuid();
        var request = new UpdateTreatmentRequest();

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _currentUserMock
            .Setup(c => c.GetUserId())
            .Throws(new UnauthorizedAccessException("User is not authenticated"));

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await Endpoint.HandleAsync(
                id,
                request,
                _validatorMock.Object,
                _currentUserMock.Object,
                _treatmentRepositoryMock.Object,
                _readingRepositoryMock.Object,
                _mealRepositoryMock.Object,
                _ingredientRepositoryMock.Object,
                _injectionRepositoryMock.Object,
                CancellationToken.None));
    }
}
