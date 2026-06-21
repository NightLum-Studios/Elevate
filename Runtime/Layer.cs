using NightLum.Elevate.Composition;

namespace NightLum.Elevate.Core
{
    /// <summary>Describes one scalar-map contribution applied by <see cref="Composer"/>.</summary>
    public class Layer
    {
        /// <summary>Gets or sets the required source map.</summary>
        public ScalarMap Source { get; set; }
        /// <summary>Gets or sets an optional influence map. Null means full influence.</summary>
        public ScalarMap Influence { get; set; }
        /// <summary>Gets or sets the layer weight combined with per-cell influence.</summary>
        public float Weight { get; set; } = 1f;
        /// <summary>Gets or sets whether the layer participates in composition.</summary>
        public bool Enabled { get; set; } = true;
        /// <summary>Gets or sets the sort priority. Lower values are applied first.</summary>
        public int Priority { get; set; }
        /// <summary>Gets or sets the blend operation.</summary>
        public BlendMode Mode { get; set; } = BlendMode.Blend;
    }
}
