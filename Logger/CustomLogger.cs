using PowerPositionService.Utils;
using Serilog;

namespace PowerPositionService.Logger;

internal class CustomLogger : ICustomLogger
{
    public CustomLogger()
    {
        var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        var dateFormat = $"PowerPositionServicelog_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.log";
        var fullLogFilePath = Path.Combine("Petroineos", "Logs", dateFormat);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: outputTemplate)
            .WriteTo.File(fullLogFilePath, outputTemplate: outputTemplate)
            .CreateLogger();

        LogInformation($"Log file at: {Path.Combine(Directory.GetCurrentDirectory(), fullLogFilePath)}");
    }

    public void LogInformation(string message)
    {
        Log.Logger.Information(message.AddThreadId());
    }

    public void LogError(Exception ex, string message)
    {
        Log.Logger.Error(ex, message.AddThreadId());
    }
}