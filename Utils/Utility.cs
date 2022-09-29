using System.Reactive.Linq;
using PowerPositionService.Setttings;

namespace PowerPositionService.Utils;

public class Utility : IUtility
{
    private readonly IAppConfigSettings _configSettings;
    private readonly ICustomSchedulerProvider _customSchedulerProvider;

    public Utility(IAppConfigSettings configSettings,
        ICustomSchedulerProvider customSchedulerProvider)
    {
        _configSettings = configSettings;
        _customSchedulerProvider = customSchedulerProvider;
    }

    public DateTime CurrentDateTime => DateTime.Now;
    
    public TimeSpan StartAt => TimeSpan.Zero;
    public TimeSpan Interval => TimeSpan.FromMinutes(_configSettings.ScheduleIntervalInMinutes);

    public void WriteToFile(string fullFilePath, string data)
    {
        File.WriteAllText(fullFilePath, data);
    }

    public IDisposable StartTimer(Action timerCallBack)
    {
        return Observable.Timer(StartAt, Interval, _customSchedulerProvider.TaskPool)
            .Subscribe(_ => timerCallBack());
    }
}