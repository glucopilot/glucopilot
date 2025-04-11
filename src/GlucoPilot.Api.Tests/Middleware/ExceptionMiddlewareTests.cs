using System;
using System.Threading.Tasks;
using GlucoPilot.Api.Middleware;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Middleware;

[TestFixture]
internal sealed class ExceptionMiddlewareTests
{
    private HttpContext _httpContext;
    private Mock<ICurrentUser> _currentUser;
    private Mock<ILogger<ExceptionMiddleware>> _logger;

    private ExceptionMiddleware _sut;

    [SetUp]
    public void SetUp()
    {
        _httpContext = new DefaultHttpContext();
        _currentUser = new Mock<ICurrentUser>();
        _logger = new Mock<ILogger<ExceptionMiddleware>>();

        _sut = new ExceptionMiddleware(_currentUser.Object, _logger.Object);
    }

    [Test]
    public async Task InvokeAsync_WithNoException_ReturnsOkStatusCode()
    {
        var next = new RequestDelegate(_ => Task.CompletedTask);

        await _sut.InvokeAsync(_httpContext, next);

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }

    [Test]
    public async Task InvokeAsync_WithConflictException_ReturnsConflictStatusCode()
    {
        var next = new RequestDelegate(_ => throw new ConflictException("Conflict occurred"));

        await _sut.InvokeAsync(_httpContext, next);

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
    }

    [Test]
    public async Task InvokeAsync_WithNotFoundException_ReturnsNotFoundStatusCode()
    {
        var next = new RequestDelegate(_ => throw new NotFoundException("Not found"));

        await _sut.InvokeAsync(_httpContext, next);

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task InvokeAsync_WithUnauthorizedException_ReturnsUnauthorizedStatusCode()
    {
        var next = new RequestDelegate(_ => throw new UnauthorizedException("Unauthorized"));

        await _sut.InvokeAsync(_httpContext, next);

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
    }

    [Test]
    public async Task InvokeAsync_WithForbiddenException_ReturnsForbiddenStatusCode()
    {
        var next = new RequestDelegate(_ => throw new ForbiddenException("Forbidden"));

        await _sut.InvokeAsync(_httpContext, next);

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
    }

    [Test]
    public async Task InvokeAsync_WithGenericException_ReturnsInternalServerErrorStatusCode()
    {
        var next = new RequestDelegate(_ => throw new Exception("Generic error"));

        await _sut.InvokeAsync(_httpContext, next);

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }
}