using System;
using System.Linq;
using System.Security.Claims;
using GlucoPilot.Identity.Authentication;

namespace GlucoPilot.Identity.Tests.Authentication;

[TestFixture]
internal sealed class CurrentUserTests
{
    [Test]
    public void GetUserId_WithAuthenticatedUser_ReturnsUserId()
    {
        var userId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "mock"));
        var currentUser = new CurrentUser();
        currentUser.SetCurrentUser(user);

        var result = currentUser.GetUserId();

        Assert.That(result, Is.EqualTo(userId));
    }

    [Test]
    public void GetUserId_WithUnauthenticatedUser_ReturnsNull()
    {
        var currentUser = new CurrentUser();

        var result = currentUser.GetUserId();

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetUserEmail_WithAuthenticatedUser_ReturnsUserEmail()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Email, "test@example.com")], "mock"));
        var currentUser = new CurrentUser();
        currentUser.SetCurrentUser(user);

        var result = currentUser.GetUserEmail();

        Assert.That(result, Is.EqualTo("test@example.com"));
    }

    [Test]
    public void GetUserEmail_WithUnauthenticatedUser_ReturnsNull()
    {
        var currentUser = new CurrentUser();

        var result = currentUser.GetUserEmail();

        Assert.That(result, Is.Null);
    }

    [Test]
    public void IsAuthenticated_WithAuthenticatedUser_ReturnsTrue()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "123")], "mock"));
        var currentUser = new CurrentUser();
        currentUser.SetCurrentUser(user);

        var result = currentUser.IsAuthenticated();

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAuthenticated_WithUnauthenticatedUser_ReturnsFalse()
    {
        var currentUser = new CurrentUser();

        var result = currentUser.IsAuthenticated();

        Assert.That(result, Is.False);
    }

    [Test]
    public void GetClaimsIdentity_WithAuthenticatedUser_ReturnsClaims()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "123"), new Claim(ClaimTypes.Email, "test@example.com") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
        var currentUser = new CurrentUser();
        currentUser.SetCurrentUser(user);

        var result = currentUser.GetClaimsIdentity();

        Assert.That(result.Select(c => $"{c.Type}:{c.Value}"), Is.EqualTo(claims.Select(c => $"{c.Type}:{c.Value}")).AsCollection);
    }

    [Test]
    public void GetClaimsIdentity_WithUnauthenticatedUser_ReturnsEmpty()
    {
        var currentUser = new CurrentUser();

        var result = currentUser.GetClaimsIdentity();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void SetCurrentUser_WithAlreadySetUser_ThrowsInvalidOperationException()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "123")], "mock"));
        var currentUser = new CurrentUser();
        currentUser.SetCurrentUser(user);

        Assert.That(() => currentUser.SetCurrentUser(user), Throws.InstanceOf<InvalidOperationException>());
    }
}