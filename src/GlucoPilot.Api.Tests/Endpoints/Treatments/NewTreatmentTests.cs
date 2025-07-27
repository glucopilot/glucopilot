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
    private Mock<IRepository<Insulin>> _insulinRepositoryMock;
    private Mock<IRepository<Ingredient>> _ingredientRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<NewTreatmentRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _treatmentRepositoryMock = new Mock<IRepository<Treatment>>();
        _readingRepositoryMock = new Mock<IRepository<Reading>>();
        _mealRepositoryMock = new Mock<IRepository<Meal>>();
        _injectionRepositoryMock = new Mock<IRepository<Injection>>();
        _insulinRepositoryMock = new Mock<IRepository<Insulin>>();
        _ingredientRepositoryMock = new Mock<IRepository<Ingredient>>();
    }

    [Test]
    public async Task HandleAsync_Should_Created_New_Injection_When_Injection_In_Request_Is_Not_Null()
    {
        var insulin = new Insulin
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Test Insulin",
            Type = InsulinType.Bolus
        };
        var request = new NewTreatmentRequest
        {
            Injection = new NewInjection
            {
                Created = DateTimeOffset.UtcNow,
                InsulinId = insulin.Id,
                Units = 5
            }
        };
        var userId = Guid.NewGuid();

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _injectionRepositoryMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Injection, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Injection)null);
        _injectionRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Injection>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _insulinRepositoryMock.Setup(i => i.FindOneAsync(It.IsAny<Expression<Func<Insulin, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insulin);

        await Endpoint.HandleAsync(
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _treatmentRepositoryMock.Object,
            _readingRepositoryMock.Object,
            _mealRepositoryMock.Object,
            _ingredientRepositoryMock.Object,
            _injectionRepositoryMock.Object,
            _insulinRepositoryMock.Object,
            CancellationToken.None);

        _injectionRepositoryMock.Verify(r => r.AddAsync(It.Is<Injection>(i =>
            i.UserId == userId &&
            i.InsulinId == request.Injection.InsulinId &&
            i.Units == request.Injection.Units &&
            i.Created == request.Injection.Created), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void HandleAsync_Should_Throw_MEAL_NOT_FOUND_When_Meal_Is_Not_Found()
    {
        var request = new NewTreatmentRequest { Meals = [new NewTreatmentMeal { Id = Guid.NewGuid(), Quantity = 1 }] };
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
            _ingredientRepositoryMock.Object,
            _injectionRepositoryMock.Object,
            _insulinRepositoryMock.Object,
            CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("MEAL_NOT_FOUND"));
    }

    [Test]
    public void HandleAsync_Should_Throw_INGREDIENT_NOT_FOUND_When_Ingredient_Is_Not_Found()
    {
        var request = new NewTreatmentRequest { Ingredients = [new NewTreatmentIngredient { Id = Guid.NewGuid(), Quantity = 1 }] };
        var userId = Guid.NewGuid();

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _ingredientRepositoryMock.Setup(r => r.Find(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>())).Returns(new List<Ingredient>().AsQueryable());

        Assert.That(async () => await Endpoint.HandleAsync(
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _treatmentRepositoryMock.Object,
            _readingRepositoryMock.Object,
            _mealRepositoryMock.Object,
            _ingredientRepositoryMock.Object,
            _injectionRepositoryMock.Object,
            _insulinRepositoryMock.Object,
            CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("INGREDIENT_NOT_FOUND"));
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
            _ingredientRepositoryMock.Object,
            _injectionRepositoryMock.Object,
            _insulinRepositoryMock.Object,
            CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("READING_NOT_FOUND"));
    }

    [Test]
    public void HandleAsync_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
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
            _ingredientRepositoryMock.Object,
            _injectionRepositoryMock.Object,
            _insulinRepositoryMock.Object,
            CancellationToken.None), Throws.InstanceOf<UnauthorizedAccessException>());
    }

    [Test]
    public async Task HandleAsync_Should_Return_Ok_When_Treatment_Is_Created_Successfully()
    {
        var insulin = new Insulin
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Fiasp",
            Type = InsulinType.Bolus
        };
        var request = new NewTreatmentRequest
        {
            Created = DateTimeOffset.UtcNow,
            Meals = [new NewTreatmentMeal { Id = Guid.NewGuid(), Quantity = 1 }],
            Ingredients = [new NewTreatmentIngredient { Id = Guid.NewGuid(), Quantity = 1 }],
            Injection = new NewInjection { Created = DateTimeOffset.UtcNow, InsulinId = insulin.Id, Units = 5 },
            ReadingId = Guid.NewGuid()
        };
        var userId = Guid.NewGuid();

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.IsAuthenticated()).Returns(true);
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);

        _mealRepositoryMock.Setup(r => r.Find(It.IsAny<Expression<Func<Meal, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new List<Meal>() { new Meal { Id = request.Meals.First().Id, UserId = userId, Created = DateTimeOffset.UtcNow, Name = "Sugar on Toast" } }.AsQueryable());
        _ingredientRepositoryMock.Setup(r => r.Find(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new List<Ingredient>() { new Ingredient { Id = request.Ingredients.First().Id, UserId = userId, Name = "Sugar", Created = DateTimeOffset.UtcNow, Uom = UnitOfMeasurement.Grams } }.AsQueryable());
        _injectionRepositoryMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Injection, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Injection { Id = Guid.NewGuid(), UserId = userId, InsulinId = Guid.NewGuid(), Units = 5, Insulin = new Insulin() { Name = "Fiasp", Type = InsulinType.Bolus } });
        _readingRepositoryMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Reading, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Reading { Id = request.ReadingId.Value, UserId = userId, Created = DateTimeOffset.UtcNow, Direction = ReadingDirection.Steady, GlucoseLevel = 5.0 });
        _insulinRepositoryMock.Setup(i => i.FindOneAsync(It.IsAny<Expression<Func<Insulin, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insulin);
        _treatmentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Treatment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Endpoint.HandleAsync(
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _treatmentRepositoryMock.Object,
            _readingRepositoryMock.Object,
            _mealRepositoryMock.Object,
            _ingredientRepositoryMock.Object,
            _injectionRepositoryMock.Object,
            _insulinRepositoryMock.Object,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            var okResult = result.Result as Ok<NewTreatmentResponse>;
            Assert.That(okResult, Is.InstanceOf<Ok<NewTreatmentResponse>>());
            Assert.That(okResult!.Value.Meals.FirstOrDefault().Id, Is.EqualTo(request.Meals.FirstOrDefault().Id));
            Assert.That(okResult!.Value.Ingredients.FirstOrDefault().Id, Is.EqualTo(request.Ingredients.FirstOrDefault().Id));
            Assert.That(okResult.Value.ReadingId, Is.EqualTo(request.ReadingId));
            Assert.That(okResult.Value.InsulinName, Is.EqualTo("Fiasp"));
            Assert.That(okResult.Value.InsulinUnits, Is.EqualTo(5));
            Assert.That(okResult.Value.ReadingGlucoseLevel, Is.EqualTo(5.0));
        });
    }
}
