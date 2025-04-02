using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using GlucoPilot.Data.Entities;
using GlucoPilot.Identity.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace GlucoPilot.Identity.Tests.Services;

[TestFixture]
internal sealed class TokenServiceTests
{
    private Mock<IOptions<IdentityOptions>> _mockOptions;
    private TokenService _tokenService;

    [SetUp]
    public void SetUp()
    {
        _mockOptions = new Mock<IOptions<IdentityOptions>>();
        _mockOptions.Setup(o => o.Value).Returns(new IdentityOptions
        {
            TokenSigningKey = Guid.NewGuid().ToString(),
            TokenExpirationInMinutes = 30
        });
        _tokenService = new TokenService(_mockOptions.Object);
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullExceptions()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => new TokenService(null!), Throws.ArgumentNullException);
            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(o => o.Value).Returns((IdentityOptions)null);
            Assert.That(() => new TokenService(options.Object), Throws.ArgumentNullException);
        });
    }

    [Test]
    public void GenerateJwtToken_ValidUser_ReturnsToken()
    {
        var user = new Patient { Id = Guid.NewGuid(), Email = "user@example.com", PasswordHash = "hash" };
        var expectedClaimTypes = new[] { "nameid", "email" };
        var expectedClaimValues = new[] { user.Id.ToString(), user.Email };


        var token = _tokenService.GenerateJwtToken(user);

        Assert.Multiple(() =>
        {
            Assert.That(token, Is.Not.Null);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            Assert.That(jwtToken.Claims.Select(x => x.Type), Is.SupersetOf(expectedClaimTypes));
            Assert.That(jwtToken.Claims.Select(x => x.Value), Is.SupersetOf(expectedClaimValues));
        });
    }

    [Test]
    public void GenerateJwtToken_NullUser_ThrowsArgumentNullException()
    {
        Assert.That(() => _tokenService.GenerateJwtToken(null), Throws.ArgumentNullException);
    }

    [Test]
    public void GenerateJwtToken_EmptySigningKey_ThrowsArgumentException()
    {
        _mockOptions.Setup(o => o.Value).Returns(new IdentityOptions
        {
            TokenSigningKey = "",
            TokenExpirationInMinutes = 30
        });
        var tokenService = new TokenService(_mockOptions.Object);
        var user = new Patient { Id = Guid.NewGuid(), Email = "user@example.com", PasswordHash = "hash" };

        Assert.That(() => tokenService.GenerateJwtToken(user), Throws.ArgumentException);
    }
}