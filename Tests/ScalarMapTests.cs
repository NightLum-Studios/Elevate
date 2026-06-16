using System;
using NightLum.Elevate.Core;
using NUnit.Framework;

namespace NightLum.Elevate.Tests
{
    public sealed class ScalarMapTests
    {
        [Test]
        public void ConstructorCreatesExplicitSizedMap()
        {
            ScalarMap map = new ScalarMap(3, 2);

            Assert.AreEqual(3, map.Width);
            Assert.AreEqual(2, map.Height);
            Assert.AreEqual(6, map.Length);
        }

        [Test]
        public void ConstructorRejectsInvalidDimensions()
        {
            Assert.Throws<ArgumentException>(() => new ScalarMap(0, 1));
            Assert.Throws<ArgumentException>(() => new ScalarMap(1, 0));
        }

        [Test]
        public void ConstructorCopiesSourceData()
        {
            float[] data = { 1f, 2f, 3f, 4f };
            ScalarMap map = new ScalarMap(2, 2, data);

            data[0] = 99f;

            Assert.AreEqual(1f, map.Get(0, 0));
        }

        [Test]
        public void ConstructorRejectsWrongDataLength()
        {
            Assert.Throws<ArgumentException>(() => new ScalarMap(2, 2, new float[3]));
        }

        [Test]
        public void ConstructorRejectsOverflowingDimensionsBeforeAllocation()
        {
            Assert.Throws<OverflowException>(() => new ScalarMap(int.MaxValue, int.MaxValue));
        }

        [Test]
        public void GetAndSetUseRowMajorCoordinates()
        {
            ScalarMap map = new ScalarMap(3, 2);

            map.Set(2, 1, 7.5f);

            Assert.AreEqual(7.5f, map.Get(2, 1));
            Assert.AreEqual(7.5f, map.GetRawData()[5]);
        }

#if DEBUG
        [Test]
        public void GetAndSetRejectOutOfRangeCoordinatesInDebugBuilds()
        {
            ScalarMap map = new ScalarMap(2, 2);

            Assert.Throws<ArgumentOutOfRangeException>(() => map.Get(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => map.Set(0, 2, 1f));
        }
#endif

        [Test]
        public void CopyToRequiresSameSizeAndCopiesValues()
        {
            ScalarMap source = new ScalarMap(2, 2, new[] { 1f, 2f, 3f, 4f });
            ScalarMap target = new ScalarMap(2, 2);

            source.CopyTo(target);

            Assert.AreEqual(3f, target.Get(0, 1));
            Assert.Throws<ArgumentException>(() => source.CopyTo(new ScalarMap(1, 4)));
        }

        [Test]
        public void CloneCreatesIndependentCopy()
        {
            ScalarMap source = new ScalarMap(2, 1, new[] { 1f, 2f });
            ScalarMap clone = source.Clone();

            source.Set(0, 0, 9f);

            Assert.AreEqual(1f, clone.Get(0, 0));
            Assert.AreNotSame(source.GetRawData(), clone.GetRawData());
        }

        [Test]
        public void FillAndClearWriteAllValues()
        {
            ScalarMap map = new ScalarMap(2, 2);

            map.Fill(3f);
            CollectionAssert.AreEqual(new[] { 3f, 3f, 3f, 3f }, map.GetRawData());

            map.Clear();
            CollectionAssert.AreEqual(new[] { 0f, 0f, 0f, 0f }, map.GetRawData());
        }
    }
}
