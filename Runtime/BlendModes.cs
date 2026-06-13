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
            if (rawStrength <= 0f) return current;

            float strength = Math.Clamp(rawStrength, 0f, 1f);

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
    }
}