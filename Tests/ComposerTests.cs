using System;
using NightLum.Elevate.Composition;
using NightLum.Elevate.Core;
using NUnit.Framework;

namespace NightLum.Elevate.Tests
{
    public sealed class ComposerTests
    {
        [Test]
        public void ComposeRejectsNullInputs()
        {
            ScalarMap map = Map(1f);
            Layer[] layers = { new Layer { Source = Map(2f) } };
            ComposeSettings settings = new ComposeSettings();

            Assert.Throws<ArgumentNullException>(() => Composer.Compose(null, layers, settings));
            Assert.Throws<ArgumentNullException>(() => Composer.Compose(map, null, settings));
            Assert.Throws<ArgumentNullException>(() => Composer.Compose(map, layers, null));
        }

        [Test]
        public void ComposeRejectsInvalidSettings()
        {
            Assert.Throws<ArgumentException>(() => Composer.Compose(
                Map(0f),
                Array.Empty<Layer>(),
                new ComposeSettings { ClampOutput = true, MinValue = 2f, MaxValue = 1f }));
        }

        [Test]
        public void ComposeRejectsInvalidLayers()
        {
            ScalarMap baseMap = Map(0f);
            ComposeSettings settings = new ComposeSettings();

            Assert.Throws<ArgumentException>(() => Composer.Compose(baseMap, new Layer[] { null }, settings));
            Assert.Throws<ArgumentException>(() => Composer.Compose(baseMap, new[] { new Layer() }, settings));
            Assert.Throws<ArgumentException>(() => Composer.Compose(baseMap, new[] { new Layer { Source = new ScalarMap(1, 1) } }, settings));
            Assert.Throws<ArgumentException>(() => Composer.Compose(baseMap, new[] { new Layer { Source = Map(1f), Influence = new ScalarMap(1, 1) } }, settings));
            Assert.Throws<ArgumentException>(() => Composer.Compose(baseMap, new[] { new Layer { Source = Map(1f), Weight = float.NaN } }, settings));
        }

        [Test]
        public void EmptyLayerListReturnsCopyOfBaseMap()
        {
            ScalarMap baseMap = Map(1f, 2f, 3f, 4f);

            ScalarMap result = Composer.Compose(baseMap, Array.Empty<Layer>(), new ComposeSettings());

            CollectionAssert.AreEqual(baseMap.GetRawData(), result.GetRawData());
            Assert.AreNotSame(baseMap.GetRawData(), result.GetRawData());
        }

        [Test]
        public void DisabledAndZeroWeightLayersHaveNoEffect()
        {
            ScalarMap result = Composer.Compose(
                Map(1f),
                new[]
                {
                    new Layer { Source = Map(100f), Weight = 1f, Enabled = false, Mode = BlendMode.Add },
                    new Layer { Source = Map(100f), Weight = 0f, Mode = BlendMode.Add }
                },
                new ComposeSettings());

            Assert.AreEqual(1f, result.Get(0, 0));
        }

        [Test]
        public void InfluenceAndWeightAreCombinedAndClamped()
        {
            ScalarMap result = Composer.Compose(
                Map(10f, 10f, 10f, 10f),
                new[]
                {
                    new Layer
                    {
                        Source = Map(20f, 20f, 20f, 20f),
                        Influence = Map(0f, 0.5f, 1f, 2f),
                        Weight = 0.5f,
                        Mode = BlendMode.Blend
                    }
                },
                new ComposeSettings());

            CollectionAssert.AreEqual(new[] { 10f, 12.5f, 15f, 20f }, result.GetRawData());
        }

        [Test]
        public void LayersAreSortedByPriorityThenOriginalIndex()
        {
            ScalarMap result = Composer.Compose(
                Map(0f),
                new[]
                {
                    new Layer { Source = Map(1f), Weight = 1f, Priority = 1, Mode = BlendMode.Add },
                    new Layer { Source = Map(2f), Weight = 1f, Priority = 0, Mode = BlendMode.Blend },
                    new Layer { Source = Map(4f), Weight = 1f, Priority = 1, Mode = BlendMode.Add }
                },
                new ComposeSettings());

            Assert.AreEqual(7f, result.Get(0, 0));
        }

        [Test]
        public void ComposeSupportsAllBlendModes()
        {
            Assert.AreEqual(12f, ComposeSingle(10f, 4f, BlendMode.Add, 0.5f));
            Assert.AreEqual(7f, ComposeSingle(10f, 4f, BlendMode.Blend, 0.5f));
            Assert.AreEqual(15f, ComposeSingle(10f, 20f, BlendMode.Max, 0.5f));
            Assert.AreEqual(7f, ComposeSingle(10f, 4f, BlendMode.Min, 0.5f));
        }

        [Test]
        public void ClampOutputAppliesAfterComposition()
        {
            ScalarMap result = Composer.Compose(
                Map(10f),
                new[] { new Layer { Source = Map(10f), Weight = 1f, Mode = BlendMode.Add } },
                new ComposeSettings { ClampOutput = true, MinValue = 0f, MaxValue = 12f });

            Assert.AreEqual(12f, result.Get(0, 0));
        }

        [Test]
        public void ComposeDoesNotModifyInputs()
        {
            ScalarMap baseMap = Map(1f);
            ScalarMap source = Map(2f);
            ScalarMap influence = Map(0.5f);

            Composer.Compose(
                baseMap,
                new[] { new Layer { Source = source, Influence = influence, Weight = 1f, Mode = BlendMode.Add } },
                new ComposeSettings());

            Assert.AreEqual(1f, baseMap.Get(0, 0));
            Assert.AreEqual(2f, source.Get(0, 0));
            Assert.AreEqual(0.5f, influence.Get(0, 0));
        }

        private static float ComposeSingle(float current, float layer, BlendMode mode, float weight)
        {
            ScalarMap result = Composer.Compose(
                Map(current),
                new[] { new Layer { Source = Map(layer), Weight = weight, Mode = mode } },
                new ComposeSettings());
            return result.Get(0, 0);
        }

        private static ScalarMap Map(params float[] values)
        {
            if (values.Length == 1)
                return new ScalarMap(1, 1, values);

            return new ScalarMap(2, 2, values);
        }
    }
}
