using FluentValidation;
using GlucoPilot.Api.Endpoints.Ingredients.NewIngredient;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Tests.Endpoints.Ingredients.NewIngredient;

[TestFixture]
public class EndpointTests
{
    private Mock<IValidator<NewIngredientRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Ingredient>> _ingredientRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<NewIngredientRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _ingredientRepositoryMock = new Mock<IRepository<Ingredient>>();
    }

    [Test]
    public async Task HandleAsync_Returns_ValidationProblem_When_Request_Is_Invalid()
    {
        var request = new NewIngredientRequest { Name = "Test", Carbs = 10, Protein = 5, Fat = 2, Calories = 100, Uom = UnitOfMeasurement.Grams };
        var validationResult = new FluentValidation.Results.ValidationResult(new[] { new FluentValidation.Results.ValidationFailure("Name", "Name is required") });

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _ingredientRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_Response_When_Request_Is_Valid()
    {
        var request = new NewIngredientRequest { Name = "Test", Carbs = 10, Protein = 5, Fat = 2, Calories = 100, Uom = UnitOfMeasurement.Grams };
        var validationResult = new FluentValidation.Results.ValidationResult();
        var userId = Guid.NewGuid();

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(userId);

        _ingredientRepositoryMock.Setup(r => r.Add(It.IsAny<Ingredient>()));

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _ingredientRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<NewIngredientResponse>>());
        var okResult = result.Result as Ok<NewIngredientResponse>;
        Assert.That(okResult!.Value.Name, Is.EqualTo(request.Name));
        Assert.That(okResult.Value.Carbs, Is.EqualTo(request.Carbs));
        Assert.That(okResult.Value.Protein, Is.EqualTo(request.Protein));
        Assert.That(okResult.Value.Fat, Is.EqualTo(request.Fat));
        Assert.That(okResult.Value.Calories, Is.EqualTo(request.Calories));
        Assert.That(okResult.Value.Uom, Is.EqualTo(request.Uom));
    }
}