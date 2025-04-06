using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Readings;
using GlucoPilot.Api.Models;
using GlucoPilot.Data;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
    private readonly Guid UserId = Guid.NewGuid();

    [Test]
    public async Task HandleAsync_ReturnsOkResult_WhenRequestIsValid()
    {
        var request = new ListReadingsRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow
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

        var repositoryMock = new Mock<IRepository<Reading>>();
        repositoryMock.Setup(r => r.Find(It.IsAny<Expression<Func<Reading, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new List<Reading> { reading }.AsQueryable());

        var result = await List.HandleAsync(
            request,
            validatorMock.Object,
            currentUserMock.Object,
            repositoryMock.Object,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            var okResult = (Ok<List<ReadingsResponse>>)result.Result;
            Assert.That(okResult, Is.InstanceOf<Ok<List<ReadingsResponse>>>());
            Assert.That(okResult.Value, Has.Count.EqualTo(1));
            Assert.That(okResult.Value[0].UserId, Is.EqualTo(UserId));
        });
    }
}
