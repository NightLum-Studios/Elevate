using System;

namespace NightLum.Elevate.Core
{
    public class ScalarMap
    {
        private readonly float[] _data;

        public int Width { get; }
        public int Height { get; }
        public int Length => _data.Length;

        public ScalarMap(int width, int height)
        {
            ValidateDimensions(width, height);

            Width = width;
            Height = height;

            _data = new float[width * height];
        }

        public ScalarMap(int width, int height, float[] data)
        {
            ValidateDimensions(width, height);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Length != width * height)
                throw new ArgumentException(
                    "Data length must equal width * height.",
                    nameof(data));

            Width = width;
            Height = height;

            // Deep copy
            _data = (float[])data.Clone();
        }

        public float Get(int x, int y)
        {
            return _data[GetIndex(x, y)];
        }

        public void Set(int x, int y, float value)
        {
            _data[GetIndex(x, y)] = value;
        }

        public float[] GetRawData() // returns internal array (direct, unsafe)
        {
            return _data;
        }

        public Span<float> AsSpan() // safe span without copy
        {
            return _data.AsSpan();
        }

        public void CopyTo(ScalarMap target) // sizes must match
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (target.Width != Width || target.Height != Height)
                throw new ArgumentException("Target map size must match source map size.",
                    nameof(target));

            Array.Copy(_data, target._data, _data.Length);
        }

        public ScalarMap Clone() // deep copy
        {
            return new ScalarMap(Width, Height, _data);
        }

        public void Fill(float value)
        {
            Array.Fill(_data, value);
        }

        public void Clear()
        {
            Fill(0f);
        }

        private int GetIndex(int x, int y)
        {
#if DEBUG
            if (x < 0 || x >= Width)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException(nameof(y));
#endif

            return y * Width + x;
        }

        private static void ValidateDimensions(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentException("Width must be greater than zero.",
                    nameof(width));

            if (height <= 0)
                throw new ArgumentException("Height must be greater than zero.",
                    nameof(height));

            checked
            {
                _ = width * height;
            }
        }
    }
}
