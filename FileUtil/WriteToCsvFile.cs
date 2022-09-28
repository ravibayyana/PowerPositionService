using System.Text;
using PowerPositionService.Logger;
using PowerPositionService.Setttings;
using PowerPositionService.Utils;

namespace PowerPositionService.FileUtil;

public class WriteToCsvFile : IWriteToCsvFile
{
    private readonly IAppConfigSettings _appConfigSettings;
    private readonly ICustomLogger _logger;
    private readonly Dictionary<int, string> _periodMap;

    public WriteToCsvFile(ICustomLogger logger,
        IAppConfigSettings appConfigSettings)
    {
        _logger = logger;
        _appConfigSettings = appConfigSettings;
        _periodMap = SetupPeriodMap();
    }

    public async Task Write(PositionVolumes positionVolumes)
    {
        await Task.Run(() =>
        {
            var positions = positionVolumes.PowerPositions.ToDictionary(x => x.Key, x => x.Value.Sum());
            var date = positionVolumes.ForDate;
            var data = new StringBuilder();

            for (var i = 0; i < positionVolumes.PowerPositions.Count; i++)
            {
                data.Append($"Local Time, Volume{Environment.NewLine}");
                foreach (var position in positions)
                {
                    data.Append($"{_periodMap[position.Key]}, {position.Value}{Environment.NewLine}");
                }
                data.Append($",{Environment.NewLine}");
                data.Append($",{Environment.NewLine}");
            }

            var fullFilePath = GetPath(date);
            File.WriteAllText(fullFilePath, data.ToString());
            _logger.LogInformation($"SUCCESSFULLY created PowerPosition: {fullFilePath}");
        });
    }

    private Dictionary<int, string> SetupPeriodMap()
    {
        var startPeriod = 2;
        var statTime = 0;

        var periodMap = new Dictionary<int, TimeSpan>
        {
            {1, new TimeSpan(0, 23, 0, 0)}
        };

        for (var i = 0; i < 23; i++) periodMap.Add(startPeriod + i, new TimeSpan(0, statTime + i, 0, 0));

        return periodMap.ToDictionary(x => x.Key, x => $"{x.Value.Hours:00}:{x.Value.Minutes:00}");
    }

    private string GetPath(DateTime forDate)
    {
        var fileName =
            $"PowerPosition_{forDate.Year:0000}{forDate.Month:00}{forDate.Day:00}_{forDate.Hour:00}{forDate.Minute:00}.csv";
        var fullPath = Path.Combine(_appConfigSettings.PowerPositionCSVLocation, fileName);
        return fullPath;
    }
}