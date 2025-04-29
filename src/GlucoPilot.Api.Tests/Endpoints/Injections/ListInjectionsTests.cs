using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.Api.Endpoints.Injections.ListInjections;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Injections;

[TestFixture]
public class ListInjectionsTests
{
    private Mock<IValidator<ListInjectionsRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Injection>> _repositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<ListInjectionsRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _repositoryMock = new Mock<IRepository<Injection>>();
    }

    [Test]
    public async Task HandleAsync_Should_Return_ValidationProblem_When_Request_Is_Invalid()
    {
        var request = new ListInjectionsRequest { Page = 0, PageSize = 10 };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult([
                new FluentValidation.Results.ValidationFailure("Page", "Page must be greater than 0")
            ]));

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object,
            _repositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
    }

    [Test]
    public void HandleAsync_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        var request = new ListInjectionsRequest { Page = 0, PageSize = 10 };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock
            .Setup(c => c.GetUserId())
            .Throws<UnauthorizedAccessException>();

        Assert.That(
            () => Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object,
                _repositoryMock.Object, CancellationToken.None),
            Throws.InstanceOf<UnauthorizedAccessException>());
    }

    [Test]
    public async Task HandleAsync_Should_Return_Ok_With_Correct_Response_When_Request_Is_Valid()
    {
        var userId = Guid.NewGuid();
        var request = new ListInjectionsRequest { Page = 0, PageSize = 2 };
        var injections = new List<Injection>
        {
            new Injection
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Created = DateTimeOffset.UtcNow,
                InsulinId = Guid.NewGuid(),
                Insulin = new Insulin { Name = "Insulin A", Type = InsulinType.Bolus },
                Units = 10
            },
            new Injection
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Created = DateTimeOffset.UtcNow.AddMinutes(-10),
                InsulinId = Guid.NewGuid(),
                Insulin = new Insulin { Name = "Insulin B", Type = InsulinType.Basal },
                Units = 15
            }
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(userId);
        _repositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Injection, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(injections.AsQueryable());
        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Injection, bool>>>(), CancellationToken.None))
            .ReturnsAsync(2);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object,
            _repositoryMock.Object, CancellationToken.None);

        var okResult = result.Result as Ok<ListInjectionsResponse>;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.TypeOf<Ok<ListInjectionsResponse>>());
            Assert.That(okResult?.Value.NumberOfPages, Is.EqualTo(1));
            Assert.That(okResult?.Value.Injections.Count, Is.EqualTo(2));
        });
    }
}