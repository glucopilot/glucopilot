using GlucoPilot.Api.Endpoints.Sensors.RemoveSensor;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Endpoints.Sensors;

[TestFixture]
public class RemoveSensorsEndpointTests
{
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Sensor>> _sensorRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _sensorRepositoryMock = new Mock<IRepository<Sensor>>();
    }

    [Test]
    public async Task HandleAsync_Should_Return_NoContent_When_Sensor_Is_Deleted()
    {
        var userId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        var sensor = new Sensor { Id = sensorId, UserId = userId, Expires = DateTimeOffset.MaxValue, Started = DateTimeOffset.MinValue, SensorId = "1234" };

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _sensorRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Sensor, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);
        _sensorRepositoryMock
            .Setup(x => x.DeleteAsync(sensor, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Endpoint.HandleAsync(sensorId, _currentUserMock.Object, _sensorRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NoContent>());
    }

    [Test]
    public void HandleAsync_Should_Throw_NotFoundException_When_Sensor_Is_Not_Found()
    {
        var userId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _sensorRepositoryMock
            .Setup(x => x.GetAll(It.IsAny<FindOptions>()))
            .Returns(Enumerable.Empty<Sensor>().AsQueryable());

        Assert.That(async () => await Endpoint.HandleAsync(sensorId, _currentUserMock.Object, _sensorRepositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("SENSOR_NOT_FOUND"));
    }

    [Test]
    public void HandleAsync_Should_Throw_UnauthorizedHttpResult_When_User_Is_Not_Authenticated()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Throws(new UnauthorizedAccessException());
        var userId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        var sensor = new Sensor { Id = sensorId, UserId = userId, Expires = DateTimeOffset.MaxValue, Started = DateTimeOffset.MinValue, SensorId = "1234" };
        _sensorRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Sensor, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        Assert.That(async () => await Endpoint.HandleAsync(Guid.NewGuid(), _currentUserMock.Object, _sensorRepositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<UnauthorizedAccessException>());
    }
}
