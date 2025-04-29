using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace GlucoPilot.Identity.Tests.Services;

[TestFixture]
internal sealed class TokenServiceTests
{
    private Mock<IOptions<IdentityOptions>> _mockOptions;
    private Mock<IRepository<User>> _mockUserRepository;
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
        _mockUserRepository = new Mock<IRepository<User>>();
        _tokenService = new TokenService(_mockOptions.Object, _mockUserRepository.Object);
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullExceptions()
    {
        var nullOption = new Mock<IOptions<IdentityOptions>>();
        nullOption.Setup(o => o.Value).Returns((IdentityOptions)null);

        Assert.Multiple(() =>
        {
            Assert.That(() => new TokenService(null!, _mockUserRepository.Object), Throws.ArgumentNullException);
            Assert.That(() => new TokenService(nullOption.Object, _mockUserRepository.Object),
                Throws.ArgumentNullException);
            Assert.That(() => new TokenService(_mockOptions.Object, null!), Throws.ArgumentNullException);
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
        var tokenService = new TokenService(_mockOptions.Object, _mockUserRepository.Object);
        var user = new Patient { Id = Guid.NewGuid(), Email = "user@example.com", PasswordHash = "hash" };

        Assert.That(() => tokenService.GenerateJwtToken(user), Throws.ArgumentException);
    }

    [Test]
    public void GenerateRefreshToken_With_Valid_IpAddress_Returns_Valid_RefreshToken()
    {
        var options = Options.Create(new IdentityOptions { RefreshTokenExpirationInDays = 7 });
        _mockUserRepository.Setup(r => r.Any(It.IsAny<Expression<Func<User, bool>>>())).Returns(false);
        var tokenService = new TokenService(options, _mockUserRepository.Object);

        var refreshToken = tokenService.GenerateRefreshToken("127.0.0.1");

        Assert.That(refreshToken, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(refreshToken.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(refreshToken.Expires, Is.GreaterThan(DateTimeOffset.UtcNow));
            Assert.That(refreshToken.CreatedByIp, Is.EqualTo("127.0.0.1"));
        });
    }

    [Test]
    public void GenerateRefreshToken_With_Non_Unique_Token_Regenerates_Token()
    {
        var options = Options.Create(new IdentityOptions { RefreshTokenExpirationInDays = 7 });
        _mockUserRepository.SetupSequence(r => r.Any(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(true)
            .Returns(false);
        var tokenService = new TokenService(options, _mockUserRepository.Object);

        var refreshToken = tokenService.GenerateRefreshToken("127.0.0.1");

        Assert.That(refreshToken, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(refreshToken.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(refreshToken.Expires, Is.GreaterThan(DateTimeOffset.UtcNow));
            Assert.That(refreshToken.CreatedByIp, Is.EqualTo("127.0.0.1"));
        });
    }
}