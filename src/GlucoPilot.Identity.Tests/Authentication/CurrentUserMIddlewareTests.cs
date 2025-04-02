using System;
using System.Threading.Tasks;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;

namespace GlucoPilot.Identity.Tests.Authentication;

[TestFixture]
internal sealed class CurrentUserMIddlewareTests
{
    [Test]
    public void Constructor_Should_Throw_ArgumentNullExceptions()
    {
        Assert.That(() => new CurrentUserMiddleware(null!), Throws.InstanceOf<ArgumentNullException>());
    }
    
    [Test]
    public async Task InvokeAsync_Should_Set_CurrentUser_And_Call_Next()
    {
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        var currentUserInitializer = new Mock<ICurrentUserInitializer>();
        var middleware = new CurrentUserMiddleware(currentUserInitializer.Object);
        
        await middleware.InvokeAsync(context, next);
        
        currentUserInitializer.Verify(x => x.SetCurrentUser(context.User), Times.Once);
        Assert.That(nextCalled, Is.True);
    }
}