using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Insights.TimeInRange;
using GlucoPilot.Api.Models;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Endpoints.Insights;

[TestFixture]
internal sealed class TimeInRangeEndpointTests
{
    private Mock<IValidator<TimeInRangeRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<GlucoseRange>> _repositoryMock;
    private CancellationToken _cancellationToken;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<TimeInRangeRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _repositoryMock = new Mock<IRepository<GlucoseRange>>();
        _cancellationToken = CancellationToken.None;
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_RangeMin_And_RangeMax_Values_()
    {
        var userId = Guid.NewGuid();
        var request = new TimeInRangeRequest { From = DateTimeOffset.UtcNow.AddDays(-1), To = DateTimeOffset.UtcNow };
        var validationResult = new ValidationResult();
        var glucoseRanges = new List<GlucoseRange>
        {
            new GlucoseRange { RangeId = 0, TotalMinutes = 10, Percentage = 5.0m, RangeMin = 0, RangeMax = 70 },
            new GlucoseRange { RangeId = 1, TotalMinutes = 100, Percentage = 50.0m, RangeMin = 70, RangeMax = 140 },
            new GlucoseRange { RangeId = 2, TotalMinutes = 60, Percentage = 30.0m, RangeMin = 140, RangeMax = 180 },
            new GlucoseRange { RangeId = 3, TotalMinutes = 30, Percentage = 15.0m, RangeMin = 180, RangeMax = 9999 }
        };

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _validatorMock.Setup(x => x.ValidateAsync(request, _cancellationToken)).ReturnsAsync(validationResult);
        _repositoryMock.Setup(x => x.FromSqlRaw<GlucoseRange>(It.IsAny<string>(), It.IsAny<FindOptions>(), userId, request.From, request.To, It.IsAny<double>(), It.IsAny<double>()))
            .Returns(glucoseRanges.AsQueryable());

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, _cancellationToken);

        Assert.That(result.Result, Is.InstanceOf<Ok<TimeInRangeResponse>>());
        var okResult = (Ok<TimeInRangeResponse>)result.Result;
        Assert.That(okResult.Value.Ranges.Count, Is.EqualTo(4));
        Assert.That(okResult.Value.Ranges.First().RangeMin, Is.EqualTo(0));
        Assert.That(okResult.Value.Ranges.First().RangeMax, Is.EqualTo(70));
        Assert.That(okResult.Value.Ranges.Last().RangeMin, Is.EqualTo(180));
        Assert.That(okResult.Value.Ranges.Last().RangeMax, Is.EqualTo(9999));
    }

    [Test]
    public async Task HandleAsync_Returns_ValidationProblem_When_Request_Is_Invalid_()
    {
        var userId = Guid.NewGuid();
        var request = new TimeInRangeRequest();
        var validationResult = new ValidationResult(new[] { new ValidationFailure("From", "Required") });

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _validatorMock.Setup(x => x.ValidateAsync(request, _cancellationToken)).ReturnsAsync(validationResult);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, _cancellationToken);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
    }

    [Test]
    public async Task HandleAsync_Uses_Current_Date_When_To_Is_Null_()
    {
        var userId = Guid.NewGuid();
        var request = new TimeInRangeRequest { From = null, To = null };
        var validationResult = new ValidationResult();
        var glucoseRanges = new List<GlucoseRange>();

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<TimeInRangeRequest>(), _cancellationToken)).ReturnsAsync(validationResult);
        _repositoryMock.Setup(repo => repo.FromSqlRaw<GlucoseRange>(
             It.IsAny<string>(),
             It.IsAny<FindOptions>(),
             It.IsAny<object[]>())).Returns(glucoseRanges.AsQueryable());

        var before = DateTimeOffset.UtcNow;
        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, _cancellationToken).ConfigureAwait(false);
        var after = DateTimeOffset.UtcNow;

        Assert.That(result.Result, Is.InstanceOf<Ok<TimeInRangeResponse>>());
        var okResult = (Ok<TimeInRangeResponse>)result.Result;
        Assert.That(okResult.Value.To, Is.InRange(before, after));
        Assert.That(okResult.Value.From.Date, Is.EqualTo(okResult.Value.To.Date));
    }
}
