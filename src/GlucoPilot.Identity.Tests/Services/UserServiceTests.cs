using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Models;
using GlucoPilot.Identity.Services;
using GlucoPilot.Mail;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace GlucoPilot.Identity.Tests.Services;

[TestFixture]
internal sealed class UserServiceTests
{
    private Mock<IRepository<User>> _userRepository;
    private Mock<ITokenService> _tokenService;
    private Mock<IMailService> _mailService;
    private Mock<ITemplateService> _templateService;
    private IdentityOptions _options;
    private Mock<IOptions<IdentityOptions>> _identityOptions;
    private UserService _sut;

    [SetUp]
    public void Setup()
    {
        _userRepository = new Mock<IRepository<User>>();
        _tokenService = new Mock<ITokenService>();
        _mailService = new Mock<IMailService>();
        _templateService = new Mock<ITemplateService>();
        _identityOptions = new Mock<IOptions<IdentityOptions>>();
        _options = new IdentityOptions() { RequireEmailVerification = false };
        _identityOptions.Setup(x => x.Value).Returns(_options);

        _sut = new UserService(_userRepository.Object, _tokenService.Object, _mailService.Object,
            _templateService.Object, _identityOptions.Object);
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullExceptions()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                () => new UserService(null!, _tokenService.Object, _mailService.Object, _templateService.Object,
                    _identityOptions.Object), Throws.ArgumentNullException);
            Assert.That(
                () => new UserService(_userRepository.Object!, null!, _mailService.Object, _templateService.Object,
                    _identityOptions.Object), Throws.ArgumentNullException);
            Assert.That(
                () => new UserService(_userRepository.Object!, _tokenService.Object, _mailService.Object, null!,
                    _identityOptions.Object), Throws.ArgumentNullException);
            Assert.That(
                () => new UserService(_userRepository.Object!, _tokenService.Object, _mailService.Object,
                    _templateService.Object, null!), Throws.ArgumentNullException);
        });
    }

    [Test]
    public async Task LoginAsync_WithValidPatientCredentials_ReturnsLoginResponse()
    {
        var request = new LoginRequest { Email = "test@example.com", Password = "password" };
        var user = new Patient
        { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") };

        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _tokenService.Setup(t => t.GenerateRefreshToken(It.IsAny<string>())).Returns(new RefreshToken { Token = "refresh_token", CreatedByIp = "127.0.0.1" });
        
        var result = await _sut.LoginAsync(request, "127.0.0.1");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Empty);
        });
    }

    [Test]
    public async Task LoginAsync_WithValidCareGiverCredentials_ReturnsLoginResponse()
    {        _tokenService.Setup(t => t.GenerateRefreshToken(It.IsAny<string>())).Returns(new RefreshToken { Token = "refresh_token", CreatedByIp = "127.0.0.1" });

        var request = new LoginRequest { Email = "test@example.com", Password = "password" };
        var user = new CareGiver
        { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), Created = DateTimeOffset.UtcNow, RefreshTokens = new List<RefreshToken>() };
        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _tokenService.Setup(t => t.GenerateRefreshToken(It.IsAny<string>())).Returns(new RefreshToken { Token = "refresh_token", CreatedByIp = "127.0.0.1" });

        var result = await _sut.LoginAsync(request, "127.0.0.1");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Empty);
            Assert.That(result.RefreshToken, Is.EqualTo("refresh_token"));
        });
    }

    [Test]
    public void LoginAsync_WithValidPatientCredentials_Unverified_Throws_Unauthorized()
    {
        _options.RequireEmailVerification = true;
        var request = new LoginRequest { Email = "test@example.com", Password = "password" };
        var user = new Patient
        { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") };

        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(user);

        Assert.That(() => _sut.LoginAsync(request, "127.0.0.1"), Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public void LoginAsync_WithValidCareGiverCredentials_Unverified_Throws_Unauthorized()
    {
        _options.RequireEmailVerification = true;
        var request = new LoginRequest { Email = "test@example.com", Password = "password" };
        var user = new CareGiver
        { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") };

        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(user);

        Assert.That(() => _sut.LoginAsync(request, "127.0.0.1"), Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public void LoginAsync_WithInvalidPatientCredentials_ThrowsUnauthorizedException()
    {
        var request = new LoginRequest { Email = "test@example.com", Password = "wrongpassword" };
        var user = new Patient
        { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") };
        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(user);

        Assert.That(() => _sut.LoginAsync(request, "127.0.0.1"), Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public void LoginAsync_WithInvalidCareGiverCredentials_ThrowsUnauthorizedException()
    {
        var request = new LoginRequest { Email = "test@example.com", Password = "wrongpassword" };
        var user = new CareGiver
        { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") };
        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(user);

        Assert.That(() => _sut.LoginAsync(request, "127.0.0.1"), Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public async Task RegisterAsync_WithNewPatient_ReturnsRegisterResponse()
    {
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "password",
            ConfirmPassword = "password",
            AcceptedTerms = true,
            FirstName = "first name",
            LastName = "last name",
        };

        var result = await _sut.RegisterAsync(request, "http://localhost", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo(request.Email));
        });
    }

    [Test]
    public async Task RegisterAsync_WithNewCareGiver_ReturnsRegisterResponse()
    {
        var patient = new Patient()
        { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") };

        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(patient);

        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "password",
            ConfirmPassword = "password",
            AcceptedTerms = true,
            PatientId = patient.Id
        };

        var result = await _sut.RegisterAsync(request, "http://localhost", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo(request.Email));
        });
    }

    [Test]
    public async Task RegisterAsync_Patient_RequiresEmailVerification()
    {
        _options.RequireEmailVerification = true;
        var patient = new Patient()
        { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") };

        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(patient);

        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "password",
            ConfirmPassword = "password",
            AcceptedTerms = true,
            PatientId = patient.Id
        };

        _ = await _sut.RegisterAsync(request, "http://localhost", CancellationToken.None);

        _mailService.Verify(
            m => m.SendAsync(It.Is<MailRequest>(x => x.To.SequenceEqual(new[] { request.Email })),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RegisterAsync_CareGiver_RequiresEmailVerification()
    {
        _options.RequireEmailVerification = true;

        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "password",
            ConfirmPassword = "password",
            AcceptedTerms = true,
        };

        _ = await _sut.RegisterAsync(request, "http://localhost", CancellationToken.None);

        _mailService.Verify(
            m => m.SendAsync(It.Is<MailRequest>(x => x.To.SequenceEqual(new[] { request.Email })),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void RegisterAsync_WithExistingUser_ThrowsConflictException()
    {
        var request = new RegisterRequest
        {
            Email = "existinguser@example.com",
            Password = "password",
            ConfirmPassword = "password",
            AcceptedTerms = true
        };
        _userRepository.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Assert.That(() => _sut.RegisterAsync(request, "http://localhost", CancellationToken.None),
            Throws.InstanceOf<ConflictException>());
    }

    [Test]
    public void RegisterAsync_WithNonExistentPatientId_ThrowsNotFoundException()
    {
        var request = new RegisterRequest
        {
            Email = "caregiver@example.com",
            Password = "password",
            ConfirmPassword = "password",
            AcceptedTerms = true,
            PatientId = Guid.NewGuid()
        };

        Assert.That(() => _sut.RegisterAsync(request, "http://localhost", CancellationToken.None),
            Throws.InstanceOf<NotFoundException>());
    }

    [Test]
    public void VerifyEmailAsync_Throws_UnauthorizedException()
    {
        var request = new VerifyEmailRequest
        {
            Token = "token"
        };

        _userRepository
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        Assert.That(() => _sut.VerifyEmailAsync(request), Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public async Task VerifyEmailAsync_Updates_User()
    {
        var user = new Patient
        {
            Email = "user@nomail.com",
            PasswordHash = "password",
            EmailVerificationToken = "token",
        };
        var request = new VerifyEmailRequest
        {
            Token = "token"
        };

        _userRepository
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _sut.VerifyEmailAsync(request, CancellationToken.None);

        _userRepository.Verify(
            r => r.UpdateAsync(It.Is<User>(u => u.EmailVerificationToken == null && u.IsVerified),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RefreshTokenAsync_With_Valid_Token_Returns_New_Token_Response()
    {
        var user = new Patient
        {
            Email = "existinguser@example.com",
            PasswordHash = "password",
            RefreshTokens =
            [
                new RefreshToken
                    { Token = "valid-token", Expires = DateTime.UtcNow.AddMinutes(5), CreatedByIp = "127.0.0.1" }
            ]
        };
        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var newRefreshToken = new RefreshToken
        {
            Token = "new-refresh-token",
            Expires = DateTime.UtcNow.AddMinutes(5),
            CreatedByIp = "127.0.0.1",
        };
        _tokenService.Setup(t => t.GenerateRefreshToken("127.0.0.1")).Returns(newRefreshToken);
        var newJwtToken = "new-token";
        _tokenService.Setup(t => t.GenerateJwtToken(user)).Returns(newJwtToken);

        var result = await _sut.RefreshTokenAsync("valid-token", "127.0.0.1", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.EqualTo(newJwtToken));
            Assert.That(result.RefreshToken, Is.EqualTo(newRefreshToken.Token));
        });
    }

    [Test]
    public void RefreshTokenAsync_With_Null_Token_Throws_UnauthorizedException()
    {
        Assert.That(() => _sut.RefreshTokenAsync(null, "127.0.0.1", CancellationToken.None),
            Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public void RefreshTokenAsync_With_Invalid_Token_Throws_UnauthorizedException()
    {
        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        Assert.That(() => _sut.RefreshTokenAsync("invalid-token", "127.0.0.1", CancellationToken.None),
            Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public void RefreshTokenAsync_With_Revoked_Token_Throws_UnauthorizedException()
    {
        var user = new Patient
        {
            Email = "test@nomail.com",
            PasswordHash = "password",
            RefreshTokens = new List<RefreshToken>
            {
                new RefreshToken { Token = "revoked-token", CreatedByIp = "127.0.0.1" }
            }
        };
        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Assert.That(() => _sut.RefreshTokenAsync("revoked-token", "127.0.0.1", CancellationToken.None),
            Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public async Task RefreshTokenAsync_With_Expired_Token_Throws_UnauthorizedException()
    {
        var user = new Patient
        {
            Email = "test@nomail.com",
            PasswordHash = "password",
            RefreshTokens =
            [
                new RefreshToken
                    { Token = "expired-token", Expires = DateTime.UtcNow.AddMinutes(-5), CreatedByIp = "127.0.0.1" }
            ]
        };
        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Assert.That(() => _sut.RefreshTokenAsync("expired-token", "127.0.0.1", CancellationToken.None),
            Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public void RefreshTokenAsync_When_Refresh_Token_Is_Revoked_Revokes_Recursively()
    {
        var revokedToken = new RefreshToken
        {
            Token = "revoked-token",
            Revoked = DateTimeOffset.Now.AddMinutes(-1),
            ReplacedByToken = "child-token",
            CreatedByIp = "127.0.0.1",
        };

        var childToken = new RefreshToken
        {
            Token = "child-token",
            Expires = DateTimeOffset.Now.AddMinutes(1),
            CreatedByIp = "127.0.0.1",
        };

        var user = new Patient
        {
            Email = "test@nomail.com",
            PasswordHash = "password",
            RefreshTokens = new List<RefreshToken> { revokedToken, childToken }
        };

        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Assert.Multiple(() =>
        {
            Assert.That(async () => await _sut.RefreshTokenAsync("revoked-token", "127.0.0.1", CancellationToken.None),
                Throws.TypeOf<UnauthorizedException>());

            Assert.That(revokedToken.IsRevoked, Is.True);
            Assert.That(childToken.IsRevoked, Is.True);
        });
    }

    [Test]
    public async Task Revoke_Token_Async_With_Valid_Token_Revokes_Token_Successfully()
    {
        var token = "valid-token";
        var ipAddress = "127.0.0.1";
        var refreshToken = new RefreshToken { Token = token, CreatedByIp = "127.0.0.1", Expires = DateTimeOffset.Now.AddMinutes(5) };
        var user = new Patient { Email = "test@nomail.com", PasswordHash = "password", RefreshTokens = new List<RefreshToken> { refreshToken } };

        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _sut.RevokeTokenAsync(token, ipAddress, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(refreshToken.IsActive, Is.False);
            Assert.That(refreshToken.Revoked, Is.Not.Null);
            Assert.That(refreshToken.RevokedByIp, Is.EqualTo(ipAddress));
            Assert.That(refreshToken.RevokedReason, Is.EqualTo("Revoked without replacement"));
        });

        _userRepository.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Revoke_Token_Async_With_Invalid_Token_Throws_UnauthorizedException()
    {
        var token = "invalid-token";

        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        Assert.That(async () => await _sut.RevokeTokenAsync(token, "127.0.0.1", CancellationToken.None),
            Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public void Revoke_Token_Async_With_Inactive_Token_Throws_UnauthorizedException()
    {
        var token = "inactive-token";
        var refreshToken = new RefreshToken { Token = token, CreatedByIp = "127.0.0.1", Expires = DateTimeOffset.Now.AddMinutes(-5) };
        var user = new Patient { Email = "test@nomail.com", PasswordHash = "password", RefreshTokens = new List<RefreshToken> { refreshToken } };

        _userRepository.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Assert.That(async () => await _sut.RevokeTokenAsync(token, "127.0.0.1", CancellationToken.None),
            Throws.TypeOf<UnauthorizedException>());
    }
}