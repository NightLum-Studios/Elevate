using System;
using NightLum.Elevate.Composition;

namespace NightLum.Elevate.Core
{
    /// <summary>Provides the scalar formulas used by the built-in blend modes.</summary>
    public static class BlendModes
    {
        /// <summary>Applies one blend operation to a pair of scalar values.</summary>
        /// <param name="mode">The blend operation.</param>
        /// <param name="current">The accumulated value.</param>
        /// <param name="layer">The source-layer value.</param>
        /// <param name="rawStrength">Strength clamped to the range zero through one.</param>
        /// <returns>The blended value.</returns>
        public static float Apply(
            BlendMode mode,
            float current,
            float layer,
            float rawStrength)
        {
            if (float.IsNaN(rawStrength) || float.IsInfinity(rawStrength))
                throw new ArgumentException("Strength must be a finite number.", nameof(rawStrength));

            if (rawStrength <= 0f) return current;

            float strength = Clamp01(rawStrength);

            switch (mode)
            {
                case BlendMode.Add:
                    return current + layer * strength;

                case BlendMode.Blend:
                    return (1f - strength) * current +
                           strength * layer;

                case BlendMode.Max:
                    {
                        float target = Math.Max(current, layer);

                        return (1f - strength) * current +
                               strength * target;
                    }

                case BlendMode.Min:
                    {
                        float target = Math.Min(current, layer);

                        return (1f - strength) * current +
                               strength * target;
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            return value > 1f ? 1f : value;
        }
    }
}
