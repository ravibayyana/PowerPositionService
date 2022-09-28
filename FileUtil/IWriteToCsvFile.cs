using PowerPositionService.Utils;

namespace PowerPositionService.FileUtil;

public interface IWriteToCsvFile
{
    Task Write(PositionVolumes positionVolumes);
}