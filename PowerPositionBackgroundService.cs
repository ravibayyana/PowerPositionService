using PowerPositionService.Logger;
using PowerPositionService.Service;

namespace PowerPositionService;

public class PowerPositionBackgroundService : BackgroundService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ICustomLogger _logger;
    private readonly IPositionService _positionService;

    public PowerPositionBackgroundService(
        IPositionService positionService,
        ICustomLogger logger,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _positionService = positionService;
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("***************** PowerPositionBackgroundService STARTED *****************");

            while (!stoppingToken.IsCancellationRequested)
            {
                _positionService.StartCalculatingPowerPositions();
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("***************** PowerPositionBackgroundService STOPPED *****************");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"UnHandled Exception. Service stopped abruptly: {ex.Message}");
            //Environment.Exit(1);
        }
        finally
        {
            (_positionService as IDisposable)?.Dispose();
            _hostApplicationLifetime.StopApplication();
        }
    }
}