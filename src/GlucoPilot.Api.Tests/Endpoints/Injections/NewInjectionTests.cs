using FluentValidation;
using GlucoPilot.Api.Endpoints.Injections.NewInjection;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Tests.Endpoints.Injections.NewInjection
{
    [TestFixture]
    public class NewInjectionTests
    {
        private Mock<IValidator<NewInjectionRequest>> _validatorMock;
        private Mock<ICurrentUser> _currentUserMock;
        private Mock<IRepository<Injection>> _repositoryMock;

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IValidator<NewInjectionRequest>>();
            _currentUserMock = new Mock<ICurrentUser>();
            _repositoryMock = new Mock<IRepository<Injection>>();
        }

        [Test]
        public async Task HandleAsync_Should_Return_ValidationProblem_When_Request_Is_Invalid()
        {
            var request = new NewInjectionRequest { InsulinId = Guid.NewGuid(), Units = 10 };
            _validatorMock
                .Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult
                {
                    Errors = { new FluentValidation.Results.ValidationFailure("Units", "Invalid units") }
                });

            var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

            Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
        }

        [Test]
        public async Task HandleAsync_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
        {
            var request = new NewInjectionRequest { InsulinId = Guid.NewGuid(), Units = 10 };
            _validatorMock
                .Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _currentUserMock
                .Setup(c => c.GetUserId())
                .Throws<UnauthorizedAccessException>();

            Assert.That(async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None),
                Throws.InstanceOf<UnauthorizedAccessException>());
        }

        [Test]
        public async Task HandleAsync_Should_Return_Ok_When_Request_Is_Valid()
        {
            var request = new NewInjectionRequest { InsulinId = Guid.NewGuid(), Units = 10 };
            var userId = Guid.NewGuid();

            _validatorMock
                .Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _currentUserMock
                .Setup(c => c.GetUserId())
                .Returns(userId);

            _repositoryMock
                .Setup(r => r.Add(It.IsAny<Injection>()));

            var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

            var okResult = result.Result as Ok<NewInjectionResponse>;
            Assert.That(okResult, Is.TypeOf<Ok<NewInjectionResponse>>());
            Assert.That(okResult?.Value, Is.Not.Null);
            Assert.That(okResult?.Value.InsulinId, Is.EqualTo(request.InsulinId));
            Assert.That(okResult?.Value.Units, Is.EqualTo(request.Units));
        }
    }
}