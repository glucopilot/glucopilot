using System;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Data;

public partial class DataService : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DataService> _logger;
    private readonly DataServiceOptions _options;
    private Timer? _timer;

    private bool _disposed;

    public DataService(IServiceScopeFactory scopeFactory, ILogger<DataService> logger, IOptions<DataServiceOptions> options)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
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
        StartingDataService();

        _timer = new Timer(DoWork, cancellationToken, TimeSpan.Zero, _options.RunInterval);

        return Task.CompletedTask;
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
            DataServiceFailed(ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        StoppingDataService();
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    internal async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        StartingDataClean();

        using (var scope = _scopeFactory.CreateScope())
        {
            var readingsRepository = scope.ServiceProvider.GetRequiredService<IRepository<Reading>>();

            DeletingExpiredData();

            var expireDate = DateTime.UtcNow.Date - TimeSpan.FromDays(_options.DataExpireAge);
            await readingsRepository.DeleteManyAsync(r => r.Created < expireDate, cancellationToken);
        }
    }

    [LoggerMessage(LogLevel.Information, "Starting data service.")]
    private partial void StartingDataService();

    [LoggerMessage(LogLevel.Information, "Aggregating data.")]
    private partial void AggregatingData();

    [LoggerMessage(LogLevel.Information, "Deleting expired data.")]
    private partial void DeletingExpiredData();

    [LoggerMessage(LogLevel.Error, "Data service failed.")]
    private partial void DataServiceFailed(Exception ex);

    [LoggerMessage(LogLevel.Information, "Starting data clean.")]
    private partial void StartingDataClean();

    [LoggerMessage(LogLevel.Information, "Stopping data service.")]
    private partial void StoppingDataService();
}