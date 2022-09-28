using PowerPositionService.FileUtil;
using PowerPositionService.Logger;
using PowerPositionService.Setttings;
using PowerPositionService.Utils;
using Services;

namespace PowerPositionService.Service;

public class PositionService : IPositionService
{
    private readonly ICustomLogger _logger;
    private readonly IAppConfigSettings _appConfigSettings;
    private readonly IWriteToCsvFile _writeToCsvFile;
    private readonly PowerService _powerService;
    private Timer _timer;
    private bool _isStarted = false;

    public PositionService(
        ICustomLogger logger,
        IAppConfigSettings appConfigSettings,
        IWriteToCsvFile writeToCsvFile)
    {
        _logger = logger;
        _logger = logger;
        _appConfigSettings = appConfigSettings;
        _writeToCsvFile = writeToCsvFile;
        _powerService = new PowerService();
    }

    private void TimerCallBack(object? state)
    {
        Start();
    }

    private async Task Start()
    {
        var currentDateTime = DateTime.Now;
        _logger.LogInformation($"================ START PositionService [{currentDateTime}] ======================");

        try
        {
            var trades = await _powerService.GetTradesAsync(currentDateTime);
            var powerTrades = GetPositionVolumes(trades, currentDateTime);
            _writeToCsvFile.Write(powerTrades);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to Write PositionVolumes for Date: {currentDateTime}, Msg: {e.Message}");
        }

        _logger.LogInformation($"================ END PositionService [{currentDateTime}] ======================");
    }

    public void StartCalculatingPowerPositions()
    {
        if(_isStarted)
            return;

        _isStarted = true;
        _timer = new Timer(TimerCallBack, this, TimeSpan.Zero, TimeSpan.FromMinutes(_appConfigSettings.ScheduleIntervalInMinutes));
    }

    private PositionVolumes GetPositionVolumes(IEnumerable<PowerTrade> powerTrades, DateTime forDate)
    {
        var powerTradesDictionary = new Dictionary<int, List<double>>();
        foreach (var powerTrade in powerTrades)
        {
            foreach (var powerTradePeriod in powerTrade.Periods)
            {
                if (!powerTradesDictionary.ContainsKey(powerTradePeriod.Period))
                {
                    powerTradesDictionary.Add(powerTradePeriod.Period, new List<double>());
                }

                powerTradesDictionary[powerTradePeriod.Period].Add(powerTradePeriod.Volume);
            }
        }

        return new PositionVolumes { ForDate = forDate, PowerPositions = powerTradesDictionary };
    }
}