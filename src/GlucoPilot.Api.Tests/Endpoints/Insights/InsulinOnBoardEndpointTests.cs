using GlucoPilot.Api.Endpoints.Insights.InsulinOnBoard;
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
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Endpoints.Insights;

[TestFixture]
public class InsulinOnBoardEndpointTests
{
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Treatment>> _treatmentRepositoryMock;

    private readonly Guid userId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _treatmentRepositoryMock = new Mock<IRepository<Treatment>>();
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_Valid_Treatments()
    {
        var insulin = new Insulin
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow.AddHours(-1),
            Duration = 2,
            PeakTime = 1,
            Scale = 1,
            Name = "TestInsulin",
            Type = InsulinType.Bolus
        };

        var injection = new Injection
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow.AddHours(-1),
            Insulin = insulin,
            InsulinId = insulin.Id,
            Units = 5,
            UserId = userId
        };

        var treatment = new Treatment
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow.AddHours(-1),
            Injection = injection,
            UserId = userId
        };

        var treatments = new List<Treatment> { treatment }.AsQueryable();

        var treatmentRepoMock = new Mock<IRepository<Treatment>>();
        treatmentRepoMock.Setup(x => x.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Treatment, bool>>>(), null))
            .Returns(treatments);

        var insulinRepoMock = new Mock<IRepository<Insulin>>();

        var result = await Endpoint.HandleAsync(
            _currentUserMock.Object,
            treatmentRepoMock.Object,
            insulinRepoMock.Object,
            CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<InsulinOnBoardResponse>>());
        var okResult = result.Result as Ok<InsulinOnBoardResponse>;
        Assert.That(okResult!.Value.Treatments.Count, Is.EqualTo(1));
        Assert.That(okResult.Value.Treatments.First().Injection.Units, Is.EqualTo(5));
        Assert.That(okResult.Value.Treatments.First().Injection.Insulin.Duration, Is.EqualTo(2));
        Assert.That(okResult.Value.Treatments.First().Injection.Insulin.PeakTime, Is.EqualTo(1));
        Assert.That(okResult.Value.Treatments.First().Injection.Insulin.Scale, Is.EqualTo(1));
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_Empty_Treatments_When_None_Match()
    {
        _treatmentRepositoryMock.Setup(x => x.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Treatment, bool>>>(), null))
            .Returns(new List<Treatment>().AsQueryable());

        var insulinRepoMock = new Mock<IRepository<Insulin>>();

        var result = await Endpoint.HandleAsync(
            _currentUserMock.Object,
            _treatmentRepositoryMock.Object,
            insulinRepoMock.Object,
            CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<InsulinOnBoardResponse>>());
        var okResult = result.Result as Ok<InsulinOnBoardResponse>;
        Assert.That(okResult!.Value.Treatments.Count, Is.EqualTo(0));
    }
}
