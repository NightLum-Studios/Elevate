using System;
using System.Collections.Generic;
using NightLum.Elevate.Core;

namespace NightLum.Elevate.Composition {
    public static class HeightComposer {
        public static HeightMap Compose(HeightMap baseHeight, IList<HeightLayer> layers, HeightComposeSettings settings) {
            if (baseHeight == null) {
                throw new ArgumentNullException(nameof(baseHeight));
            }

            if (layers == null) {
                throw new ArgumentNullException(nameof(layers));
            }

            settings ??= new HeightComposeSettings();
            settings.Validate();

            HeightMap output = baseHeight.Clone();
            if (layers.Count == 0) {
                ApplyOutputSettings(output, settings);
                return output;
            }

            List<LayerEntry> enabledLayers = PrepareLayers(baseHeight, layers);
            enabledLayers.Sort(CompareLayers);

            float[] outputData = output.GetRawData();
            int length = output.Length;
            for (int layerIndex = 0; layerIndex < enabledLayers.Count; layerIndex++) {
                HeightLayer layer = enabledLayers[layerIndex].Layer;
                float[] sourceData = layer.Source.GetRawData();
                float[] influenceData = layer.Influence == null ? null : layer.Influence.GetRawData();
                float weight = layer.Weight;

                switch (layer.Mode) {
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

        public static HeightMap Compose(HeightMap baseHeight, IEnumerable<HeightLayer> layers, HeightComposeSettings settings = null) {
            if (layers == null) {
                throw new ArgumentNullException(nameof(layers));
            }

            if (layers is IList<HeightLayer> list) {
                return Compose(baseHeight, list, settings);
            }

            return Compose(baseHeight, new List<HeightLayer>(layers), settings);
        }

        private static List<LayerEntry> PrepareLayers(HeightMap baseHeight, IList<HeightLayer> layers) {
            List<LayerEntry> enabledLayers = new List<LayerEntry>(layers.Count);
            for (int i = 0; i < layers.Count; i++) {
                HeightLayer layer = layers[i];
                if (layer == null) {
                    throw new ArgumentException($"Layer at index {i} is null.", nameof(layers));
                }

                if (!layer.Enabled) {
                    continue;
                }

                ValidateLayer(baseHeight, layer, i);
                enabledLayers.Add(new LayerEntry(layer, i));
            }

            return enabledLayers;
        }

        private static void ValidateLayer(HeightMap baseHeight, HeightLayer layer, int index) {
            if (layer.Source == null) {
                throw new ArgumentException($"Layer at index {index} has no Source.", nameof(layer));
            }

            if (float.IsNaN(layer.Weight) || float.IsInfinity(layer.Weight)) {
                throw new ArgumentException($"Layer at index {index} has invalid Weight.", nameof(layer));
            }

            if (layer.Weight < 0f) {
                throw new ArgumentException($"Layer at index {index} has negative Weight. Negative weights are not supported in the MVP.", nameof(layer));
            }

            HeightMap.EnsureSameSize(baseHeight, layer.Source, nameof(layer.Source));
            if (layer.Influence != null) {
                HeightMap.EnsureSameSize(baseHeight, layer.Influence, nameof(layer.Influence));
            }
        }

        private static int CompareLayers(LayerEntry left, LayerEntry right) {
            int priority = left.Layer.Priority.CompareTo(right.Layer.Priority);
            if (priority != 0) {
                return priority;
            }

            int sourceOrder = left.Layer.SourceOrder.CompareTo(right.Layer.SourceOrder);
            if (sourceOrder != 0) {
                return sourceOrder;
            }

            return left.OriginalIndex.CompareTo(right.OriginalIndex);
        }

        private static void ApplyAdd(float[] output, float[] source, float[] influence, float weight, int length) {
            if (influence == null) {
                for (int i = 0; i < length; i++) {
                    output[i] = output[i] + source[i] * weight;
                }
                return;
            }

            for (int i = 0; i < length; i++) {
                output[i] = output[i] + source[i] * influence[i] * weight;
            }
        }

        private static void ApplyBlend(float[] output, float[] source, float[] influence, float weight, int length) {
            if (influence == null) {
                for (int i = 0; i < length; i++) {
                    output[i] = Lerp(output[i], source[i], weight);
                }
                return;
            }

            for (int i = 0; i < length; i++) {
                output[i] = Lerp(output[i], source[i], influence[i] * weight);
            }
        }

        private static void ApplyMax(float[] output, float[] source, float[] influence, float weight, int length) {
            if (influence == null) {
                for (int i = 0; i < length; i++) {
                    float target = output[i] > source[i] ? output[i] : source[i];
                    output[i] = Lerp(output[i], target, weight);
                }
                return;
            }

            for (int i = 0; i < length; i++) {
                float target = output[i] > source[i] ? output[i] : source[i];
                output[i] = Lerp(output[i], target, influence[i] * weight);
            }
        }

        private static void ApplyMin(float[] output, float[] source, float[] influence, float weight, int length) {
            if (influence == null) {
                for (int i = 0; i < length; i++) {
                    float target = output[i] < source[i] ? output[i] : source[i];
                    output[i] = Lerp(output[i], target, weight);
                }
                return;
            }

            for (int i = 0; i < length; i++) {
                float target = output[i] < source[i] ? output[i] : source[i];
                output[i] = Lerp(output[i], target, influence[i] * weight);
            }
        }

        private static void ApplyOutputSettings(HeightMap output, HeightComposeSettings settings) {
            if (!settings.ClampOutput) {
                return;
            }

            float[] data = output.GetRawData();
            for (int i = 0; i < data.Length; i++) {
                data[i] = Clamp(data[i], settings.MinValue, settings.MaxValue);
            }
        }

        private static float Lerp(float current, float target, float strength) {
            return current + (target - current) * strength;
        }

        private static float Clamp(float value, float min, float max) {
            if (value < min) {
                return min;
            }

            return value > max ? max : value;
        }

        private readonly struct LayerEntry {
            public readonly HeightLayer Layer;
            public readonly int OriginalIndex;

            public LayerEntry(HeightLayer layer, int originalIndex) {
                Layer = layer;
                OriginalIndex = originalIndex;
            }
        }
    }
}
