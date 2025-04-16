using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Identity.Authentication;
using GlucoPilot.Identity.Models;
using GlucoPilot.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Moq;
using Endpoint = GlucoPilot.Identity.Endpoints.RevokeToken.Endpoint;

namespace GlucoPilot.Identity.Tests.RevokeToken;

[TestFixture]
internal sealed class RevokeTokenTests
{
    [Test]
    public async Task HandleAsync_With_Valid_Token_Returns_NoContent()
    {
        var userId = Guid.NewGuid();
        var request = new RevokeTokenRequest { Token = "validToken" };
        var validatorMock = new Mock<IValidator<RevokeTokenRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RevokeTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(u => u.FindByRefreshTokenAsync("validToken", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Patient { Id = userId, Email = "test@nomail.com", PasswordHash = "password" });
        var identityOptions = Options.Create(new IdentityOptions { RefreshTokenCookieName = "RefreshToken" });
        var httpContext = new DefaultHttpContext();

        var result = await Endpoint.HandleAsync(request, validatorMock.Object, currentUserMock.Object,
            userServiceMock.Object, identityOptions, httpContext, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NoContent>());
    }

    [Test]
    public async Task HandleAsync_With_Invalid_Token_Returns_ValidationProblem()
    {
        var request = new RevokeTokenRequest { Token = null };
        var validatorMock = new Mock<IValidator<RevokeTokenRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RevokeTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Token", "Token is required") }));
        var currentUserMock = new Mock<ICurrentUser>();
        var userServiceMock = new Mock<IUserService>();
        var identityOptions = Options.Create(new IdentityOptions { RefreshTokenCookieName = "RefreshToken" });
        var httpContext = new DefaultHttpContext();

        var result = await Endpoint.HandleAsync(request, validatorMock.Object, currentUserMock.Object,
            userServiceMock.Object, identityOptions, httpContext, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
    }

    [Test]
    public async Task HandleAsync_With_Unauthorized_User_Throws_UnauthorizedException()
    {
        var request = new RevokeTokenRequest { Token = "validToken" };
        var validatorMock = new Mock<IValidator<RevokeTokenRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RevokeTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(c => c.GetUserId()).Returns(Guid.NewGuid());
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(u => u.FindByRefreshTokenAsync("validToken", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Patient { Id = Guid.NewGuid(), Email = "test@nomail.com", PasswordHash = "password" });
        var identityOptions = Options.Create(new IdentityOptions { RefreshTokenCookieName = "RefreshToken" });
        var httpContext = new DefaultHttpContext();

        Assert.That(
            async () => await Endpoint.HandleAsync(request, validatorMock.Object, currentUserMock.Object,
                userServiceMock.Object, identityOptions, httpContext, CancellationToken.None),
            Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public async Task HandleAsync_With_Null_Token_Uses_Cookie_Token()
    {
        var userId = Guid.NewGuid();
        var request = new RevokeTokenRequest { Token = null };
        var validatorMock = new Mock<IValidator<RevokeTokenRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RevokeTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(u => u.FindByRefreshTokenAsync("cookieToken", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Patient {  Id = userId, Email = "test@nomail.com", PasswordHash = "password" });
        var identityOptions = Options.Create(new IdentityOptions { RefreshTokenCookieName = "RefreshToken" });
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Cookie = "RefreshToken=cookieToken";

        var result = await Endpoint.HandleAsync(request, validatorMock.Object, currentUserMock.Object,
            userServiceMock.Object, identityOptions, httpContext, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NoContent>());
    }
}