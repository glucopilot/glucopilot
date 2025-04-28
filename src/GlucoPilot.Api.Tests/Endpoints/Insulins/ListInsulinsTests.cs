using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Insulins.List;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Endpoints.Insulins;

[TestFixture]
public class ListInsulinsTests
{
    private Mock<IValidator<ListInsulinsRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Insulin>> _repositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<ListInsulinsRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _repositoryMock = new Mock<IRepository<Insulin>>();
    }

    [Test]
    public async Task HandleAsync_Should_Return_ValidationProblem_When_Request_Is_Invalid()
    {
        var request = new ListInsulinsRequest();
        var validationResult = new ValidationResult(
        [
            new ValidationFailure("Page", "Page is required")
        ]);

        _validatorMock
            .Setup(v => v.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
    }

    [Test]
    public void HandleAsync_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        var request = new ListInsulinsRequest();
        _validatorMock
            .Setup(v => v.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(new ValidationResult());
        _currentUserMock
            .Setup(c => c.GetUserId())
            .Throws<UnauthorizedAccessException>();

        Assert.That(() => Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None),
            Throws.InstanceOf<UnauthorizedAccessException>());
    }

    [Test]
    public async Task HandleAsync_Should_Return_Ok_With_Insulins_When_Request_Is_Valid()
    {
        var userId = Guid.NewGuid();
        var request = new ListInsulinsRequest { Page = 0, PageSize = 10 };
        var insulins = new List<Insulin>
        {
            new Insulin
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Test Insulin",
                Type = InsulinType.Bolus,
                Created = DateTimeOffset.UtcNow
            }
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(new ValidationResult());
        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(userId);
        _repositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Insulin, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(insulins.AsQueryable());
        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Insulin, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insulins.Count);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

        var okResult = result.Result as Ok<ListInsulinsResponse>;
        Assert.That(okResult, Is.TypeOf<Ok<ListInsulinsResponse>>());
        Assert.That(okResult?.Value.Insulins.Count, Is.EqualTo(insulins.Count));
    }
}