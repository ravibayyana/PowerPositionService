using System.Text;
using PowerPositionService.Logger;
using PowerPositionService.Utils;

namespace PowerPositionService.Setttings;

internal class AppConfigSettings : IAppConfigSettings
{
    private readonly Dictionary<string, string> _defaultSettingValues = new()
    {
        {AppConstants.ScheduleIntervalInMinutes, "5"},
        {
            AppConstants.PowerPositionCSVLocation,
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        }
    };

    private readonly IConfiguration _config;
    private readonly AppSettings _appSettings;
    private readonly ICustomLogger _logger;

    public AppConfigSettings(ICustomLogger logger)
    {
        _logger = logger;
        _config = new ConfigurationBuilder()
            .AddJsonFile(AppConstants.AppSettingsJson)
            .Build();

        _appSettings = _config.GetRequiredSection(AppConstants.AppSettings).Get<AppSettings>();

        ScheduleIntervalInMinutes = GetScheduleIntervalInMinutes();
        PowerPositionCSVLocation = GetPowerPositionCSVLocation();

        LogAppSettings();
    }

    private void LogAppSettings()
    {
        var msg = new StringBuilder();
        msg.Append($"AppSettings ScheduleIntervalInMinutes = {ScheduleIntervalInMinutes}, ");
        msg.Append($"PowerPositionCSVLocation = {PowerPositionCSVLocation}");

        _logger.LogInformation(msg.ToString());
    }

    public int ScheduleIntervalInMinutes { get; }
    public string PowerPositionCSVLocation { get; }

    private int GetScheduleIntervalInMinutes()
    {
        var settingValue = _appSettings.ScheduleIntervalInMinutes;
        if (int.TryParse(settingValue, out var value)) return value;

        return int.Parse(_defaultSettingValues[AppConstants.ScheduleIntervalInMinutes]);
    }

    private string GetPowerPositionCSVLocation()
    {
        var settingValue = _appSettings.PowerPositionCSVLocation;
        if (!Directory.Exists(settingValue))
        {
            Directory.CreateDirectory(settingValue);
        }
        return settingValue;
    }
}