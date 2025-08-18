using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace GlucoPilot.Data.Tests;

[TestFixture]
public class DataServiceTests
{
    private DataService _sut;
    private Mock<IRepository<Reading>> _readingRepository;
    private Mock<IServiceScopeFactory> _scopeFactory;
    private Mock<IOptions<DataServiceOptions>> _options;
    private Mock<ILogger<DataService>> _logger;

    [SetUp]
    public void SetUp()
    {
        _readingRepository = new Mock<IRepository<Reading>>();
        _scopeFactory = new Mock<IServiceScopeFactory>();
        _scopeFactory.Setup(x => x.CreateScope())
            .Returns(() =>
            {
                var scope = new Mock<IServiceScope>();
                var serviceProvider = new Mock<IServiceProvider>();
                serviceProvider.Setup(x => x.GetService(typeof(IRepository<Reading>)))
                    .Returns(_readingRepository.Object);
                scope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
                return scope.Object;
            });
        _options = new Mock<IOptions<DataServiceOptions>>();
        var options = new DataServiceOptions() { };
        _options.Setup(o => o.Value).Returns(options);
        _logger = new Mock<ILogger<DataService>>();
        _logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _sut = new DataService(_scopeFactory.Object, _logger.Object, _options.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    [Test]
    public void Constructor()
    {
        Assert.Throws<ArgumentNullException>(() => new DataService(null!, _logger.Object, _options.Object));
        Assert.Throws<ArgumentNullException>(() => new DataService(_scopeFactory.Object, null!, _options.Object));
        Assert.Throws<ArgumentNullException>(() => new DataService(_scopeFactory.Object, _logger.Object, null!));
    }

    [Test]
    public async Task StartAsync_Starts_Timer_And_Logs()
    {
        await _sut.StartAsync(CancellationToken.None);
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Starting data service."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task StopAsync_Stops_Timer_And_Logs()
    {
        await _sut.StopAsync(CancellationToken.None);
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Stopping data service."),
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
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Data service failed."),
                It.IsAny<ArgumentException>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((_, _) => true)),
            Times.Once);
    }

    [Test]
    public async Task DoWorkAsync_Logs_Starting_Data_Clean()
    {
        await _sut.DoWorkAsync(CancellationToken.None).ConfigureAwait((false));
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Starting data clean."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Test]
    public async Task DoWorkAsync_Logs_Deleting_Expired_Data()
    {
        await _sut.DoWorkAsync(CancellationToken.None).ConfigureAwait(false);
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Deleting expired data."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}