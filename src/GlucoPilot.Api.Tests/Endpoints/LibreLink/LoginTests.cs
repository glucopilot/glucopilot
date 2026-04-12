using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.Api.Endpoints.LibreLink.Login;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using GlucoPilot.LibreLinkClient;
using GlucoPilot.LibreLinkClient.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using LibreAuthTicket = GlucoPilot.LibreLinkClient.Models.AuthTicket;

namespace GlucoPilot.Api.Tests.Endpoints.LibreLink;

[TestFixture]
public class LoginTests
{
    private Mock<IValidator<LoginRequest>> _validatorMock;
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<ILibreLinkClient> _libreLinkClientMock;
    private Mock<ILibreLinkClientFactory> _libreLinkClientFactoryMock;
    private Mock<IRepository<Patient>> _patientRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<LoginRequest>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _libreLinkClientMock = new Mock<ILibreLinkClient>();
        _libreLinkClientFactoryMock = new Mock<ILibreLinkClientFactory>();
        _libreLinkClientFactoryMock.Setup(f => f.CreateLibreLinkClient(It.IsAny<LibreRegion>()))
            .Returns(_libreLinkClientMock.Object);
        _patientRepositoryMock = new Mock<IRepository<Patient>>();
    }

    [Test]
    public async Task HandleAsync_Should_Return_Validation_Problem_When_Validation_Fails()
    {
        var request = new LoginRequest { Username = "", Password = "password" };
        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult([
                new FluentValidation.Results.ValidationFailure("Username", "Username is required")
            ]));

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object,
            _libreLinkClientFactoryMock.Object, _patientRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<ValidationProblem>());
    }

    [Test]
    public async Task HandleAsync_Should_Return_Ok_When_Login_Is_Successful()
    {
        var request = new LoginRequest { Username = "test", Password = "password" };
        var userId = Guid.NewGuid();
        var authTicket = new LibreAuthTicket { Token = "token", Expires = 1234567890, Duration = 3600 };
        var patient = new Patient
        { Id = userId, AuthTicket = null, Email = "test@test.com", PasswordHash = "passwordhash", Region = Region.Eu };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _libreLinkClientMock
            .Setup(c => c.LoginAsync(request.Username, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authTicket);
        _patientRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object,
            _libreLinkClientFactoryMock.Object, _patientRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<LoginResponse>>());
        var okResult = result.Result as Ok<LoginResponse>;
        Assert.That(okResult!.Value.Token, Is.EqualTo(authTicket.Token));
    }

    [Test]
    public void HandleAsync_Should_Throw_Unauthorized_Exception_When_Authentication_Fails()
    {
        var request = new LoginRequest { Username = "test", Password = "password" };
        var userId = Guid.NewGuid();
        var patient = new Patient
        {
            Id = userId,
            AuthTicket = new AuthTicket
            {
                Token = "same-token",
                Expires = 1234567890,
                Duration = 3600,
                PatientId = "patient_id"
            },
            Email = "test@test.com",
            PasswordHash = "passwordhash",
            Region = Region.Eu
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _patientRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _libreLinkClientMock
            .Setup(c => c.LoginAsync(request.Username, request.Password, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LibreLinkAuthenticationFailedException());

        Assert.That(
            async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object,
                _libreLinkClientFactoryMock.Object, _patientRepositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<UnauthorizedException>().With.Message.EqualTo("LIBRE_LINK_AUTH_FAILED"));
    }

    [Test]
    public async Task HandleAsync_Should_Not_Update_Database_When_AuthToken_Is_Same()
    {
        var request = new LoginRequest { Username = "test", Password = "password" };
        var userId = Guid.NewGuid();
        var authTicket = new LibreAuthTicket { Token = "same-token", Expires = 1234567890, Duration = 3600 };
        var patient = new Patient
        {
            Id = userId,
            AuthTicket = new AuthTicket
            {
                Token = "same-token",
                Expires = 1234567890,
                Duration = 3600,
                PatientId = "patient_id"
            },
            Email = "test@test.com",
            PasswordHash = "passwordhash",
            Region = Region.Eu
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _libreLinkClientMock
            .Setup(c => c.LoginAsync(request.Username, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authTicket);
        _patientRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object,
            _libreLinkClientFactoryMock.Object, _patientRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<LoginResponse>>());
        var okResult = result.Result as Ok<LoginResponse>;
        Assert.That(okResult!.Value.Token, Is.EqualTo(authTicket.Token));

        _patientRepositoryMock.Verify(r => r.Update(It.IsAny<Patient>()), Times.Never);
    }

    [Test]
    public void HandleAsync_Should_Throw_Unauthorized_Exception_When_Patient_Not_Found()
    {
        var request = new LoginRequest { Username = "test", Password = "password" };
        var userId = Guid.NewGuid();

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _patientRepositoryMock
            .Setup(r => r.FindOne(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns((Patient)null);

        Assert.That(
            async () => await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object,
                _libreLinkClientFactoryMock.Object, _patientRepositoryMock.Object, CancellationToken.None),
            Throws.TypeOf<UnauthorizedException>().With.Message.EqualTo("PATIENT_NOT_FOUND"));
    }

    [Test]
    public async Task HandleAsync_Should_Return_Ok_When_Patient_Has_Valid_AuthTicket()
    {
        var request = new LoginRequest { Username = "test", Password = "password" };
        var userId = Guid.NewGuid();
        var authTicket = new AuthTicket
        {
            Token = "valid-token",
            Expires = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds(),
            Duration = 3600,
            PatientId = "patient_id"
        };
        var patient = new Patient
        {
            Id = userId,
            AuthTicket = authTicket,
            Email = "test@test.com",
            PasswordHash = "passwordhash",
            Region = Region.Eu
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _patientRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object,
            _libreLinkClientFactoryMock.Object, _patientRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<LoginResponse>>());
        var okResult = result.Result as Ok<LoginResponse>;
        Assert.Multiple(() =>
        {
            Assert.That(okResult!.Value.Token, Is.EqualTo(authTicket.Token));
            Assert.That(okResult.Value.Expires, Is.EqualTo(authTicket.Expires));
        });
    }

    [Test]
    public async Task HandleAsync_Should_Use_Correct_Regioned_Client([Values] Region region)
    {
        var request = new LoginRequest { Username = "test", Password = "password" };
        var userId = Guid.NewGuid();
        var patient = new Patient
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "passwordhash",
            Region = region
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _patientRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _libreLinkClientMock
            .Setup(c => c.LoginAsync(request.Username, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LibreAuthTicket { Token = "token", Expires = 1, Duration = 1 });

        await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object,
            _libreLinkClientFactoryMock.Object, _patientRepositoryMock.Object, CancellationToken.None);

        _libreLinkClientFactoryMock.Verify(f =>
            f.CreateLibreLinkClient(Enum.Parse<LibreRegion>(region.ToString())), Times.Once);
    }

    [Test]
    public async Task HandleAsync_Should_Retry_Login_When_RegionRedirectException_Is_Thrown()
    {
        var request = new LoginRequest { Username = "test", Password = "password" };
        var userId = Guid.NewGuid();
        var patient = new Patient
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "passwordhash",
            Region = Region.Eu
        };
        var authTicket = new LibreAuthTicket { Token = "token", Expires = 1234567890, Duration = 3600 };
        var redirectedClientMock = new Mock<ILibreLinkClient>();

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _patientRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _libreLinkClientMock
            .Setup(c => c.LoginAsync(request.Username, request.Password, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LibreLinkRegionRedirectException(LibreRegion.Us));
        _libreLinkClientFactoryMock
            .Setup(f => f.CreateLibreLinkClient(LibreRegion.Us))
            .Returns(redirectedClientMock.Object);
        redirectedClientMock
            .Setup(c => c.LoginAsync(request.Username, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authTicket);
        _patientRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object,
            _libreLinkClientFactoryMock.Object, _patientRepositoryMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<LoginResponse>>());
        var okResult = result.Result as Ok<LoginResponse>;
        Assert.That(okResult!.Value.Token, Is.EqualTo(authTicket.Token));
        _libreLinkClientFactoryMock.Verify(f => f.CreateLibreLinkClient(LibreRegion.Eu), Times.Once);
        _libreLinkClientFactoryMock.Verify(f => f.CreateLibreLinkClient(LibreRegion.Us), Times.Once);
        _patientRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Patient>(p => p.Region == Region.Us), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task HandleAsync_Should_Default_To_Eu_When_Patient_Region_Is_Null()
    {
        var request = new LoginRequest { Username = "test", Password = "password" };
        var userId = Guid.NewGuid();
        var patient = new Patient
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "passwordhash",
            Region = null
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
        _patientRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _patientRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _libreLinkClientMock
            .Setup(c => c.LoginAsync(request.Username, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LibreAuthTicket { Token = "token", Expires = 1, Duration = 1 });

        await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object,
            _libreLinkClientFactoryMock.Object, _patientRepositoryMock.Object, CancellationToken.None);

        _libreLinkClientFactoryMock.Verify(f => f.CreateLibreLinkClient(LibreRegion.Eu), Times.Once);
    }
}
