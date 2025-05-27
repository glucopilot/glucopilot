using FluentValidation;
using GlucoPilot.Api.Endpoints.Pens.ListPens;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using FluentValidation.Results;
using GlucoPilot.Data.Enums;
using System.Linq.Expressions;

namespace GlucoPilot.Api.Tests.Endpoints.Pens;

[TestFixture]
public class ListPensTests
{
    private Mock<IValidator<ListPensRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Pen>> _pensRepositoryMock;
    private ListPensRequest _validRequest;
    private Guid _userId;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<ListPensRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _pensRepositoryMock = new Mock<IRepository<Pen>>();
        _userId = Guid.NewGuid();

        _validRequest = new ListPensRequest
        {
            Page = 0,
            PageSize = 2
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ListPensRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _currentUserMock
            .Setup(u => u.GetUserId())
            .Returns(_userId);
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_Pens_Response_When_Request_Is_Valid_()
    {
        var pens = new List<Pen>
        {
            new Pen
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                Created = DateTimeOffset.UtcNow,
                Model = PenModel.NovePen6,
                Colour = PenColour.Blue,
                Serial = "123",
                InsulinId = Guid.NewGuid(),
                Insulin = new Insulin { Name = "Humalog", Type = InsulinType.Bolus }
            }
        }.AsQueryable();

        _pensRepositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Pen, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(pens);

        _pensRepositoryMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Pen, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await Endpoint.HandleAsync(
            _validRequest,
            _validatorMock.Object,
            _currentUserMock.Object,
            _pensRepositoryMock.Object,
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<Ok<ListPensResponse>>());
        var okResult = result.Result as Ok<ListPensResponse>;
        Assert.That(okResult!.Value.Pens.Count, Is.EqualTo(1));
        Assert.That(okResult.Value.NumberOfPages, Is.EqualTo(1));
    }

    [Test]
    public async Task HandleAsync_Returns_ValidationProblem_When_Request_Is_Invalid_()
    {
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Page", "Page is required")
        });

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ListPensRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var result = await Endpoint.HandleAsync(
            _validRequest,
            _validatorMock.Object,
            _currentUserMock.Object,
            _pensRepositoryMock.Object,
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
    }

    [Test]
    public async Task HandleAsync_Returns_Empty_Pens_When_No_Pens_Found_()
    {
        _pensRepositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Pen, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(Enumerable.Empty<Pen>().AsQueryable());

        _pensRepositoryMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Pen, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await Endpoint.HandleAsync(
            _validRequest,
            _validatorMock.Object,
            _currentUserMock.Object,
            _pensRepositoryMock.Object,
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<Ok<ListPensResponse>>());
        var okResult = result.Result as Ok<ListPensResponse>;
        Assert.That(okResult!.Value.Pens.Count, Is.EqualTo(0));
        Assert.That(okResult.Value.NumberOfPages, Is.EqualTo(0));
    }
}
