﻿using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Api.Endpoints.LibreLink.PairConnection;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using GlucoPilot.LibreLinkClient;
using GlucoPilot.LibreLinkClient.Exceptions;
using GlucoPilot.LibreLinkClient.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using AuthTicket = GlucoPilot.Data.Entities.AuthTicket;

namespace GlucoPilot.Api.Tests.Endpoints.LibreLink;

[TestFixture]
public class PairConnectionTests
{
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Patient>> _patientRepositoryMock;
    private Mock<ILibreLinkClient> _libreLinkClientMock;

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _patientRepositoryMock = new Mock<IRepository<Patient>>();
        _libreLinkClientMock = new Mock<ILibreLinkClient>();
    }

    [Test]
    public async Task HandleAsync_Should_Return_Ok_When_Connection_Exists()
    {
        var userId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = new Patient
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "passwordhash",
            AuthTicket = new AuthTicket
            {
                Token = "libreToken",
                Duration = 1000,
                Expires = long.MaxValue,
                PatientId = "patient_id"
            }
        };
        var request = new PairConnectionRequest { PatientId = patientId };

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _patientRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _libreLinkClientMock.Setup(x => x.GetConnectionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ConnectionData { PatientId = patientId, FirstName = "Firstname", LastName = "LastName" }
            ]);

        var result = await Endpoint.HandleAsync(request, _currentUserMock.Object, _patientRepositoryMock.Object,
            _libreLinkClientMock.Object, CancellationToken.None);

        var okResult = result.Result as Ok<PairConnectionResponse>;
        Assert.Multiple(() =>
        {
            Assert.That(okResult.Value, Is.InstanceOf<PairConnectionResponse>());
            Assert.That(okResult!.Value.Id, Is.EqualTo(userId));
        });
        Assert.That(okResult.Value.PatientId, Is.EqualTo(patientId));
    }

    [Test]
    public void HandleAsync_Should_Throw_UnauthorizedException_When_Patient_AuthTicket_Not_Found()
    {
        var userId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = new Patient { Id = userId, Email = "test@test.com", PasswordHash = "passwordhash" };
        var request = new PairConnectionRequest { PatientId = patientId };

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _patientRepositoryMock.Setup(x =>
                x.FindOne(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(patient);
        _libreLinkClientMock.Setup(x => x.GetConnectionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ConnectionData { PatientId = patientId, FirstName = "Firstname", LastName = "LastName" }
            ]);

        Assert.ThrowsAsync<UnauthorizedException>(async () =>
            await Endpoint.HandleAsync(request, _currentUserMock.Object, _patientRepositoryMock.Object,
                _libreLinkClientMock.Object, CancellationToken.None));
    }

    [Test]
    public void HandleAsync_Should_Throw_UnauthorizedException_When_Patient_Not_Found()
    {
        var userId = Guid.NewGuid();
        var request = new PairConnectionRequest { PatientId = Guid.NewGuid() };

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _patientRepositoryMock.Setup(x =>
                x.FindOne(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns((Patient)null);

        Assert.ThrowsAsync<UnauthorizedException>(async () =>
            await Endpoint.HandleAsync(request, _currentUserMock.Object, _patientRepositoryMock.Object,
                _libreLinkClientMock.Object, CancellationToken.None));
    }

    [Test]
    public void HandleAsync_Should_Throw_NotFoundException_When_Connection_Not_Found()
    {
        var userId = Guid.NewGuid();
        var patient = new Patient
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "passwordhash",
            AuthTicket = new AuthTicket
            {
                Token = "libreToken",
                Duration = 1000,
                Expires = long.MaxValue,
                PatientId = "patient_id"
            }
        };
        var request = new PairConnectionRequest { PatientId = Guid.NewGuid() };

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _patientRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _libreLinkClientMock.Setup(x => x.GetConnectionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await Endpoint.HandleAsync(request, _currentUserMock.Object, _patientRepositoryMock.Object,
                _libreLinkClientMock.Object, CancellationToken.None));
    }

    [Test]
    public void HandleAsync_Should_Throw_UnauthorizedException_When_NotAuthenticated()
    {
        var userId = Guid.NewGuid();
        var patient = new Patient
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "passwordhash",
            AuthTicket = new AuthTicket
            {
                Token = "libreToken",
                Duration = 1000,
                Expires = long.MaxValue,
                PatientId = "patient_id"
            }
        };
        var request = new PairConnectionRequest { PatientId = Guid.NewGuid() };

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _patientRepositoryMock
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _libreLinkClientMock.Setup(x => x.GetConnectionsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LibreLinkNotAuthenticatedException());

        Assert.ThrowsAsync<UnauthorizedException>(async () =>
            await Endpoint.HandleAsync(request, _currentUserMock.Object, _patientRepositoryMock.Object,
                _libreLinkClientMock.Object, CancellationToken.None));
    }

    [Test]
    public void HandleAsync_Should_Throw_UnauthorizedException_When_Authentication_Expired()
    {
        var userId = Guid.NewGuid();
        var patient = new Patient
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "passwordhash",
            AuthTicket = new AuthTicket
            {
                Token = "libreToken",
                Duration = 1000,
                Expires = long.MaxValue,
                PatientId = "patient_id"
            }
        };
        var request = new PairConnectionRequest { PatientId = Guid.NewGuid() };

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _patientRepositoryMock.Setup(x =>
                x.FindOne(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(patient);
        _libreLinkClientMock.Setup(x => x.GetConnectionsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LibreLinkAuthenticationExpiredException());

        Assert.That(() => Endpoint.HandleAsync(request, _currentUserMock.Object, _patientRepositoryMock.Object,
            _libreLinkClientMock.Object, CancellationToken.None), Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public void HandleAsync_Should_Throw_UnauthorizedException_When_Authentication_Fails()
    {
        var userId = Guid.NewGuid();
        var patient = new Patient
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "passwordhash",
            AuthTicket = new AuthTicket
            {
                Token = "libreToken",
                Duration = 1000,
                Expires = long.MaxValue,
                PatientId = "patient_id"
            }
        };
        var request = new PairConnectionRequest { PatientId = Guid.NewGuid() };

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _patientRepositoryMock.Setup(x =>
                x.FindOne(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns(patient);
        _libreLinkClientMock.Setup(x => x.GetConnectionsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LibreLinkAuthenticationFailedException());

        Assert.ThrowsAsync<UnauthorizedException>(async () =>
            await Endpoint.HandleAsync(request, _currentUserMock.Object, _patientRepositoryMock.Object,
                _libreLinkClientMock.Object, CancellationToken.None));
    }
}