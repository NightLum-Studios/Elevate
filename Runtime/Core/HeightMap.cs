using System;

namespace NightLum.Elevate.Core {
    public sealed class HeightMap {
        private readonly float[] data;

        public int Width { get; }
        public int Height { get; }
        public int Length {
            get { return data.Length; }
        }

        public HeightMap(int width, int height) {
            ValidateDimensions(width, height);

            Width = width;
            Height = height;
            data = new float[checked(width * height)];
        }

        public HeightMap(int width, int height, float[] sourceData) {
            ValidateDimensions(width, height);
            if (sourceData == null) {
                throw new ArgumentNullException(nameof(sourceData));
            }

            int expectedLength = checked(width * height);
            if (sourceData.Length != expectedLength) {
                throw new ArgumentException(
                    $"HeightMap data length must be {expectedLength}, but was {sourceData.Length}.",
                    nameof(sourceData));
            }

            Width = width;
            Height = height;
            data = new float[expectedLength];
            Array.Copy(sourceData, data, expectedLength);
        }

        public float Get(int x, int y) {
            return data[GetIndex(x, y)];
        }

        public void Set(int x, int y, float value) {
            data[GetIndex(x, y)] = value;
        }

        // Returns the internal buffer intentionally for allocation-free runtime access.
        public float[] GetRawData() {
            return data;
        }

        public HeightMap Clone() {
            return new HeightMap(Width, Height, data);
        }

        public void CopyTo(HeightMap target) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }

            EnsureSameSize(this, target, nameof(target));
            Array.Copy(data, target.data, data.Length);
        }

        internal static void EnsureSameSize(HeightMap expected, HeightMap actual, string paramName) {
            if (expected == null) {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null) {
                throw new ArgumentNullException(paramName);
            }

            if (expected.Width != actual.Width || expected.Height != actual.Height) {
                throw new ArgumentException(
                    $"HeightMap size mismatch. Expected {expected.Width}x{expected.Height}, but got {actual.Width}x{actual.Height}.",
                    paramName);
            }
        }

        private int GetIndex(int x, int y) {
            if (x < 0 || x >= Width) {
                throw new ArgumentOutOfRangeException(nameof(x), x, $"x must be within 0..{Width - 1}.");
            }

            if (y < 0 || y >= Height) {
                throw new ArgumentOutOfRangeException(nameof(y), y, $"y must be within 0..{Height - 1}.");
            }

            return y * Width + x;
        }

        private static void ValidateDimensions(int width, int height) {
            if (width <= 0) {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than zero.");
            }

            if (height <= 0) {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than zero.");
            }

            checked {
                _ = width * height;
            }
        }
    }
}
