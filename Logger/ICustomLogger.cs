namespace PowerPositionService.Logger;

public interface ICustomLogger
{
    void LogInformation(string message);

    void LogError(Exception ex, string message);
}