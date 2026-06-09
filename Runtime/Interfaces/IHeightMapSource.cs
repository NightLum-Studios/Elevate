using NightLum.Elevate.Core;

namespace NightLum.Elevate.Interfaces {
    public interface IHeightMapSource {
        int Width { get; }
        int Height { get; }
        HeightMap CreateHeightMap();
    }
}
