using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Endpoints.IsVerified;
using GlucoPilot.Identity.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Moq;

namespace GlucoPilot.Identity.Tests.Endpoints.IsVerified;

[TestFixture]
internal sealed class EndpointTests
{
    [Test]
    public async Task HandleAsync_WithInvalidRequest_ReturnsValidationProblem()
    {
        var request = new IsVerifiedRequest { Email = "invalid-email" };
        var validatorMock = new Mock<IValidator<IsVerifiedRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Email", "Invalid email")]));
        var userRepositoryMock = new Mock<IRepository<User>>();
        var identityOptions = Options.Create(new IdentityOptions { RequireEmailVerification = true });

        var result = await Endpoint.HandleAsync(request, validatorMock.Object, userRepositoryMock.Object,
            identityOptions, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
    }

    [Test]
    public async Task HandleAsync_WithEmailVerificationDisabled_ReturnsNoContent()
    {
        var request = new IsVerifiedRequest { Email = "user@example.com" };
        var validatorMock = new Mock<IValidator<IsVerifiedRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var userRepositoryMock = new Mock<IRepository<User>>();
        var identityOptions = Options.Create(new IdentityOptions { RequireEmailVerification = false });

        var result = await Endpoint.HandleAsync(request, validatorMock.Object, userRepositoryMock.Object,
            identityOptions, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NoContent>());
    }

    [Test]
    public async Task HandleAsync_WithUnverifiedUser_ThrowsForbiddenException()
    {
        var request = new IsVerifiedRequest { Email = "user@example.com" };
        var validatorMock = new Mock<IValidator<IsVerifiedRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var userRepositoryMock = new Mock<IRepository<User>>();
        userRepositoryMock.Setup(repo => repo.FindOne(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new Patient { Email = "user@example.com", PasswordHash = "password", IsVerified = false });
        var identityOptions = Options.Create(new IdentityOptions { RequireEmailVerification = true });

        Assert.That(() => Endpoint.HandleAsync(request, validatorMock.Object,
            userRepositoryMock.Object, identityOptions, CancellationToken.None), Throws.InstanceOf<ForbiddenException>().With.Message.EqualTo("USER_NOT_VERIFIED"));
    }

    [Test]
    public async Task HandleAsync_WithVerifiedUser_ReturnsNoContent()
    {
        var request = new IsVerifiedRequest { Email = "user@example.com" };
        var validatorMock = new Mock<IValidator<IsVerifiedRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var userRepositoryMock = new Mock<IRepository<User>>();
        userRepositoryMock.Setup(repo => repo.FindOne(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(new Patient { Email = "user@example.com", PasswordHash = "password", IsVerified = true });
        var identityOptions = Options.Create(new IdentityOptions { RequireEmailVerification = true });

        var result = await Endpoint.HandleAsync(request, validatorMock.Object, userRepositoryMock.Object,
            identityOptions, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NoContent>());
    }

    [Test]
    public void HandleAsync_WithNonExistentUser_ThrowsForbiddenException()
    {
        var request = new IsVerifiedRequest { Email = "nonexistent@example.com" };
        var validatorMock = new Mock<IValidator<IsVerifiedRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var userRepositoryMock = new Mock<IRepository<User>>();
        userRepositoryMock.Setup(repo => repo.FindOne(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>()))
            .Returns((User)null);
        var identityOptions = Options.Create(new IdentityOptions { RequireEmailVerification = true });

        Assert.That(() => Endpoint.HandleAsync(request, validatorMock.Object,
            userRepositoryMock.Object, identityOptions, CancellationToken.None), Throws.InstanceOf<ForbiddenException>().With.Message.EqualTo("USER_NOT_VERIFIED"));
    }
}