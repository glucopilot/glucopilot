using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Identity.Endpoints.Login;
using GlucoPilot.Identity.Models;
using GlucoPilot.Identity.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace GlucoPilot.Identity.Tests.Endpoints.Login;

[TestFixture]
internal sealed class EndpointTests
{
    [Test]
    public async Task HandleAsync_WithValidRequest_ReturnsOkResult()
    {
        var request = new LoginRequest { Email = "test@example.com", Password = "password" };
        var validatorMock = new Mock<IValidator<LoginRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(us => us.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResponse { Token = "valid_token", UserId = Guid.NewGuid(), Email = "test@example.com" });

        var result = await Endpoint.HandleAsync(request, validatorMock.Object, userServiceMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<Ok<LoginResponse>>());
    }

    [Test]
    public async Task HandleAsync_WithInvalidRequest_ReturnsValidationProblem()
    {
        var request = new LoginRequest { Email = "test@example.com", Password = "password" };
        var validationFailures = new List<ValidationFailure> { new ValidationFailure("Email", "Invalid email") };
        var validatorMock = new Mock<IValidator<LoginRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));
        var userServiceMock = new Mock<IUserService>();

        var result = await Endpoint.HandleAsync(request, validatorMock.Object, userServiceMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
    }
}