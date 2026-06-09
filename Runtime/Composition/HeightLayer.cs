using NightLum.Elevate.Core;

namespace NightLum.Elevate.Composition {
    public sealed class HeightLayer {
        public HeightMap Source { get; set; }
        public HeightMap Influence { get; set; }
        public float Weight { get; set; } = 1f;
        public bool Enabled { get; set; } = true;
        public int Priority { get; set; }
        public BlendMode Mode { get; set; } = BlendMode.Blend;
        public int SourceOrder { get; set; }
    }
}
