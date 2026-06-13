using System;
using NightLum.Elevate.Composition;

namespace NightLum.Elevate.Core
{
    public class Layer
    {
        public ScalarMap Source { get; set; }
        public ScalarMap Influence { get; set; }
        public float Weight { get; set; } = 1f;
        public bool Enabled { get; set; } = true;
        public int Priority { get; set; }
        public BlendMode Mode { get; set; }
    }
}
