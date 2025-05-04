using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
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
    private Mock<IValidator<InsulinInsightsRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Treatment>> _repositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<InsulinInsightsRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _repositoryMock = new Mock<IRepository<Treatment>>();
    }

    [Test]
    public async Task Handle_With_Valid_User_Returns_Insulin_Insights_Response()
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

        var request = new InsulinInsightsRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow
        };
        
        _validatorMock
            .Setup(v => v.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(new ValidationResult());

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<InsulinInsightsResponse>>());
            var response = result.Result as Ok<InsulinInsightsResponse>;
            Assert.That(response.Value.TotalBasalUnits, Is.EqualTo(10));
            Assert.That(response.Value.TotalBolusUnits, Is.EqualTo(20));
        });
    }
    
    [Test]
    public async Task Handle_With_Validation_Error_Returns_Validation_Problem()
    {
        var request = new InsulinInsightsRequest
        {
            From = DateTimeOffset.UtcNow,
            To = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.From), "From date must be less than To date.")
            }));

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
    }

    [Test]
    public void Handle_With_Unauthorized_User_Returns_Unauthorized_Result()
    {
        _currentUserMock.Setup(cu => cu.GetUserId()).Throws<UnauthorizedAccessException>();
        var request = new InsulinInsightsRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow
        };

        Assert.That(() => Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None),
            Throws.InstanceOf<UnauthorizedAccessException>());
    }

    [Test]
    public async Task Handle_With_No_Treatments_Returns_Zero_Units()
    {
        _currentUserMock.Setup(cu => cu.GetUserId()).Returns(Guid.NewGuid());
        _repositoryMock.Setup(repo => repo.Find(
            It.IsAny<Expression<Func<Treatment, bool>>>(),
            It.IsAny<FindOptions>()))
            .Returns(Enumerable.Empty<Treatment>().AsQueryable());

        var request = new InsulinInsightsRequest
        {
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow
        };
        
        _validatorMock
            .Setup(v => v.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(new ValidationResult());

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<InsulinInsightsResponse>>());
            var response = result.Result as Ok<InsulinInsightsResponse>; ;
            Assert.That(response.Value.TotalBasalUnits, Is.EqualTo(0));
            Assert.That(response.Value.TotalBolusUnits, Is.EqualTo(0));
        });
    }
}