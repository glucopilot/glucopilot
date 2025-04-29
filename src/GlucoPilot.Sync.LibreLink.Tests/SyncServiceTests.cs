using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.LibreLinkClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using LibreAuthTicket = GlucoPilot.LibreLinkClient.Models.AuthTicket;
using System.Linq.Expressions;
using GlucoPilot.LibreLinkClient.Models;
using AuthTicket = GlucoPilot.Data.Entities.AuthTicket;

namespace GlucoPilot.Sync.LibreLink.Tests;

[TestFixture]
public class SyncServiceTests
{
    private SyncService _sut;
    private Mock<IRepository<Patient>> _patientRepository;
    private Mock<IRepository<Reading>> _readingRepository;
    private Mock<IRepository<Sensor>> _sensorRepository;
    private Mock<IServiceScopeFactory> _scopeFactory;
    private Mock<ILibreLinkClient> _libreLinkClient;
    private Mock<ILogger<SyncService>> _logger;

    [SetUp]
    public void SetUp()
    {
        _patientRepository = new Mock<IRepository<Patient>>();
        _readingRepository = new Mock<IRepository<Reading>>();
        _sensorRepository = new Mock<IRepository<Sensor>>();
        _scopeFactory = new Mock<IServiceScopeFactory>();
        _scopeFactory
            .Setup(x => x.CreateScope())
            .Returns(() =>
            {
                var scope = new Mock<IServiceScope>();
                var serviceProvider = new Mock<IServiceProvider>();
                serviceProvider.Setup(x => x.GetService(typeof(IRepository<Patient>))).Returns(_patientRepository.Object);
                serviceProvider.Setup(x => x.GetService(typeof(IRepository<Reading>))).Returns(_readingRepository.Object);
                serviceProvider.Setup(x => x.GetService(typeof(IRepository<Sensor>))).Returns(_sensorRepository.Object);
                serviceProvider.Setup(x => x.GetService(typeof(ILibreLinkClient))).Returns(_libreLinkClient.Object);
                scope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
                return scope.Object;
            });
        _logger = new Mock<ILogger<SyncService>>();
        _logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _libreLinkClient = new Mock<ILibreLinkClient>();
        _sut = new SyncService(_scopeFactory.Object, _logger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    [Test]
    public void Constructor()
    {
        Assert.Throws<ArgumentNullException>(() => new SyncService(null!, _logger.Object));
        Assert.Throws<ArgumentNullException>(() => new SyncService(_scopeFactory.Object, null!));
        Assert.DoesNotThrow(() => new SyncService(_scopeFactory.Object, _logger.Object));
    }

    [Test]
    public async Task StartAsync_Starts_Timer_And_Logs()
    {
        await _sut.StartAsync(CancellationToken.None);
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Starting libre link sync service."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task StopAsync_Stops_Timer_And_Logs()
    {
        await _sut.StartAsync(CancellationToken.None);
        await _sut.StopAsync(CancellationToken.None);

        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Stopping libre link sync service."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void StartAsync_Logs_Error_When_Cancellation_Token_Is_Not_Cancellation_Token()
    {
        var invalidState = new object();

        Assert.DoesNotThrow(() => _sut.GetType()
            .GetMethod("DoWork", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_sut, [invalidState]));

        _logger.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) => v.ToString() == "Libre link sync failed."),
            It.IsAny<ArgumentException>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((_, _) => true)),
        Times.Once);
    }

