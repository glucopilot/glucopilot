using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GlucoPilot.Identity.Models;
using GlucoPilot.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Moq;
using Endpoint = GlucoPilot.Identity.Endpoints.Login.Endpoint;

namespace GlucoPilot.Identity.Tests.Endpoints.Login;

[TestFixture]
internal sealed class EndpointTests
{
    private Mock<IValidator<LoginRequest>> _validatorMock;
    private Mock<IUserService> _userServiceMock;
    private IOptions<IdentityOptions> _identityOptions;
    private DefaultHttpContext _httpContext;

    [SetUp]
    public void SetUp()
    {
        _validatorMock = new Mock<IValidator<LoginRequest>>();
        _userServiceMock = new Mock<IUserService>();
        _identityOptions = Options.Create(new IdentityOptions { RefreshTokenCookieName = "RefreshToken" });
        _httpContext = new DefaultHttpContext();
    }

    [Test]
    public async Task HandleAsync_With_Valid_Request_Returns_Ok_With_Login_Response()
    {
        var request = new LoginRequest { Email = "user", Password = "password" };
        var loginResponse = new LoginResponse { Email = "user", Token = "accessToken", RefreshToken = "refreshToken" };

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _userServiceMock.Setup(u => u.LoginAsync(request, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(loginResponse);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _userServiceMock.Object, _identityOptions, _httpContext, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<Ok<LoginResponse>>());
        Assert.That(((Ok<LoginResponse>)result.Result).Value, Is.EqualTo(loginResponse));
    }

    [Test]
    public async Task HandleAsync_With_Invalid_Request_Returns_Validation_Problem()
    {
        var request = new LoginRequest { Email = "", Password = "" };
        var validationResult = new FluentValidation.Results.ValidationResult(new[]
        {
            new FluentValidation.Results.ValidationFailure("Username", "Username is required"),
            new FluentValidation.Results.ValidationFailure("Password", "Password is required")
        });

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(validationResult);

        var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _userServiceMock.Object, _identityOptions, _httpContext, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
        var problemDetails = ((ValidationProblem)result.Result).ProblemDetails;
        Assert.That(problemDetails.Errors.ContainsKey("Username"), Is.True);
        Assert.That(problemDetails.Errors.ContainsKey("Password"), Is.True);
    }

    [Test]
    public async Task HandleAsync_With_Valid_Request_Sets_Refresh_Token_Cookie()
    {
        var request = new LoginRequest { Email = "user", Password = "password" };
        var loginResponse = new LoginResponse { Email = "user", Token = "accessToken", RefreshToken = "refreshToken" };

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _userServiceMock.Setup(u => u.LoginAsync(request, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(loginResponse);

        await Endpoint.HandleAsync(request, _validatorMock.Object, _userServiceMock.Object, _identityOptions, _httpContext, CancellationToken.None);

        Assert.That(_httpContext.Response.Headers.ContainsKey("Set-Cookie"), Is.True);
        Assert.That(_httpContext.Response.Headers["Set-Cookie"].ToString(), Does.Contain("RefreshToken=refreshToken"));
    }
}