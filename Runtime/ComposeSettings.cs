using System;

namespace NightLum.Elevate.Core
{
    public class ComposeSettings
    {
        public bool ClampOutput { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; } = 1f;

        internal void Validate()
        {
            if (ClampOutput && MinValue > MaxValue)
                throw new ArgumentException("MinValue must be less than or equal to MaxValue when ClampOutput is enabled.");
        }
    }
}
