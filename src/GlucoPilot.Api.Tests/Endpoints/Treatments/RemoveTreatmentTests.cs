using GlucoPilot.Api.Endpoints.Treatments.RemoveTreatment;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq.Expressions;

namespace GlucoPilot.Api.Tests.Endpoints.Treatments;

[TestFixture]
public class RemoveTreatmentTests
{
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Treatment>> _treatmentRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _treatmentRepositoryMock = new Mock<IRepository<Treatment>>();
    }

    [Test]
    public async Task HandleAsync_Should_Return_NoContent_When_Treatment_Deleted()
    {
        var treatmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var treatment = new Treatment { Id = treatmentId, UserId = userId };

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _treatmentRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Treatment, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(treatment);

        var result = await Endpoint.HandleAsync(treatmentId, _currentUserMock.Object, _treatmentRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NoContent>());
        _treatmentRepositoryMock.Verify(r => r.Delete(treatment), Times.Once);
    }

    [Test]
    public void HandleAsync_Should_Throw_NotFoundException_When_Treatment_Not_Found()
    {
        var treatmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _treatmentRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Treatment, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Treatment?)null);

        Assert.That(async () => await Endpoint.HandleAsync(treatmentId, _currentUserMock.Object, _treatmentRepositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("TREATMENT_NOT_FOUND"));
    }

    [Test]
    public void HandleAsync_Should_Throw_UnauthorizedHttpResult_When_User_Not_Authenticated()
    {
        var treatmentId = Guid.NewGuid();

        _currentUserMock.Setup(c => c.GetUserId()).Throws<UnauthorizedAccessException>();

        Assert.That(async () => await Endpoint.HandleAsync(treatmentId, _currentUserMock.Object, _treatmentRepositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<UnauthorizedAccessException>());
    }
}
