using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Readings.List;
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

namespace GlucoPilot.Api.Tests.Endpoints.Readings;

[TestFixture]
internal sealed class ListTests
{
    private static readonly Guid _userId = Guid.NewGuid();
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IValidator<ListReadingsRequest>> _validatorMock;
    Mock<IRepository<Reading>> _repositoryMock;

    [SetUp]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(c => c.GetUserId()).Returns(_userId);
        _validatorMock = new Mock<IValidator<ListReadingsRequest>>();
        _repositoryMock = new Mock<IRepository<Reading>>();
    }

    [Test]
    public async Task HandleAsync_ReturnsOkResult_WhenRequestIsValid()
    {
        var request = new ListReadingsRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow
        };

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var reading = new Reading
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Created = DateTimeOffset.UtcNow.AddMinutes(-15),
            GlucoseLevel = 100,
            Direction = ReadingDirection.Steady
        };

        _repositoryMock.Setup(r => r.FromSqlRaw(It.IsAny<string>(), It.IsAny<FindOptions>(), It.IsAny<object[]>()))
            .Returns(new List<Reading> { reading }.AsQueryable());

        var result = await Endpoint.HandleAsync(
            request,
            _validatorMock.Object,
            _currentUserMock.Object,
            _repositoryMock.Object,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            var okResult = (Ok<List<ReadingsResponse>>)result.Result;
            Assert.That(okResult, Is.InstanceOf<Ok<List<ReadingsResponse>>>());
            Assert.That(okResult.Value, Has.Count.EqualTo(1));
            Assert.That(okResult.Value[0].UserId, Is.EqualTo(_userId));
        });
    }

    [Test]
    public async Task HandleAsync_Returns_ValidationProblem_When_Request_Is_Invalid()
    {
        var request = new ListReadingsRequest
        {
            From = DateTimeOffset.UtcNow,
            To = DateTimeOffset.UtcNow.AddDays(1)
        };

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure(nameof(ListReadingsRequest.To), "'To' must not be empty.")
            ]));

        var reading = new Reading
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Created = DateTimeOffset.UtcNow.AddMinutes(-30),
            GlucoseLevel = 100,
            Direction = ReadingDirection.Steady
        };


        _repositoryMock.Setup(r => r.Find(It.IsAny<Expression<Func<Reading, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new List<Reading> { reading }.AsQueryable());

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
        var validationProblem = result.Result as ValidationProblem;
        Assert.That(validationProblem, Is.Not.Null);
        Assert.That(validationProblem!.ProblemDetails.Errors, Contains.Key(nameof(ListReadingsRequest.To)));
    }
}
