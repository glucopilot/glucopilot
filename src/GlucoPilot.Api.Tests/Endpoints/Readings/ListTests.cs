using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Readings;
using GlucoPilot.Api.Models;
using GlucoPilot.Data;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Endpoints.Readings;

[TestFixture]
internal sealed class ListTests : DatabaseTests
{
    private readonly Guid UserId = Guid.NewGuid();

    [Test]
    public async Task HandleAsync_ReturnsOkResult_WhenRequestIsValid()
    {
        var request = new ListReadingsRequest
        {
            from = DateTimeOffset.UtcNow.AddDays(-1),
            to = DateTimeOffset.UtcNow
        };

        var validatorMock = new Mock<IValidator<ListReadingsRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(c => c.GetUserId()).Returns(UserId);

        var reading = new Reading
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Created = DateTimeOffset.UtcNow.AddMinutes(-30),
            GlucoseLevel = 100,
            Direction = ReadingDirection.Steady
        };

        _dbContext.Readings.Add(reading);

        _dbContext.SaveChanges();

        var result = await List.HandleAsync(request, validatorMock.Object, currentUserMock.Object, _dbContext, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<Ok<List<ReadingsResponse>>>());
        var okResult = result.Result as Ok<List<ReadingsResponse>>;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.Not.Null);
        Assert.That(okResult.Value.Count, Is.EqualTo(1));

        var response = okResult.Value.First();
        Assert.That(response.Id, Is.EqualTo(reading.Id));
        Assert.That(response.UserId, Is.EqualTo(reading.UserId));
        Assert.That(response.Created, Is.EqualTo(reading.Created));
        Assert.That(response.GlucoseLevel, Is.EqualTo(reading.GlucoseLevel));
        Assert.That(response.Direction, Is.EqualTo(reading.Direction));
    }
}
