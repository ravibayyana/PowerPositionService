namespace PowerPositionService.Utils;

public interface IUtility
{
    public DateTime CurrentDateTime { get; }
    public TimeSpan StartAt{ get; }
    public TimeSpan Interval { get; }
    void WriteToFile(string fileName, string data);
    IDisposable StartTimer(Action action);
}