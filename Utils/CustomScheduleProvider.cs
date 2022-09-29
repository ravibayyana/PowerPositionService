using System.Reactive.Concurrency;

namespace PowerPositionService.Utils;

public sealed class CustomScheduleProvider : ICustomSchedulerProvider
{
    public IScheduler TaskPool => TaskPoolScheduler.Default;

}