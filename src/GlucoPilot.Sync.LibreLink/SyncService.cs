using GlucoPilot.Data.Enums;
using GlucoPilot.LibreLinkClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GlucoPilot.Data.Entities;
using AuthTicket = GlucoPilot.LibreLinkClient.Models.AuthTicket;
using System.Globalization;
using GlucoPilot.Data.Repository;
using GlucoPilot.LibreLinkClient.Models;

namespace GlucoPilot.Sync.LibreLink;

public partial class SyncService : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SyncService> _logger;
    private Timer? _timer;

    private bool _disposed;

    public SyncService(IServiceScopeFactory scopeFactory, ILogger<SyncService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        
        if (disposing)
        {
            _timer?.Dispose();
        }

        _disposed = true;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartingLibreLinkSyncService();

        var now = DateTime.UtcNow;
        var startTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc);
        var dueTime = startTime.AddMinutes(1) - DateTime.UtcNow;

        _timer = new Timer(DoWork, cancellationToken, dueTime, TimeSpan.FromMinutes(1));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        StoppingLibreLinkSyncService();
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    internal async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        StartingLibreLinkSync();

        using (var scope = _scopeFactory.CreateScope())
        {
            var patientRepository = scope.ServiceProvider.GetRequiredService<IRepository<Patient>>();
            var readingRepository = scope.ServiceProvider.GetRequiredService<IRepository<Reading>>();
            var sensorRepository = scope.ServiceProvider.GetRequiredService<IRepository<Sensor>>();
            var patients = patientRepository.Find(p => p.PatientId != null && p.AuthTicket != null && p.GlucoseProvider == GlucoseProvider.LibreLink).ToList();
            foreach (var patient in patients)
            {
                try
                {
                    var patientId = Guid.Parse(patient.PatientId!);
                    var libreLinkClient = scope.ServiceProvider.GetRequiredService<ILibreLinkClient>();
                    var authTicket = new AuthTicket
                    {
                        Token = patient.AuthTicket!.Token,
                        Expires = patient.AuthTicket.Expires
                    };
                    await libreLinkClient.LoginAsync(authTicket, cancellationToken).ConfigureAwait(false);
                    var graph = await libreLinkClient.GraphAsync(patientId, cancellationToken).ConfigureAwait(false);
                    if (graph is null)
                    {
                        LibreLinkGraphDataNotFound(patientId);
                        continue;
                    }

                    await SyncSensor(patient, patientId, graph, sensorRepository, cancellationToken).ConfigureAwait(false);

                    await SyncReading(patient, patientId, graph, readingRepository, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LibreLinkSyncReadingFailed(patient.PatientId, ex);
                }
            }
        }

        LibreLinkSyncCompleted();

        return;
    }

    private async void DoWork(object? state)
    {
        try
        {
            if (state is not CancellationToken cancellationToken)
            {
                throw new ArgumentException("State must be a CancellationToken", nameof(state));
            }

            await DoWorkAsync(cancellationToken).ConfigureAwait(false);

        }
        catch (Exception ex)
        {
            LibreLinkSyncFailed(ex);
        }
    }

    private async Task SyncSensor(Patient patient, Guid patientId, GraphInformation graph, IRepository<Sensor> sensorRepository, CancellationToken cancellationToken)
    {
        var latestSensor = graph.Connection?.Sensor;
        if (latestSensor is null)
        {
            LibreLinkNoCurrentSensor(patientId);
            return;
        }

        var sensor = new Sensor
        {
            Created = DateTimeOffset.UtcNow,
            Started = DateTimeOffset.FromUnixTimeSeconds(latestSensor.Started),
            Expires = DateTimeOffset.FromUnixTimeSeconds(latestSensor.Started).AddDays(14),
            SensorId = latestSensor.SensorId,
            UserId = patient.Id,
        };
        if (!await sensorRepository.AnyAsync(s => s.UserId == patient.Id && s.SensorId == latestSensor.SensorId, cancellationToken).ConfigureAwait(false))
        {
            await sensorRepository.AddAsync(sensor, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task SyncReading(Patient patient, Guid patientId, GraphInformation graph, IRepository<Reading> readingRepository, CancellationToken cancellationToken)
    {
        var lastReading = graph.Connection?.CurrentMeasurement;
        if (lastReading is null)
        {
            LibreLinkNoCurrentReading(patientId);
            return;
        }

        var reading = new Reading
        {
            Created = NormaliseTimeStamp(DateTime.ParseExact(lastReading.FactoryTimeStamp, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)),
            GlucoseLevel = lastReading.Value,
            Direction = (ReadingDirection)lastReading.TrendArrow,
            UserId = patient.Id,
        };
        if (!await readingRepository.AnyAsync(r => r.UserId == reading.UserId && r.Created == reading.Created, cancellationToken).ConfigureAwait(false))
        {
            await readingRepository.AddAsync(reading, cancellationToken).ConfigureAwait(false);
        }
    }

    private static DateTime NormaliseTimeStamp(DateTime timeStamp)
    {
        return new DateTime(timeStamp.Year, timeStamp.Month, timeStamp.Day, timeStamp.Hour, timeStamp.Minute, timeStamp.Second, DateTimeKind.Utc);
    }

    [LoggerMessage(LogLevel.Information, "Starting libre link sync service.")]
    private partial void StartingLibreLinkSyncService();

    [LoggerMessage(LogLevel.Information, "Stopping libre link sync service.")]
    private partial void StoppingLibreLinkSyncService();

    [LoggerMessage(LogLevel.Error, "Libre link sync failed.")]
    private partial void LibreLinkSyncFailed(Exception error);

    [LoggerMessage(LogLevel.Information, "Starting libre link sync.")]
    private partial void StartingLibreLinkSync();

    [LoggerMessage(LogLevel.Information, "Libre link sync completed.")]
    private partial void LibreLinkSyncCompleted();

    [LoggerMessage(LogLevel.Warning, "Could not retrieve libre link graph data for patient {PatientId}")]
    private partial void LibreLinkGraphDataNotFound(Guid patientId);

    [LoggerMessage(LogLevel.Information, "No current reading for patient {PatientId}")]
    private partial void LibreLinkNoCurrentReading(Guid patientId);

    [LoggerMessage(LogLevel.Error, "Failed to sync reading for patient {PatientId}")]
    private partial void LibreLinkSyncReadingFailed(string? patientId, Exception error);

    [LoggerMessage(LogLevel.Information, "No current sensor for patient {PatientId}.")]
    private partial void LibreLinkNoCurrentSensor(Guid patientId);
}
