namespace PowerPositionService.Setttings
{
    public interface IAppConfigSettings
    {
        int ScheduleIntervalInMinutes { get; }

        string PowerPositionCSVLocation { get; }
    }
}