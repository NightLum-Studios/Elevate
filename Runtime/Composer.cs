using System;
using System.Collections.Generic;
using NightLum.Elevate.Composition;

namespace NightLum.Elevate.Core
{
    /// <summary>Composes a base scalar map with an ordered collection of layers.</summary>
    public static class Composer
    {
        /// <summary>Creates a new map by applying enabled layers to a copy of the base map.</summary>
        /// <param name="baseMap">The required base map. It is never modified.</param>
        /// <param name="layers">The layers to validate, sort, and apply.</param>
        /// <param name="settings">The required output settings.</param>
        /// <returns>A newly allocated map containing the composition result.</returns>
        public static ScalarMap Compose(
            ScalarMap baseMap,
            IReadOnlyList<Layer> layers,
            ComposeSettings settings)
        {
            if (baseMap == null)
                throw new ArgumentNullException(nameof(baseMap));

            if (layers == null)
                throw new ArgumentNullException(nameof(layers));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            settings.Validate();

            ScalarMap output = baseMap.Clone();
            if (layers.Count == 0)
            {
                ApplyOutputSettings(output, settings);
                return output;
            }

            List<LayerEntry> enabledLayers = PrepareLayers(baseMap, layers);
            enabledLayers.Sort(CompareLayers);

            float[] outputData = output.GetRawData();
            int length = output.Length;

            for (int layerIndex = 0; layerIndex < enabledLayers.Count; layerIndex++)
            {
                Layer layer = enabledLayers[layerIndex].Layer;
                float[] sourceData = layer.Source.GetRawData();
                float[] influenceData = layer.Influence == null ? null : layer.Influence.GetRawData();
                float weight = layer.Weight;

                switch (layer.Mode)
                {
                    case BlendMode.Add:
                        ApplyAdd(outputData, sourceData, influenceData, weight, length);
                        break;
                    case BlendMode.Blend:
                        ApplyBlend(outputData, sourceData, influenceData, weight, length);
                        break;
                    case BlendMode.Max:
                        ApplyMax(outputData, sourceData, influenceData, weight, length);
                        break;
                    case BlendMode.Min:
                        ApplyMin(outputData, sourceData, influenceData, weight, length);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(layer.Mode), layer.Mode, "Unsupported blend mode.");
                }
            }

            ApplyOutputSettings(output, settings);
            return output;
        }

        private static List<LayerEntry> PrepareLayers(ScalarMap baseMap, IReadOnlyList<Layer> layers)
        {
            List<LayerEntry> enabledLayers = new List<LayerEntry>(layers.Count);
            for (int i = 0; i < layers.Count; i++)
            {
                Layer layer = layers[i];
                if (layer == null)
                    throw new ArgumentException($"Layer at index {i} is null.", nameof(layers));

                if (!layer.Enabled)
                    continue;

                ValidateLayer(baseMap, layer, i);
                enabledLayers.Add(new LayerEntry(layer, i));
            }

            return enabledLayers;
        }

        private static void ValidateLayer(ScalarMap baseMap, Layer layer, int index)
        {
            if (layer.Source == null)
                throw new ArgumentException($"Layer at index {index} has no Source.", nameof(layer));

            if (float.IsNaN(layer.Weight) || float.IsInfinity(layer.Weight))
                throw new ArgumentException($"Layer at index {index} has invalid Weight.", nameof(layer));

            EnsureSameSize(baseMap, layer.Source, nameof(layer.Source));
            if (layer.Influence != null)
                EnsureSameSize(baseMap, layer.Influence, nameof(layer.Influence));
        }

        private static int CompareLayers(LayerEntry left, LayerEntry right)
        {
            int priority = left.Layer.Priority.CompareTo(right.Layer.Priority);
            if (priority != 0)
                return priority;

            return left.OriginalIndex.CompareTo(right.OriginalIndex);
        }

        private static void ApplyAdd(float[] output, float[] source, float[] influence, float weight, int length)
        {
            if (influence == null)
            {
                float strength = ClampStrength(weight);
                for (int i = 0; i < length; i++)
                    output[i] = output[i] + source[i] * strength;

                return;
            }

            for (int i = 0; i < length; i++)
            {
                float strength = ClampStrength(influence[i] * weight);
                output[i] = output[i] + source[i] * strength;
            }
        }

        private static void ApplyBlend(float[] output, float[] source, float[] influence, float weight, int length)
        {
            if (influence == null)
            {
                float strength = ClampStrength(weight);
                for (int i = 0; i < length; i++)
                    output[i] = Lerp(output[i], source[i], strength);

                return;
            }

            for (int i = 0; i < length; i++)
            {
                float strength = ClampStrength(influence[i] * weight);
                output[i] = Lerp(output[i], source[i], strength);
            }
        }

        private static void ApplyMax(float[] output, float[] source, float[] influence, float weight, int length)
        {
            if (influence == null)
            {
                float strength = ClampStrength(weight);
                for (int i = 0; i < length; i++)
                {
                    float target = output[i] > source[i] ? output[i] : source[i];
                    output[i] = Lerp(output[i], target, strength);
                }

                return;
            }

            for (int i = 0; i < length; i++)
            {
                float strength = ClampStrength(influence[i] * weight);
                float target = output[i] > source[i] ? output[i] : source[i];
                output[i] = Lerp(output[i], target, strength);
            }
        }

        private static void ApplyMin(float[] output, float[] source, float[] influence, float weight, int length)
        {
            if (influence == null)
            {
                float strength = ClampStrength(weight);
                for (int i = 0; i < length; i++)
                {
                    float target = output[i] < source[i] ? output[i] : source[i];
                    output[i] = Lerp(output[i], target, strength);
                }

                return;
            }

            for (int i = 0; i < length; i++)
            {
                float strength = ClampStrength(influence[i] * weight);
                float target = output[i] < source[i] ? output[i] : source[i];
                output[i] = Lerp(output[i], target, strength);
            }
        }

        private static void ApplyOutputSettings(ScalarMap output, ComposeSettings settings)
        {
            if (!settings.ClampOutput)
                return;

            float[] data = output.GetRawData();
            for (int i = 0; i < data.Length; i++)
                data[i] = Clamp(data[i], settings.MinValue, settings.MaxValue);
        }

        private static float ClampStrength(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentException("Layer strength must be a finite number.");

            if (value <= 0f)
                return 0f;

            return value >= 1f ? 1f : value;
        }

        private static float Lerp(float current, float target, float strength)
        {
            return current + (target - current) * strength;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;

            return value > max ? max : value;
        }

        private static void EnsureSameSize(ScalarMap expected, ScalarMap actual, string paramName)
        {
            if (actual == null)
                throw new ArgumentNullException(paramName);

            if (expected.Width != actual.Width || expected.Height != actual.Height)
                throw new ArgumentException(
                    $"ScalarMap size mismatch. Expected {expected.Width}x{expected.Height}, but got {actual.Width}x{actual.Height}.",
                    paramName);
        }

        private readonly struct LayerEntry
        {
            public readonly Layer Layer;
            public readonly int OriginalIndex;

            public LayerEntry(Layer layer, int originalIndex)
            {
                Layer = layer;
                OriginalIndex = originalIndex;
            }
        }
    }
}
