using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Insights.AverageNutrition;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Insights;

[TestFixture]
internal sealed class AverageNutritionTests
{
    private Mock<IValidator<AverageNutritionRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Treatment>> _repositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<AverageNutritionRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _repositoryMock = new Mock<IRepository<Treatment>>();
    }

    [Test]
    public async Task Handle_With_Valid_Request_Returns_Average_Nutrition_Response()
    {
        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(Guid.NewGuid());
        var nutritionData = new[]
        {
            new { Calories = 200m, Carbs = 50m, Protein = 20m, Fat = 10m },
            new { Calories = 300m, Carbs = 70m, Protein = 30m, Fat = 15m }
        }.Select(n => new Endpoint.AverageNutrition
        {
            Calories = n.Calories,
            Carbs = n.Carbs,
            Protein = n.Protein,
            Fat = n.Fat
        }).AsQueryable();

        _repositoryMock.Setup(repo => repo.FromSqlRaw<Endpoint.AverageNutrition>(
            It.IsAny<string>(),
            It.IsAny<FindOptions>(),
            It.IsAny<object[]>())).Returns(nutritionData);

        var request = new AverageNutritionRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-7),
            To = DateTimeOffset.UtcNow
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(new ValidationResult());
        
        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<AverageNutritionResponse>>());
            var response = ((Ok<AverageNutritionResponse>)result.Result).Value;
            Assert.That(response.Calories, Is.EqualTo(250));
            Assert.That(response.Carbs, Is.EqualTo(60));
            Assert.That(response.Protein, Is.EqualTo(25));
            Assert.That(response.Fat, Is.EqualTo(12.5m));
        });
    }
    
    [Test]
    public async Task Handle_With_Validation_Error_Returns_Validation_Problem()
    {
        var request = new AverageNutritionRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-7),
            To = DateTimeOffset.UtcNow
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure("From", "From date is required")
            ]));

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
    }

    [Test]
    public async Task Handle_With_No_Nutrition_Data_Returns_Zero_Averages()
    {
        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(Guid.NewGuid());
        _repositoryMock.Setup(repo => repo.FromSqlRaw<Endpoint.AverageNutrition>(
            It.IsAny<string>(),
            It.IsAny<FindOptions>(),
            It.IsAny<object[]>())).Returns(Enumerable.Empty<Endpoint.AverageNutrition>().AsQueryable());

        var request = new AverageNutritionRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-7),
            To = DateTimeOffset.UtcNow
        };
        
        _validatorMock
            .Setup(v => v.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(new ValidationResult());

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<AverageNutritionResponse>>());
            var response = ((Ok<AverageNutritionResponse>)result.Result).Value;
            Assert.That(response.Calories, Is.EqualTo(0));
            Assert.That(response.Carbs, Is.EqualTo(0));
            Assert.That(response.Protein, Is.EqualTo(0));
            Assert.That(response.Fat, Is.EqualTo(0));
        });
    }

    [Test]
    public void Handle_With_Unauthorized_User_Returns_Unauthorized_Result()
    {
        var exception = new UnauthorizedException("Test");
        _currentUserMock.Setup(cu => cu.GetUserId()).Throws(exception);

        var request = new AverageNutritionRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-7),
            To = DateTimeOffset.UtcNow
        };

        Assert.That(() => Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None), Throws.Exception.SameAs(exception));
    }

    [Test]
    public async Task Handle_With_Null_Date_Range_Uses_Default_Range()
    {
        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(Guid.NewGuid());
        var nutritionData = new[]
        {
            new { Calories = 150m, Carbs = 40m, Protein = 15m, Fat = 8m }
        }.Select(n => new Endpoint.AverageNutrition
        {
            Calories = n.Calories,
            Carbs = n.Carbs,
            Protein = n.Protein,
            Fat = n.Fat
        }).AsQueryable();

        _repositoryMock.Setup(repo => repo.FromSqlRaw<Endpoint.AverageNutrition>(
            It.IsAny<string>(),
            It.IsAny<FindOptions>(),
            It.IsAny<object[]>())).Returns(nutritionData);

        var request = new AverageNutritionRequest();
        
        _validatorMock
            .Setup(v => v.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(new ValidationResult());

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<AverageNutritionResponse>>());
            var response = ((Ok<AverageNutritionResponse>)result.Result).Value;
            Assert.That(response.Calories, Is.EqualTo(150));
            Assert.That(response.Carbs, Is.EqualTo(40));
            Assert.That(response.Protein, Is.EqualTo(15));
            Assert.That(response.Fat, Is.EqualTo(8));
        });
    }
}