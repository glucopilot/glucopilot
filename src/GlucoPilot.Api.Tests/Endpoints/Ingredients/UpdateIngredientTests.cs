using FluentValidation;
using GlucoPilot.Api.Endpoints.Ingredients.UpdateIngredient;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Endpoints.Ingredients;

[TestFixture]
public sealed class UpdateIngredientTests
{
    private Mock<IValidator<UpdateIngredientRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Ingredient>> _repositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<UpdateIngredientRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _repositoryMock = new Mock<IRepository<Ingredient>>();
    }

    [Test]
    public async Task HandleAsync_Returns_Validation_Problems_When_Request_Is_Invalid()
    {
        var ingredientId = Guid.NewGuid();
        var request = new UpdateIngredientRequest { Name = "", Carbs = -1, Protein = -1, Fat = -1, Calories = -1, Uom = (Models.UnitOfMeasurement)UnitOfMeasurement.Unit };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult([
                new FluentValidation.Results.ValidationFailure("Name", "Name is required"),
                new FluentValidation.Results.ValidationFailure("Carbs", "Carbs must be greater than or equal to 0"),
                new FluentValidation.Results.ValidationFailure("Protein", "Protein must be greater than or equal to 0"),
                new FluentValidation.Results.ValidationFailure("Fat", "Fat must be greater than or equal to 0"),
                new FluentValidation.Results.ValidationFailure("Calories", "Calories must be greater than or equal to 0")
            ]));

        var result = await Endpoint.HandleAsync(ingredientId, request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
        var validationProblem = (ValidationProblem)result.Result;
        Assert.That(validationProblem.ProblemDetails.Errors, Has.Count.EqualTo(5));

    }

    [Test]
    public void HandleAsync_Returns_Unauthorized_When__User_Is_Not_Authenticated()
    {
        var ingredientId = Guid.NewGuid();
        var request = new UpdateIngredientRequest { Name = "Ingredient", Carbs = 0, Protein = 0, Fat = 0, Calories = 0, Uom = (Models.UnitOfMeasurement)UnitOfMeasurement.Unit };
        _validatorMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Throws(new UnauthorizedException("USER_NOT_LOGGED_IN"));
        Assert.That(async () => await Endpoint.HandleAsync(ingredientId, request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<UnauthorizedException>().With.Message.EqualTo("USER_NOT_LOGGED_IN"));
    }

    [Test]
    public void HandleAsync_Not_Found_When_Ingredient_Not_Found()
    {
        var ingredientId = Guid.NewGuid();
        var request = new UpdateIngredientRequest { Name = "Ingredient", Carbs = 0, Protein = 0, Fat = 0, Calories = 0, Uom = (Models.UnitOfMeasurement)UnitOfMeasurement.Unit };
        _validatorMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(Guid.NewGuid());
        _repositoryMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingredient)null);

        Assert.That(async () => await Endpoint.HandleAsync(ingredientId, request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("INGREDIENT_NOT_FOUND"));
    }

    [Test]
    public async Task HandleAsync_Returns_UpdateIngredientResponse_When_Successful()
    {
        var userId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var request = new UpdateIngredientRequest { Name = "Ingredient", Carbs = 0, Protein = 0, Fat = 0, Calories = 0, Uom = (Models.UnitOfMeasurement)UnitOfMeasurement.Unit };
        var ingredient = new Ingredient { Id = ingredientId, Created = DateTimeOffset.UtcNow, UserId = userId, Name = "Old Ingredient", Carbs = 0, Protein = 0, Fat = 0, Calories = 0, Uom = UnitOfMeasurement.Unit };
        _validatorMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _repositoryMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredient);
        var result = await Endpoint.HandleAsync(ingredientId, request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<UpdateIngredientResponse>>());
            var okResult = (Ok<UpdateIngredientResponse>)result.Result;
            Assert.That(okResult.Value.Id, Is.EqualTo(ingredientId));
            Assert.That(okResult.Value.Name, Is.EqualTo(request.Name));
            Assert.That(okResult.Value.Carbs, Is.EqualTo(request.Carbs));
            Assert.That(okResult.Value.Protein, Is.EqualTo(request.Protein));
            Assert.That(okResult.Value.Fat, Is.EqualTo(request.Fat));
            Assert.That(okResult.Value.Calories, Is.EqualTo(request.Calories));
            Assert.That(okResult.Value.Uom, Is.EqualTo(request.Uom));
            Assert.That(okResult.Value.Updated, Is.EqualTo(DateTimeOffset.UtcNow).Within(TimeSpan.FromMinutes(1)));
        });

    }
}