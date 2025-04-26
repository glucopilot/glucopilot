using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using GlucoPilot.Api.Endpoints.Insights.Insulin;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Insights;

[TestFixture]
internal sealed class InsulinEndpointTests
{
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Treatment>> _repositoryMock;

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _repositoryMock = new Mock<IRepository<Treatment>>();
    }

    [Test]
    public void Handle_With_Valid_User_Returns_Insulin_Insights_Response()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(userId);
        var treatments = new List<Treatment>
        {
            new Treatment
            {
                UserId = userId,
                Injection = new Injection
                {
                    InsulinId = Guid.NewGuid(),
                    Insulin = new Insulin { Id = Guid.NewGuid(), Name = "Basal", Type = InsulinType.Basal },
                    Units = 10
                }
            },
            new Treatment
            {
                UserId = userId,
                Injection = new Injection
                {
                    InsulinId = Guid.NewGuid(),
                    Insulin = new Insulin { Id = Guid.NewGuid(), Name = "Bolus", Type = InsulinType.Bolus },
                    Units = 20
                }
            }
        }.AsQueryable();

        _repositoryMock.Setup(repo => repo.Find(
            It.IsAny<Expression<Func<Treatment, bool>>>(),
            It.IsAny<FindOptions>()))
            .Returns(treatments);

        var result = Endpoint.Handle(_currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<InsulinInsightsResponse>>());
            var response = result.Result as Ok<InsulinInsightsResponse>;
            Assert.That(response.Value.Basal, Is.EqualTo(10));
            Assert.That(response.Value.Bolus, Is.EqualTo(20));
        });
    }

    [Test]
    public void Handle_With_Unauthorized_User_Returns_Unauthorized_Result()
    {
        _currentUserMock.Setup(cu => cu.GetUserId()).Throws<UnauthorizedAccessException>();

        Assert.That(() => Endpoint.Handle(_currentUserMock.Object, _repositoryMock.Object, CancellationToken.None),
            Throws.InstanceOf<UnauthorizedAccessException>());
    }

    [Test]
    public void Handle_With_No_Treatments_Returns_Zero_Units()
    {
        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(Guid.NewGuid());
        _repositoryMock.Setup(repo => repo.Find(
            It.IsAny<Expression<Func<Treatment, bool>>>(),
            It.IsAny<FindOptions>()))
            .Returns(Enumerable.Empty<Treatment>().AsQueryable());

        var result = Endpoint.Handle(_currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<InsulinInsightsResponse>>());
            var response = result.Result as Ok<InsulinInsightsResponse>; ;
            Assert.That(response.Value.Basal, Is.EqualTo(0));
            Assert.That(response.Value.Bolus, Is.EqualTo(0));
        });
    }
}