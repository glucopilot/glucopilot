using FluentValidation;
using GlucoPilot.Api.Endpoints.Insulins.NewInsulin;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Tests.Endpoints.Insulins.NewInsulin
{
    [TestFixture]
    public class EndpointTests
    {
        private Mock<IValidator<NewInsulinRequest>> _validatorMock;
        private Mock<IRepository<Insulin>> _repositoryMock;
        private Mock<ICurrentUser> _currentUserMock;

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IValidator<NewInsulinRequest>>();
            _repositoryMock = new Mock<IRepository<Insulin>>();
            _currentUserMock = new Mock<ICurrentUser>();
        }

        [Test]
        public async Task HandleAsync_Should_Return_ValidationProblem_When_Request_Is_Invalid()
        {
            var request = new NewInsulinRequest { Name = "Test", Type = InsulinType.Bolus };
            _validatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult
                {
                    Errors = { new FluentValidation.Results.ValidationFailure("Name", "Name is required.") }
                });

            var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _repositoryMock.Object, _currentUserMock.Object, CancellationToken.None);

            Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
        }

        [Test]
        public void HandleAsync_Should_Throw_UnauthorizedException_When_User_Is_Not_Logged_In()
        {
            var request = new NewInsulinRequest
            {
                Name = "Test Insulin",
                Type = InsulinType.Bolus,
                Duration = 4.5,
                Scale = 1.2,
                PeakTime = 2.0
            };

            _currentUserMock
                .Setup(c => c.GetUserId())
                .Throws(new UnauthorizedException("USER_NOT_LOGGED_IN"));

            Assert.That(async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _repositoryMock.Object, _currentUserMock.Object, CancellationToken.None),
                Throws.InstanceOf<UnauthorizedException>().With.Message.EqualTo("USER_NOT_LOGGED_IN"));
        }

        [Test]
        public async Task HandleAsync_Should_Return_Ok_When_Request_Is_Valid()
        {
            var userId = Guid.NewGuid();
            _currentUserMock
                .Setup(c => c.GetUserId())
                .Returns(userId);

            var request = new NewInsulinRequest
            {
                Name = "Test Insulin",
                Type = InsulinType.Bolus,
                Duration = 4.5,
                Scale = 1.2,
                PeakTime = 2.0
            };

            _validatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _currentUserMock
                .Setup(c => c.GetUserId())
                .Returns(userId);

            var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _repositoryMock.Object, _currentUserMock.Object, CancellationToken.None);

            _repositoryMock.Verify(r => r.Add(It.Is<Insulin>(i =>
                i.Name == request.Name &&
                i.Type == request.Type &&
                i.Duration == request.Duration &&
                i.Scale == request.Scale &&
                i.UserId == userId
            )), Times.Once);

            Assert.That(result.Result, Is.TypeOf<Ok<NewInsulinResponse>>());
            var okResult = result.Result as Ok<NewInsulinResponse>;
            Assert.That(okResult!.Value.Name, Is.EqualTo(request.Name));
            Assert.That(okResult.Value.Type, Is.EqualTo(request.Type));
            Assert.That(okResult.Value.Duration, Is.EqualTo(request.Duration));
            Assert.That(okResult.Value.Scale, Is.EqualTo(request.Scale));
        }
    }
}