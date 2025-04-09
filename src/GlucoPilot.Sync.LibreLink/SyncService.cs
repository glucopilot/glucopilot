using GlucoPilot.Data;
using GlucoPilot.Data.Enums;
using GlucoPilot.LibreLinkClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GlucoPilot.Data.Entities;
using AuthTicket = GlucoPilot.LibreLinkClient.Models.AuthTicket;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using GlucoPilot.Data.Repository;

namespace GlucoPilot.Sync.LibreLink;

public partial class SyncService : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SyncService> _logger;
    private Timer? _timer;

    public SyncService(IServiceScopeFactory scopeFactory, ILogger<SyncService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartingLibreLinkSyncService();
        _timer = new Timer(DoWork, cancellationToken, TimeSpan.Zero, TimeSpan.FromMinutes(1));

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

                    var lastReading = graph.Connection?.CurrentMeasurement;
                    if (lastReading is null)
                    {
                        LibreLinkNoCurrentReading(patientId);
                        continue;
                    }

                    var reading = new Reading
                    {
                        Created = NormaliseTimeStamp(DateTime.ParseExact(lastReading.FactoryTimeStamp, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)),
                        GlucoseLevel = lastReading.Value,
                        Direction = (ReadingDirection)lastReading.TrendArrow,
                        UserId = patient.Id,
                    };

                    if (await readingRepository.AnyAsync(r => r.UserId == patientId && r.Created == reading.Created))
                    {
                        continue;
                    }

                    await readingRepository.AddAsync(reading);
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
}