    [Test]
    public void StartAsync_Logs_StartingLibreLinkSyncService()
    {
        _sut.StartAsync(CancellationToken.None);
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Starting libre link sync service."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DoWorkAsync_Logs_LibreLinkGraphDataNotFound()
    {
        var mockPatients = new List<Patient>
        {
            new Patient { Id = Guid.NewGuid(), PatientId = Guid.NewGuid().ToString(), GlucoseProvider = GlucoseProvider.LibreLink, Email = "test@test.com", PasswordHash = "1234", AuthTicket = new AuthTicket { Token = "123", Expires = 1 } },
        };

        _patientRepository
            .Setup(x => x.Find(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns((Expression<Func<Patient, bool>> predicate, FindOptions? _) =>
                mockPatients.AsQueryable());

        _libreLinkClient.Setup(x => x.LoginAsync(It.IsAny<LibreAuthTicket>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _libreLinkClient.Setup(x => x.GraphAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((GraphInformation)null!);

        await _sut.DoWorkAsync(CancellationToken.None);
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == $"Could not retrieve libre link graph data for patient {mockPatients[0].PatientId}"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DoWorkAsync_Logs_LibreLinkNoCurrentReading()
    {
        var mockPatients = new List<Patient>
        {
            new Patient { Id = Guid.NewGuid(), PatientId = Guid.NewGuid().ToString(), GlucoseProvider = GlucoseProvider.LibreLink, Email = "test@test.com", PasswordHash = "1234", AuthTicket = new AuthTicket { Token = "123", Expires = 1 } },
        };

        _patientRepository
            .Setup(x => x.Find(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns((Expression<Func<Patient, bool>> predicate, FindOptions? _) =>
                mockPatients.AsQueryable());

        _libreLinkClient.Setup(x => x.LoginAsync(It.IsAny<LibreAuthTicket>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var graphInformation = new GraphInformation
        {
            Connection = new ConnectionData
            {
                CurrentMeasurement = null,
                Sensor = new SensorData
                {
                    SensorId = Guid.NewGuid().ToString(),
                    Started = 12345678,
                }
            }
        };
        _libreLinkClient.Setup(x => x.GraphAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(graphInformation);

        await _sut.DoWorkAsync(CancellationToken.None);
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == $"No current reading for patient {mockPatients[0].PatientId}"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DoWorkAsync_Does_Not_Add_Reading_When_Reading_Already_Exists()
    {
        var mockPatients = new List<Patient>
        {
            new Patient { Id = Guid.NewGuid(), PatientId = Guid.NewGuid().ToString(), GlucoseProvider = GlucoseProvider.LibreLink, Email = "test@test.com", PasswordHash = "1234", AuthTicket = new AuthTicket { Token = "123", Expires = 1 } },
            new Patient { Id = Guid.NewGuid(), PatientId = Guid.NewGuid().ToString(), GlucoseProvider = GlucoseProvider.LibreLink, Email = "test2@test.com", PasswordHash = "12345", AuthTicket = new AuthTicket { Token = "123", Expires = 1 } }
        };

        _patientRepository
            .Setup(x => x.Find(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns((Expression<Func<Patient, bool>> predicate, FindOptions? _) =>
                mockPatients.AsQueryable());

        _libreLinkClient.Setup(x => x.LoginAsync(It.IsAny<LibreAuthTicket>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var graphInformation = new GraphInformation
        {
            Connection = new ConnectionData
            {
                CurrentMeasurement = new GraphData
                {
                    FactoryTimeStamp = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt"),
                    TimeStamp = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt"),
                    Value = 5.0,
                    TrendArrow = 1
                },
                Sensor = new SensorData
                {
                    SensorId = Guid.NewGuid().ToString(),
                    Started = 12345678,
                }
            }
        };
        _libreLinkClient.Setup(x => x.GraphAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(graphInformation);

        _readingRepository.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Reading, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);


        await _sut.DoWorkAsync(CancellationToken.None);

        _readingRepository.Verify(x => x.AddAsync(It.IsAny<Reading>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task DoWorkAsync_Logs_LibreLinkSyncCompleted()
    {
        var mockPatients = new List<Patient>
        {
            new Patient { Id = Guid.NewGuid(), PatientId = Guid.NewGuid().ToString(), GlucoseProvider = GlucoseProvider.LibreLink, Email = "test@test.com", PasswordHash = "1234", AuthTicket = new AuthTicket { Token = "123", Expires = 1 } },
            new Patient { Id = Guid.NewGuid(), PatientId = Guid.NewGuid().ToString(), GlucoseProvider = GlucoseProvider.LibreLink, Email = "test2@test.com", PasswordHash = "12345", AuthTicket = new AuthTicket { Token = "123", Expires = 1 } }
        };

        _patientRepository
            .Setup(x => x.Find(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns((Expression<Func<Patient, bool>> predicate, FindOptions? _) =>
                mockPatients.AsQueryable());

        _libreLinkClient.Setup(x => x.LoginAsync(It.IsAny<LibreAuthTicket>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var graphInformation = new GraphInformation
        {
            Connection = new ConnectionData
            {
                CurrentMeasurement = new GraphData
                {
                    FactoryTimeStamp = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt"),
                    TimeStamp = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt"),
                    Value = 5.0,
                    TrendArrow = 1
                },
                Sensor = new SensorData
                {
                    SensorId = Guid.NewGuid().ToString(),
                    Started = 12345678,
                }
            }
        };
        _libreLinkClient.Setup(x => x.GraphAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(graphInformation);
        _readingRepository.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Reading, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await _sut.DoWorkAsync(CancellationToken.None);

        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Libre link sync completed."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DoWorkAsync_Logs_LibreLinkSyncReadingFailed()
    {
        var mockPatients = new List<Patient>
        {
            new Patient { Id = Guid.NewGuid(), PatientId = Guid.NewGuid().ToString(), GlucoseProvider = GlucoseProvider.LibreLink, Email = "test@test.com", PasswordHash = "1234", AuthTicket = new AuthTicket { Token = "123", Expires = 1 } },
            new Patient { Id = Guid.NewGuid(), PatientId = Guid.NewGuid().ToString(), GlucoseProvider = GlucoseProvider.LibreLink, Email = "test2@test.com", PasswordHash = "12345", AuthTicket = new AuthTicket { Token = "123", Expires = 1 } }
        };

        _patientRepository
            .Setup(x => x.Find(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns((Expression<Func<Patient, bool>> predicate, FindOptions? _) =>
                mockPatients.AsQueryable());

        var exception = new Exception("Test exception");
        _libreLinkClient.Setup(x => x.LoginAsync(It.IsAny<LibreAuthTicket>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        await _sut.DoWorkAsync(CancellationToken.None);

        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == $"Failed to sync reading for patient {mockPatients[0].PatientId}"),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DoWorkAsync_Logs_LibreLinkNoCurrentSensor()
    {
        var mockPatients = new List<Patient>
        {
            new Patient { Id = Guid.NewGuid(), PatientId = Guid.NewGuid().ToString(), GlucoseProvider = GlucoseProvider.LibreLink, Email = "test@test.com", PasswordHash = "1234", AuthTicket = new AuthTicket { Token = "123", Expires = 1 } },
        };

        _patientRepository
            .Setup(x => x.Find(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns((Expression<Func<Patient, bool>> predicate, FindOptions? _) =>
                mockPatients.AsQueryable());

        _libreLinkClient.Setup(x => x.LoginAsync(It.IsAny<LibreAuthTicket>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var graphInformation = new GraphInformation
        {
            Connection = new ConnectionData
            {
                CurrentMeasurement = new GraphData
                {
                    FactoryTimeStamp = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt"),
                    TimeStamp = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt"),
                    Value = 5.0,
                    TrendArrow = 1
                },
                Sensor = null,
            }
        };
        _libreLinkClient.Setup(x => x.GraphAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(graphInformation);

        await _sut.DoWorkAsync(CancellationToken.None);
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == $"No current sensor for patient {mockPatients[0].PatientId}."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DoWorkAsync_Adds_New_Senser()
    {
        var mockPatients = new List<Patient>
        {
            new Patient { Id = Guid.NewGuid(), PatientId = Guid.NewGuid().ToString(), GlucoseProvider = GlucoseProvider.LibreLink, Email = "test@test.com", PasswordHash = "1234", AuthTicket = new AuthTicket { Token = "123", Expires = 1 } },        };

        _patientRepository
            .Setup(x => x.Find(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns((Expression<Func<Patient, bool>> predicate, FindOptions? _) =>
                mockPatients.AsQueryable());

        _libreLinkClient.Setup(x => x.LoginAsync(It.IsAny<LibreAuthTicket>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var graphInformation = new GraphInformation
        {
            Connection = new ConnectionData
            {
                CurrentMeasurement = new GraphData
                {
                    FactoryTimeStamp = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt"),
                    TimeStamp = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt"),
                    Value = 5.0,
                    TrendArrow = 1
                },
                Sensor = new SensorData
                {
                    SensorId = Guid.NewGuid().ToString(),
                    Started = 12345678,
                }
            }
        };
        _libreLinkClient.Setup(x => x.GraphAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(graphInformation);

        await _sut.DoWorkAsync(CancellationToken.None);

        _sensorRepository.Verify(x => x.AddAsync(It.IsAny<Sensor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DoWorkAsync_Does_Not_Add_New_Senser_If_Exists()
    {
        var mockPatients = new List<Patient>
        {
            new Patient { Id = Guid.NewGuid(), PatientId = Guid.NewGuid().ToString(), GlucoseProvider = GlucoseProvider.LibreLink, Email = "test@test.com", PasswordHash = "1234", AuthTicket = new AuthTicket { Token = "123", Expires = 1 } },        };

        _patientRepository
            .Setup(x => x.Find(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<FindOptions>()))
            .Returns((Expression<Func<Patient, bool>> predicate, FindOptions? _) =>
                mockPatients.AsQueryable());

        _libreLinkClient.Setup(x => x.LoginAsync(It.IsAny<LibreAuthTicket>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var graphInformation = new GraphInformation
        {
            Connection = new ConnectionData
            {
                CurrentMeasurement = new GraphData
                {
                    FactoryTimeStamp = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt"),
                    TimeStamp = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt"),
                    Value = 5.0,
                    TrendArrow = 1
                },
                Sensor = new SensorData
                {
                    SensorId = Guid.NewGuid().ToString(),
                    Started = 12345678,
                }
            }
        };
        _libreLinkClient.Setup(x => x.GraphAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(graphInformation);
        _sensorRepository.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Sensor, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _sut.DoWorkAsync(CancellationToken.None);

        _sensorRepository.Verify(x => x.AddAsync(It.IsAny<Sensor>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
