using PowerPositionService.FileUtil;
using PowerPositionService.Logger;
using PowerPositionService.Utils;
using Services;

namespace PowerPositionService.Service;

public class PositionService : IPositionService, IDisposable
{
    private readonly ICustomLogger _logger;
    private readonly IUtility _utility;
    private readonly IWriteToCsvFile _writeToCsvFile;
    private readonly IPowerService _powerService;
    private bool _isStarted = false;
    private IDisposable _timerDisposable;

    public PositionService(
        ICustomLogger logger,
        IUtility utility,
        IWriteToCsvFile writeToCsvFile, 
        IPowerService powerService)
    {
        _logger = logger;
        _utility = utility;
        _logger = logger;
        _writeToCsvFile = writeToCsvFile;
        _powerService = powerService;
    }

    private void TimerCallBack()
    {
        Start();
    }

    private async Task Start()
    {
        var currentDateTime = _utility.CurrentDateTime;
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
        _timerDisposable =  _utility.StartTimer(TimerCallBack);
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

    public void Dispose()
    {
        _timerDisposable?.Dispose();
    }
}