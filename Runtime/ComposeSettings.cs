using System;

namespace NightLum.Elevate.Core
{
    /// <summary>Configures final-output processing for a composition.</summary>
    public class ComposeSettings
    {
        /// <summary>Gets or sets whether the final output is clamped.</summary>
        public bool ClampOutput { get; set; }
        /// <summary>Gets or sets the lower clamp bound.</summary>
        public float MinValue { get; set; }
        /// <summary>Gets or sets the upper clamp bound.</summary>
        public float MaxValue { get; set; } = 1f;

        internal void Validate()
        {
            if (ClampOutput && MinValue > MaxValue)
                throw new ArgumentException("MinValue must be less than or equal to MaxValue when ClampOutput is enabled.");
        }
    }
}
