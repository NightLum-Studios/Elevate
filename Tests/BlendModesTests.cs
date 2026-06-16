using System;
using NightLum.Elevate.Composition;
using NightLum.Elevate.Core;
using NUnit.Framework;

namespace NightLum.Elevate.Tests
{
    public sealed class BlendModesTests
    {
        [TestCase(BlendMode.Add, 10f, 4f, 0f, 10f)]
        [TestCase(BlendMode.Add, 10f, 4f, 0.5f, 12f)]
        [TestCase(BlendMode.Add, 10f, 4f, 1f, 14f)]
        [TestCase(BlendMode.Add, 10f, 4f, 2f, 14f)]
        [TestCase(BlendMode.Add, 10f, 4f, -1f, 10f)]
        [TestCase(BlendMode.Blend, 10f, 4f, 0f, 10f)]
        [TestCase(BlendMode.Blend, 10f, 4f, 0.5f, 7f)]
        [TestCase(BlendMode.Blend, 10f, 4f, 1f, 4f)]
        [TestCase(BlendMode.Blend, 10f, 4f, 2f, 4f)]
        [TestCase(BlendMode.Blend, 10f, 4f, -1f, 10f)]
        [TestCase(BlendMode.Max, 10f, 20f, 0f, 10f)]
        [TestCase(BlendMode.Max, 10f, 20f, 0.5f, 15f)]
        [TestCase(BlendMode.Max, 10f, 20f, 1f, 20f)]
        [TestCase(BlendMode.Max, 10f, 5f, 1f, 10f)]
        [TestCase(BlendMode.Min, 10f, 4f, 0f, 10f)]
        [TestCase(BlendMode.Min, 10f, 4f, 0.5f, 7f)]
        [TestCase(BlendMode.Min, 10f, 4f, 1f, 4f)]
        [TestCase(BlendMode.Min, 10f, 20f, 1f, 10f)]
        public void ApplyMatchesRoadmapFormula(BlendMode mode, float current, float layer, float strength, float expected)
        {
            Assert.AreEqual(expected, BlendModes.Apply(mode, current, layer, strength), 0.0001f);
        }

        [Test]
        public void ApplyRejectsInvalidStrength()
        {
            Assert.Throws<ArgumentException>(() => BlendModes.Apply(BlendMode.Add, 0f, 1f, float.NaN));
            Assert.Throws<ArgumentException>(() => BlendModes.Apply(BlendMode.Add, 0f, 1f, float.PositiveInfinity));
        }

        [Test]
        public void ApplyRejectsUnsupportedMode()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BlendModes.Apply((BlendMode)999, 0f, 1f, 1f));
        }
    }
}
