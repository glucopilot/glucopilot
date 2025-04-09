using GlucoPilot.Api.Endpoints.LibreLink.Connections;
using GlucoPilot.Api.Models;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using GlucoPilot.LibreLinkClient;
using GlucoPilot.LibreLinkClient.Exceptions;
using GlucoPilot.LibreLinkClient.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AuthTicket = GlucoPilot.Data.Entities.AuthTicket;
using ConnectionResponse = GlucoPilot.Api.Endpoints.LibreLink.Connections.ConnectionResponse;
using LibreAuthTicket = GlucoPilot.LibreLinkClient.Models.AuthTicket;

namespace GlucoPilot.Tests.Endpoints.LibreLink.Connections
{
    [TestFixture]
    public class ListTests
    {
        private Mock<ICurrentUser> _currentUserMock;
        private Mock<ILibreLinkClient> _libreLinkClientMock;
        private Mock<IRepository<Patient>> _patientRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _currentUserMock = new Mock<ICurrentUser>();
            _libreLinkClientMock = new Mock<ILibreLinkClient>();
            _patientRepositoryMock = new Mock<IRepository<Patient>>();
        }

        [Test]
        public async Task HandleAsync_Patient_Not_Found_Returns_Not_Found()
        {

            _currentUserMock.Setup(c => c.GetUserId()).Returns(Guid.NewGuid());
            _patientRepositoryMock.Setup(r => r.FindOne(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
                .Returns((Patient)null);

            var result = await List.HandleAsync(
                _currentUserMock.Object,
                _libreLinkClientMock.Object,
                _patientRepositoryMock.Object,
                CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<NotFound>());
        }

        [Test]
        public async Task HandleAsync_Authentication_Fails_Returns_Unauthorized()
        {
            var userId = Guid.NewGuid();
            _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);

            var patient = new Patient
            {
                Id = userId,
                Email = "test@test.com",
                PasswordHash = "hashed-password",
                AuthTicket = new AuthTicket { Token = "valid-token", Expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };
            _patientRepositoryMock.Setup(r => r.FindOne(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
                .Returns(patient);

            _libreLinkClientMock.Setup(c => c.LoginAsync(It.IsAny<LibreAuthTicket>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new LibreLinkAuthenticationExpiredException());

            var result = await List.HandleAsync(
                _currentUserMock.Object,
                _libreLinkClientMock.Object,
                _patientRepositoryMock.Object,
                CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<UnauthorizedHttpResult>());
        }

        [Test]
        public async Task HandleAsync_Valid_Request_Returns_Ok_With_Connections()
        {
            var userId = Guid.NewGuid();
            _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);

            var patient = new Patient
            {
                Id = userId,
                Email = "test@test.com",
                PasswordHash = "hashed-password",
                AuthTicket = new AuthTicket { Token = "valid-token", Expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };
            _patientRepositoryMock.Setup(r => r.FindOne(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
                .Returns(patient);

            var connections = new List<ConnectionData>
            {
                new ConnectionData { PatientId = Guid.NewGuid(), FirstName = "John", LastName = "Doe" },
                new ConnectionData { PatientId = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith" }
            };
            _libreLinkClientMock.Setup(c => c.GetConnectionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(connections);

            var result = await List.HandleAsync(
                _currentUserMock.Object,
                _libreLinkClientMock.Object,
                _patientRepositoryMock.Object,
                CancellationToken.None);

            var okResult = (Ok<List<ConnectionResponse>>)result.Result;
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value.Count, Is.EqualTo(2));
            Assert.That(okResult.Value[0].PatientId, Is.EqualTo(connections[0].PatientId));
            Assert.That(okResult.Value[1].PatientId, Is.EqualTo(connections[1].PatientId));
        }

        [Test]
        public async Task HandleAsync_Valid_Request_Returns_Ok_With_Empty_List_When_No_Connections()
        {
            var userId = Guid.NewGuid();
            _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);

            var patient = new Patient
            {
                Id = userId,
                Email = "test@test.com",
                PasswordHash = "hashed-password",
                AuthTicket = new AuthTicket { Token = "valid-token", Expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };
            _patientRepositoryMock.Setup(r => r.FindOne(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
                .Returns(patient);

            _libreLinkClientMock.Setup(c => c.GetConnectionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<ConnectionData>)null);

            var result = await List.HandleAsync(
                _currentUserMock.Object,
                _libreLinkClientMock.Object,
                _patientRepositoryMock.Object,
                CancellationToken.None);

            var okResult = (Ok<List<ConnectionResponse>>)result.Result;
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value.Count, Is.EqualTo(0));
        }
    }
}