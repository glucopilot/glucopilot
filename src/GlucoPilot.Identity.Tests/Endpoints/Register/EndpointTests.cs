using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Identity.Endpoints.Register;
using GlucoPilot.Identity.Models;
using GlucoPilot.Identity.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Identity.Tests.Endpoints.Register
{
    [TestFixture]
    internal sealed class EndpointTests
    {
        [Test]
        public async Task HandleAsync_WithValidRequest_ReturnsOkResult()
        {
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "password",
                ConfirmPassword = "password",
                AcceptedTerms = true
            };
            var response = new RegisterResponse
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Created = DateTimeOffset.UtcNow,
                AcceptedTerms = true
            };
            var validatorMock = new Mock<IValidator<RegisterRequest>>();
            validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(us => us.RegisterAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await Endpoint.HandleAsync(request, validatorMock.Object, userServiceMock.Object, CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<Ok<RegisterResponse>>());
            Assert.That((result.Result as Ok<RegisterResponse>).Value, Is.EqualTo(response));
        }

        [Test]
        public async Task HandleAsync_WithInvalidRequest_ReturnsValidationProblem()
        {
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "password",
                ConfirmPassword = "password",
                AcceptedTerms = false
            };
            var validationFailures = new List<ValidationFailure> { new ValidationFailure("AcceptedTerms", "Invalid AcceptedTerms") };
            var validatorMock = new Mock<IValidator<RegisterRequest>>();
            validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));
            var userServiceMock = new Mock<IUserService>();

            var result = await Endpoint.HandleAsync(request, validatorMock.Object, userServiceMock.Object, CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
        }
    }
}
