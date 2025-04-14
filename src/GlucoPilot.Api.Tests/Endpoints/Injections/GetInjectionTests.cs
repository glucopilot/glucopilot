using GlucoPilot.Api.Endpoints.Injections.GetInjection;
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

namespace GlucoPilot.Api.Tests.Endpoints.Injections.GetInjection
{
    [TestFixture]
    public class GetInjectionTests
    {
        private Mock<ICurrentUser> _currentUserMock;
        private Mock<IRepository<Injection>> _injectionsRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _currentUserMock = new Mock<ICurrentUser>();
            _injectionsRepositoryMock = new Mock<IRepository<Injection>>();
        }

        [Test]
        public async Task HandleAsync_Should_Return_Ok_When_Injection_Found()
        {
            var userId = Guid.NewGuid();
            var injectionId = Guid.NewGuid();
            var injection = new Injection
            {
                Id = injectionId,
                UserId = userId,
                Created = DateTimeOffset.UtcNow,
                InsulinId = Guid.NewGuid(),
                Units = 10,
                Insulin = new Insulin { Name = "Test Insulin", Type = InsulinType.Bolus }
            };

            _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
            _injectionsRepositoryMock
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Injection, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(injection);

            var result = await Endpoint.HandleAsync(injectionId, _currentUserMock.Object, _injectionsRepositoryMock.Object, CancellationToken.None);

            var okResult = result.Result as Ok<GetInjectionResponse>;
            Assert.That(okResult, Is.InstanceOf<Ok<GetInjectionResponse>>());
            Assert.That(okResult!.Value.Id, Is.EqualTo(injection.Id));
            Assert.That(okResult.Value.InsulinName, Is.EqualTo(injection.Insulin!.Name));
        }

        [Test]
        public void HandleAsync_Should_Throw_NotFoundException_When_Injection_Not_Found()
        {
            var userId = Guid.NewGuid();
            var injectionId = Guid.NewGuid();

            _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
            _injectionsRepositoryMock
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Injection, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Injection?)null);

            Assert.ThrowsAsync<NotFoundException>(async () =>
                await Endpoint.HandleAsync(injectionId, _currentUserMock.Object, _injectionsRepositoryMock.Object, CancellationToken.None));
        }

        [Test]
        public async Task HandleAsync_Should_Return_Unauthorized_When_User_Not_Authenticated()
        {
            _currentUserMock.Setup(c => c.GetUserId()).Throws<UnauthorizedAccessException>();

            Assert.That(async () => await Endpoint.HandleAsync(Guid.NewGuid(), _currentUserMock.Object, _injectionsRepositoryMock.Object, CancellationToken.None),
                Throws.TypeOf<UnauthorizedAccessException>());
        }
    }
}