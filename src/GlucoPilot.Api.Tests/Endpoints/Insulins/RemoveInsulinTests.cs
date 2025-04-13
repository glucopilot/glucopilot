using GlucoPilot.Api.Endpoints.Insulins.RemoveInsulin;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Endpoints.Insulins
{
    [TestFixture]
    public class RemoveInsulinTests
    {
        private Mock<IRepository<Insulin>> _insulinRepositoryMock;
        private Mock<ICurrentUser> _currentUserMock;

        [SetUp]
        public void SetUp()
        {
            _insulinRepositoryMock = new Mock<IRepository<Insulin>>();
            _currentUserMock = new Mock<ICurrentUser>();
        }

        [Test]
        public async Task HandleAsync_Should_Return_NoContent_When_Insulin_Is_Deleted()
        {
            var insulinId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var insulin = new Insulin { Id = insulinId, Name = "Insulin", UserId = userId, Type = InsulinType.Bolus };

            _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
            _insulinRepositoryMock
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Insulin, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(insulin);
            _insulinRepositoryMock
                .Setup(r => r.DeleteAsync(insulin, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await Endpoint.HandleAsync(insulinId, _insulinRepositoryMock.Object, _currentUserMock.Object, CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<NoContent>());
        }

        [Test]
        public void HandleAsync_Should_Throw_NotFoundException_When_Insulin_Is_Not_Found()
        {
            var insulinId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
            _insulinRepositoryMock
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Insulin, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Insulin?)null);

            Assert.That(async () => await Endpoint.HandleAsync(insulinId, _insulinRepositoryMock.Object, _currentUserMock.Object, CancellationToken.None),
                Throws.TypeOf<NotFoundException>().With.Message.EqualTo("INSULIN_NOT_FOUND"));
        }

        [Test]
        public void HandleAsync_Should_Throw_UnauthorizedHttpResult_When_User_Is_Not_Authenticated()
        {
            var insulinId = Guid.NewGuid();

            _currentUserMock.Setup(c => c.GetUserId()).Throws<UnauthorizedAccessException>();

            Assert.That(async () => await Endpoint.HandleAsync(insulinId, _insulinRepositoryMock.Object, _currentUserMock.Object, CancellationToken.None),
                Throws.TypeOf<UnauthorizedAccessException>());
        }
    }
}