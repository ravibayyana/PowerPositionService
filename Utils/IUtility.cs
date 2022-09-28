namespace PowerPositionService.Utils;

public interface IUtility
{
    public DateTime CurrentDateTime { get; }
    public void StartTimer(TimerCallback timerCallBack);

    public TimeSpan StartAt{ get; }
    public TimeSpan Interval { get; }
    void StopTimer();
    void WriteToFile(string fileName, string data);
}