using Moq;
using NUnit.Framework;

namespace GlucoPilot.Data.Tests
{
    [TestFixture]
    internal sealed class GlucoPilotDbInitialiserTests
    {
        private Mock<IDatabaseFacade> _db;

        [SetUp]
        public void SetUp()
        {
            _db = new Mock<IDatabaseFacade>();
        }

        [Test]
        public async Task InitialiseDbAsync_Should_Apply_Migrations_When_Pending_Migrations()
        {
            _db.Setup(db => db.GetPendingMigrationsAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(new[] { "Migration1", "Migration2" });
            _db.Setup(db => db.MigrateAsync(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var dbInitialiser = new GlucoPilotDbInitialiser(_db.Object);

            await dbInitialiser.InitialiseDbAsync(CancellationToken.None);

            _db.Verify(db => db.MigrateAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task InitialiseDbAsync_Should_Not_Apply_Migrations_When_No_Pending_Migrations()
        {
            _db.Setup(db => db.GetPendingMigrationsAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(Array.Empty<string>());

            var dbInitialiser = new GlucoPilotDbInitialiser(_db.Object);
            var cancellationToken = CancellationToken.None;

            await dbInitialiser.InitialiseDbAsync(cancellationToken);

            _db.Verify(db => db.MigrateAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
