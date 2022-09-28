using PowerPositionService.Setttings;

namespace PowerPositionService.Utils;

public class Utility : IUtility
{
    private readonly IAppConfigSettings _configSettings;
    private Timer _timer;

    public Utility(IAppConfigSettings configSettings)
    {
        _configSettings = configSettings;
    }

    public DateTime CurrentDateTime => DateTime.Now;

    public void StartTimer(TimerCallback timerCallBack)
    {
        _timer = new Timer(timerCallBack, this, StartAt, Interval);
    }

    public TimeSpan StartAt => TimeSpan.Zero;
    public TimeSpan Interval => TimeSpan.FromMinutes(_configSettings.ScheduleIntervalInMinutes);

    public void StopTimer()
    {
        _timer?.Dispose();
    }

    public void WriteToFile(string fullFilePath, string data)
    {
        File.WriteAllText(fullFilePath, data);
    }
}