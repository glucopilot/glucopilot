using NUnit.Framework;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Api.Endpoints.LibreLink.PairConnection;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using GlucoPilot.LibreLinkClient;
using GlucoPilot.LibreLinkClient.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.LibreLinkClient.Models;
using System.Linq.Expressions;

namespace GlucoPilot.Api.Tests.Endpoints.LibreLink.PairConnection
{
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
            var patient = new Patient { Id = userId, Email = "test@test.com", PasswordHash = "passwordhash" };
            var request = new PairConnectionRequest { PatientId = patientId };

            _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
            _patientRepositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
                .Returns(patient);
            _libreLinkClientMock.Setup(x => x.GetConnectionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { new ConnectionData { PatientId = patientId, FirstName = "Firstname", LastName = "LastName" } });

            var result = await Endpoint.HandleAsync(request, _currentUserMock.Object, _patientRepositoryMock.Object, _libreLinkClientMock.Object, CancellationToken.None);

            var okResult = result.Result as Ok<PairConnectionResponse>;
            Assert.That(okResult.Value, Is.InstanceOf<PairConnectionResponse>());
            Assert.That(okResult!.Value.Id, Is.EqualTo(userId));
            Assert.That(okResult.Value.PatientId, Is.EqualTo(patientId));
        }

        [Test]
        public void HandleAsync_Should_Throw_UnauthorizedException_When_Patient_Not_Found()
        {
            var userId = Guid.NewGuid();
            var request = new PairConnectionRequest { PatientId = Guid.NewGuid() };

            _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
            _patientRepositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
                .Returns((Patient)null);

            Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await Endpoint.HandleAsync(request, _currentUserMock.Object, _patientRepositoryMock.Object, _libreLinkClientMock.Object, CancellationToken.None));
        }

        [Test]
        public void HandleAsync_Should_Throw_NotFoundException_When_Connection_Not_Found()
        {
            var userId = Guid.NewGuid();
            var patient = new Patient { Id = userId, Email = "test@test.com", PasswordHash = "passwordhash" };
            var request = new PairConnectionRequest { PatientId = Guid.NewGuid() };

            _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
            _patientRepositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
                .Returns(patient);
            _libreLinkClientMock.Setup(x => x.GetConnectionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<ConnectionData>());

            Assert.ThrowsAsync<NotFoundException>(async () =>
                await Endpoint.HandleAsync(request, _currentUserMock.Object, _patientRepositoryMock.Object, _libreLinkClientMock.Object, CancellationToken.None));
        }

        [Test]
        public void HandleAsync_Should_Throw_UnauthorizedException_When_Authentication_Fails()
        {
            var userId = Guid.NewGuid();
            var patient = new Patient { Id = userId, Email = "test@test.com", PasswordHash = "passwordhash" };
            var request = new PairConnectionRequest { PatientId = Guid.NewGuid() };

            _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
            _patientRepositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
                .Returns(patient);
            _libreLinkClientMock.Setup(x => x.GetConnectionsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new LibreLinkAuthenticationFailedException());

            Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await Endpoint.HandleAsync(request, _currentUserMock.Object, _patientRepositoryMock.Object, _libreLinkClientMock.Object, CancellationToken.None));
        }
    }
}