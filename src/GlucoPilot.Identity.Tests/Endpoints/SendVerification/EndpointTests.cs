using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Endpoints.SendVerifiication;
using GlucoPilot.Identity.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace GlucoPilot.Identity.Tests.Endpoints.SendVerification;

[TestFixture]
internal sealed class EndpointTests
{
    private Mock<IRepository<User>> _userRepository;
    private Mock<IUserService> _userService;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IRepository<User>>();
        _userService = new Mock<IUserService>();
    }

    [Test]
    public async Task HandleAsync_ReturnsNoContent_WhenUserExists()
    {
        var email = "test@example.com";
        var user = new Patient { Email = email, EmailVerificationToken = "token", PasswordHash = "password" };
        _userRepository
            .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userService
            .Setup(service => service.SendVerificationEmailAsync(user.Email, user.EmailVerificationToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Endpoint.HandleAsync(email, _userRepository.Object, _userService.Object, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NoContent>());
        _userService.Verify(service => service.SendVerificationEmailAsync(user.Email, user.EmailVerificationToken, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task HandleAsync_ReturnsNoContent_WhenUserDoesNotExist()
    {
        var email = "nonexistent@example.com";
        _userRepository
            .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        var result = await Endpoint.HandleAsync(email, _userRepository.Object, _userService.Object, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NoContent>());
        _userService.Verify(service => service.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}