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
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Endpoints.Readings;

[TestFixture]
internal sealed class ListTests : DatabaseTests
{
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
        currentUserMock.Setup(c => c.GetUserId()).Returns(Guid.NewGuid());

        _dbContext.Readings.Add(new Reading
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow.AddMinutes(-30),
            GlucoseLevel = 100,
            Direction = ReadingDirection.Steady
        });

        _dbContext.SaveChanges();

        var result = await List.HandleAsync(request, validatorMock.Object, currentUserMock.Object, _dbContext, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<Ok<List<ReadingsResponse>>>());
        var okResult = result.Result as Ok<List<ReadingsResponse>>;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.Not.Null);
    }
}
