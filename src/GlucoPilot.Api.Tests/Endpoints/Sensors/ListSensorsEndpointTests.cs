using FluentValidation;
using GlucoPilot.Api.Endpoints.Sensors.List;
using GlucoPilot.Api.Models;
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
using GlucoPilot.Data.Entities;
using System.Linq.Expressions;

namespace GlucoPilot.Api.Tests.Endpoints.Sensors;

[TestFixture]
public class ListSensorsEndpointTests
{
    private Mock<IValidator<PagedRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Sensor>> _sensorRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<PagedRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _sensorRepositoryMock = new Mock<IRepository<Sensor>>();
    }

    [Test]
    public async Task HandleAsync_Should_Return_ValidationProblem_When_Request_Is_Invalid()
    {
        var request = new PagedRequest { Page = 0, PageSize = 10 };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("Page", "Page is required")
            }));

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _sensorRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
    }

    [Test]
    public async Task HandleAsync_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        var request = new PagedRequest { Page = 0, PageSize = 10 };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock
            .Setup(c => c.GetUserId())
            .Throws<UnauthorizedAccessException>();

        Assert.That(async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _sensorRepositoryMock.Object, CancellationToken.None),
            Throws.InstanceOf<UnauthorizedAccessException>());
    }

    [Test]
    public async Task HandleAsync_Should_Return_Ok_With_Empty_Sensors_When_No_Sensors_Exist()
    {
        var request = new PagedRequest { Page = 0, PageSize = 10 };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(Guid.NewGuid());
        _sensorRepositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Sensor, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(Enumerable.Empty<Sensor>().AsQueryable());
        _sensorRepositoryMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Sensor, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _sensorRepositoryMock.Object, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.TypeOf<Ok<ListSensorsResponse>>());
            var response = (result.Result as Ok<ListSensorsResponse>)?.Value;
            Assert.That(response?.Sensors, Is.Empty);
            Assert.That(response?.NumberOfPages, Is.EqualTo(0));
        });

    }

    [Test]
    public async Task HandleAsync_Should_Return_Ok_With_Sensors_When_Sensors_Exist()
    {
        var request = new PagedRequest { Page = 0, PageSize = 10 };
        var userId = Guid.NewGuid();
        var sensors = new List<Sensor>
        {
            new Sensor { Id = Guid.NewGuid(), UserId = userId, SensorId = "Sensor1", Started = DateTimeOffset.UtcNow, Expires = DateTimeOffset.UtcNow.AddDays(7), Created = DateTimeOffset.UtcNow }
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock
            .Setup(c => c.GetUserId())
            .Returns(userId);
        _sensorRepositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Sensor, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(sensors.AsQueryable());
        _sensorRepositoryMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Sensor, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensors.Count);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _sensorRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<ListSensorsResponse>>());
        var response = (result.Result as Ok<ListSensorsResponse>)?.Value;
        Assert.That(response?.Sensors.Count, Is.EqualTo(sensors.Count));
        Assert.That(response?.NumberOfPages, Is.EqualTo(1));
    }
}
