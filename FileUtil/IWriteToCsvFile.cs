using PowerPositionService.Utils;

namespace PowerPositionService.FileUtil;

public interface IWriteToCsvFile
{
    void Write(PositionVolumes positionVolumes);
}