using System;
using NightLum.Elevate.Composition;

namespace NightLum.Elevate.Core
{
    public static class BlendModes
    {
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
