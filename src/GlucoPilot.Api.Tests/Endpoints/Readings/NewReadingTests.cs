using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Readings.NewReading;
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

namespace GlucoPilot.Api.Tests.Endpoints.Readings;

[TestFixture]
public class NewReadingTests
{
    private static readonly Guid _userId = Guid.NewGuid();
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IValidator<NewReadingRequest>> _validatorMock;
    Mock<IRepository<Reading>> _repositoryMock;

    [SetUp]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(c => c.GetUserId()).Returns(_userId);
        _validatorMock = new Mock<IValidator<NewReadingRequest>>();
        _repositoryMock = new Mock<IRepository<Reading>>();
    }

    [Test]
    public async Task HandleAsync_ReturnsOkResult_WhenRequestIsValid()
    {
        var request = new NewReadingRequest
        {
            Created = DateTimeOffset.UtcNow,
            GlucoseLevel = 5.0
        };
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var reading = new Reading
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Created = request.Created,
            GlucoseLevel = request.GlucoseLevel,
            Direction = ReadingDirection.NotComputable,
        };
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Reading>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Created<NewReadingResponse>>());

        var okResult = result.Result as Created<NewReadingResponse>;
        Assert.That(okResult, Is.Not.Null);

        var response = okResult.Value;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Created, Is.EqualTo(request.Created));
        Assert.That(response.GlucoseLevel, Is.EqualTo(request.GlucoseLevel));
        Assert.That(response.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task HandleAsync_ReturnsValidationProblem_WhenRequestIsInvalid()
    {
        var request = new NewReadingRequest
        {
            Created = DateTimeOffset.UtcNow,
            GlucoseLevel = -5.0
        };

        var validationResult = new ValidationResult([
            new ValidationFailure("GlucoseLevel", "Glucose level must be greater than 0.")
        ]);
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);
        Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
    }
}
