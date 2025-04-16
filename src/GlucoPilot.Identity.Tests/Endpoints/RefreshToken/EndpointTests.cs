using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Identity.Models;
using GlucoPilot.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Endpoint = GlucoPilot.Identity.Endpoints.RefreshToken.Endpoint;

namespace GlucoPilot.Identity.Tests.Endpoints.RefreshToken;

[TestFixture]
internal sealed class EndpointTests
{
    [Test]
public async Task HandleAsync_With_Valid_Refresh_Token_Returns_Ok_With_Token_Response()
{
    var userServiceMock = new Mock<IUserService>();
    var identityOptions = Options.Create(new IdentityOptions { RefreshTokenCookieName = "RefreshToken" });
    var context = new DefaultHttpContext();
    context.Request.Headers["Cookie"] = $"{identityOptions.Value.RefreshTokenCookieName}=valid-refresh-token";
    var tokenResponse = new TokenResponse { Token = "new-access-token", RefreshToken = "new-refresh-token" };
    userServiceMock.Setup(s =>
            s.RefreshTokenAsync("valid-refresh-token", It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(tokenResponse);

    var result =
        await Endpoint.HandleAsync(userServiceMock.Object, identityOptions, context, CancellationToken.None);

    Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    Assert.That(result.Value, Is.EqualTo(tokenResponse));
}

[Test]
public async Task HandleAsync_With_Null_Refresh_Token_Returns_Ok_With_Empty_Token_Response()
{
    var userServiceMock = new Mock<IUserService>();
    var identityOptions = Options.Create(new IdentityOptions { RefreshTokenCookieName = "RefreshToken" });
    var context = new DefaultHttpContext();
    var tokenResponse = new TokenResponse { Token = "token" };
    userServiceMock.Setup(s => s.RefreshTokenAsync(null, It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(tokenResponse);

    var result =
        await Endpoint.HandleAsync(userServiceMock.Object, identityOptions, context, CancellationToken.None);

    Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    Assert.That(result.Value, Is.EqualTo(tokenResponse));
}

[Test]
public async Task HandleAsync_With_New_Refresh_Token_Sets_Refresh_Token_Cookie()
{
    var userServiceMock = new Mock<IUserService>();
    var identityOptions = Options.Create(new IdentityOptions { RefreshTokenCookieName = "RefreshToken" });
    var context = new DefaultHttpContext();
    context.Request.Headers["Cookie"] = $"{identityOptions.Value.RefreshTokenCookieName}=valid-refresh-token";
    var tokenResponse = new TokenResponse { Token = "new-access-token", RefreshToken = "new-refresh-token" };
    userServiceMock.Setup(s =>
            s.RefreshTokenAsync("valid-refresh-token", It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(tokenResponse);

    await Endpoint.HandleAsync(userServiceMock.Object, identityOptions, context, CancellationToken.None);

    Assert.That(context.Response.Headers.ContainsKey("Set-Cookie"), Is.True);
    Assert.That(context.Response.Headers["Set-Cookie"].ToString(), Does.Contain("new-refresh-token"));
}
}