using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Injections.NewInjection;
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

namespace GlucoPilot.Tests.Endpoints.Injections.NewInjection
{
    [TestFixture]
    public class NewInjectionTests
    {
        private Mock<IValidator<NewInjectionRequest>> _validatorMock;
        private Mock<ICurrentUser> _currentUserMock;
        private Mock<IRepository<Injection>> _injectionsRepositoryMock;
        private Mock<IRepository<Insulin>> _insulinRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IValidator<NewInjectionRequest>>();
            _currentUserMock = new Mock<ICurrentUser>();
            _injectionsRepositoryMock = new Mock<IRepository<Injection>>();
            _insulinRepositoryMock = new Mock<IRepository<Insulin>>();
        }

        [Test]
        public async Task HandleAsync_Should_Return_ValidationProblem_When_Request_Is_Invalid()
        {
            var request = new NewInjectionRequest { InsulinId = Guid.NewGuid(), Units = 10, Created = DateTimeOffset.UtcNow };
            _validatorMock
                .Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new ValidationResult
                {
                    Errors = { new ValidationFailure("Units", "Invalid units") }
                });

            var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _injectionsRepositoryMock.Object, _insulinRepositoryMock.Object, CancellationToken.None);

            Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
        }

        [Test]
        public async Task HandleAsync_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
        {
            var request = new NewInjectionRequest { InsulinId = Guid.NewGuid(), Units = 10, Created = DateTimeOffset.UtcNow };
            _validatorMock
                .Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new ValidationResult());
            _currentUserMock
                .Setup(c => c.GetUserId())
                .Throws<UnauthorizedAccessException>();

            Assert.That(async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _injectionsRepositoryMock.Object, _insulinRepositoryMock.Object, CancellationToken.None),
                Throws.InstanceOf<UnauthorizedAccessException>());
        }

        [Test]
        public async Task HandleAsync_Should_Return_Ok_When_Request_Is_Valid()
        {
            var request = new NewInjectionRequest { InsulinId = Guid.NewGuid(), Units = 10, Created = DateTimeOffset.UtcNow };
            var userId = Guid.NewGuid();

            _validatorMock
                .Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new ValidationResult());
            _currentUserMock
                .Setup(c => c.GetUserId())
                .Returns(userId);

            _injectionsRepositoryMock
                .Setup(r => r.Add(It.IsAny<Injection>()));

            _insulinRepositoryMock
                .Setup(r => r.FindOneAsync(
                    It.IsAny<Expression<Func<Insulin, bool>>>(),
                    It.IsAny<FindOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Insulin
                {
                    Id = request.InsulinId,
                    UserId = userId,
                    Name = "Test Insulin",
                    Type = InsulinType.Bolus,
                });

            var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _injectionsRepositoryMock.Object, _insulinRepositoryMock.Object, CancellationToken.None);

            var okResult = result.Result as Ok<NewInjectionResponse>;
            Assert.Multiple(() =>
            {
                Assert.That(okResult, Is.TypeOf<Ok<NewInjectionResponse>>());
                Assert.That(okResult?.Value, Is.Not.Null);
                Assert.That(okResult?.Value.InsulinId, Is.EqualTo(request.InsulinId));
                Assert.That(okResult?.Value.Units, Is.EqualTo(request.Units));
                Assert.That(okResult?.Value.Created, Is.EqualTo(request.Created));
            });
        }

        [Test]
        public void HandleAsync_Should_Throw_NotFoundException_When_Insulin_Is_Not_Found()
        {
            var request = new NewInjectionRequest
            {
                InsulinId = Guid.NewGuid(),
                Units = 10,
                Created = DateTimeOffset.UtcNow
            };

            var userId = Guid.NewGuid();

            _validatorMock
                .Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new ValidationResult());

            _currentUserMock
                .Setup(c => c.GetUserId())
                .Returns(userId);

            _insulinRepositoryMock
                .Setup(r => r.FindOneAsync(
                    It.IsAny<Expression<Func<Insulin, bool>>>(),
                    It.IsAny<FindOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Insulin?)null);

            Assert.That(async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _injectionsRepositoryMock.Object, _insulinRepositoryMock.Object, CancellationToken.None),
                Throws.TypeOf<NotFoundException>().With.Message.EqualTo("INSULIN_NOT_FOUND"));
        }
    }
}