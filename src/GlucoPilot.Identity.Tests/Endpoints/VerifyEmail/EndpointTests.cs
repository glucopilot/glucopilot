using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Identity.Endpoints.VerifyEmail;
using GlucoPilot.Identity.Models;
using GlucoPilot.Identity.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace GlucoPilot.Identity.Tests.Endpoints.VerifyEmail;

[TestFixture]
internal sealed class EndpointTests
{
    [Test]
    public async Task HandleAsync_WithValidRequest_ReturnsSuccessContent()
    {
        var request = new VerifyEmailRequest { Token = "valid-token" };
        var validatorMock = new Mock<IValidator<VerifyEmailRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var userServiceMock = new Mock<IUserService>();

        var result = await Endpoint.HandleAsync(request, validatorMock.Object, userServiceMock.Object,
            CancellationToken.None);

        Assert.That(((ContentHttpResult)result.Result).ResponseContent,
            Is.EqualTo("<h1>Success</h1><p>Your email has been verified.</p>"));
    }

    [Test]
    public async Task HandleAsync_WithInvalidRequest_ReturnsValidationProblem()
    {
        var request = new VerifyEmailRequest { Token = "" };
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Token", "Token is required")
        };
        var validatorMock = new Mock<IValidator<VerifyEmailRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));
        var userServiceMock = new Mock<IUserService>();

        var result = await Endpoint.HandleAsync(request, validatorMock.Object, userServiceMock.Object,
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
    }
}