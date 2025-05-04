using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Insights.AverageGlucose;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Insights;

[TestFixture]
internal sealed class AverageGlucoseEndpointTests
{
    private Mock<IValidator<AverageGlucoseRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Reading>> _repositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<AverageGlucoseRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _repositoryMock = new Mock<IRepository<Reading>>();
    }

    [Test]
    public async Task Handle_With_Valid_Request_Returns_Average_Glucose_Level()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(userId);
        var readings = new[]
        {
            new Reading { UserId = userId, Created = DateTimeOffset.UtcNow.AddDays(-1), GlucoseLevel = 100, Direction = ReadingDirection.Steady },
            new Reading { UserId = userId, Created = DateTimeOffset.UtcNow.AddDays(-2), GlucoseLevel = 120, Direction = ReadingDirection.Steady }
        }.AsQueryable();

        _repositoryMock.Setup(repo => repo.GetAll(It.IsAny<FindOptions>())).Returns(readings);

        var request = new AverageGlucoseRequest
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
            Assert.That(result.Result, Is.InstanceOf<Ok<double>>());
            Assert.That(((Ok<double>)result.Result).Value, Is.EqualTo(110));
        });
    }
    
    [Test]
    public async Task Handle_With_Invalid_Request_Returns_ValidationProblem()
    {
        var request = new AverageGlucoseRequest { From = DateTimeOffset.UtcNow.AddDays(-7), To = DateTimeOffset.UtcNow };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("From", "From date is required") }));

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
    }

    [Test]
    public async Task Handle_With_No_Readings_Returns_Zero_Average()
    {
        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(Guid.NewGuid());
        _repositoryMock.Setup(repo => repo.GetAll(It.IsAny<FindOptions>())).Returns(Enumerable.Empty<Reading>().AsQueryable());

        var request = new AverageGlucoseRequest
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
            Assert.That(result.Result, Is.InstanceOf<Ok<double>>());
            Assert.That(((Ok<double>)result.Result).Value, Is.EqualTo(0));
        });
    }

    [Test]
    public void Handle_With_Unauthorized_User_Returns_Unauthorized_Result()
    {
        var exception = new UnauthorizedException("Unauthorized");
        _currentUserMock.Setup(cu => cu.GetUserId()).Throws(exception);

        var request = new AverageGlucoseRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-7),
            To = DateTimeOffset.UtcNow
        };

        Assert.That(() => Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object), Throws.InstanceOf<UnauthorizedException>());
    }

    [Test]
    public async Task Handle_With_Null_Date_Range_Uses_Default_Range()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(userId);
        var readings = new[]
        {
            new Reading { UserId = userId, Created = DateTimeOffset.UtcNow.AddDays(-1), GlucoseLevel = 100, Direction = ReadingDirection.Steady },
            new Reading { UserId = userId, Created = DateTimeOffset.UtcNow.AddDays(-2), GlucoseLevel = 120, Direction = ReadingDirection.Steady }
        }.AsQueryable();

        _repositoryMock.Setup(repo => repo.GetAll(It.IsAny<FindOptions>())).Returns(readings);

        var request = new AverageGlucoseRequest();
        
        _validatorMock
            .Setup(v => v.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(new ValidationResult());

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<double>>());
            Assert.That(((Ok<double>)result.Result).Value, Is.EqualTo(110));
        });
    }
}