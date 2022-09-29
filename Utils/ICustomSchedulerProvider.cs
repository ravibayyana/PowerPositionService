using System.Reactive.Concurrency;

namespace PowerPositionService.Utils;

public interface ICustomSchedulerProvider
{
    public IScheduler TaskPool { get; }
}