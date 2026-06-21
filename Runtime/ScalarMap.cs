using System;

namespace NightLum.Elevate.Core
{
    /// <summary>
    /// Stores a fixed-size, two-dimensional scalar field in row-major order.
    /// </summary>
    public class ScalarMap
    {
        private readonly float[] _data;

        /// <summary>Gets the number of columns in the map.</summary>
        public int Width { get; }
        /// <summary>Gets the number of rows in the map.</summary>
        public int Height { get; }
        /// <summary>Gets the total number of scalar values in the map.</summary>
        public int Length => _data.Length;

        /// <summary>Initializes an empty map with the specified dimensions.</summary>
        /// <param name="width">The number of columns. Must be greater than zero.</param>
        /// <param name="height">The number of rows. Must be greater than zero.</param>
        public ScalarMap(int width, int height)
        {
            int length = ValidateDimensions(width, height);

            Width = width;
            Height = height;

            _data = new float[length];
        }

        /// <summary>Initializes a map by copying values from a row-major array.</summary>
        /// <param name="width">The number of columns. Must be greater than zero.</param>
        /// <param name="height">The number of rows. Must be greater than zero.</param>
        /// <param name="data">Values whose length equals width times height.</param>
        public ScalarMap(int width, int height, float[] data)
        {
            int expectedLength = ValidateDimensions(width, height);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Length != expectedLength)
                throw new ArgumentException(
                    "Data length must equal width * height.",
                    nameof(data));

            Width = width;
            Height = height;

            // Deep copy
            _data = (float[])data.Clone();
        }

        /// <summary>Gets the value at a coordinate.</summary>
        /// <param name="x">The zero-based column index.</param>
        /// <param name="y">The zero-based row index.</param>
        /// <returns>The stored value.</returns>
        /// <remarks>Coordinate validation is enabled in DEBUG builds only.</remarks>
        public float Get(int x, int y)
        {
            return _data[GetIndex(x, y)];
        }

        /// <summary>Sets the value at a coordinate.</summary>
        /// <param name="x">The zero-based column index.</param>
        /// <param name="y">The zero-based row index.</param>
        /// <param name="value">The value to store.</param>
        /// <remarks>Coordinate validation is enabled in DEBUG builds only.</remarks>
        public void Set(int x, int y, float value)
        {
            _data[GetIndex(x, y)] = value;
        }

        /// <summary>Gets the mutable internal row-major array without copying it.</summary>
        /// <returns>The internal storage array.</returns>
        public float[] GetRawData()
        {
            return _data;
        }

        /// <summary>Gets a mutable span over the internal row-major storage.</summary>
        /// <returns>A span whose length equals <see cref="Length"/>.</returns>
        public Span<float> AsSpan()
        {
            return _data.AsSpan();
        }

        /// <summary>Copies every value into a map with matching dimensions.</summary>
        /// <param name="target">The destination map.</param>
        public void CopyTo(ScalarMap target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (target.Width != Width || target.Height != Height)
                throw new ArgumentException("Target map size must match source map size.",
                    nameof(target));

            Array.Copy(_data, target._data, _data.Length);
        }

        /// <summary>Creates an independent deep copy of this map.</summary>
        /// <returns>A new map containing the same values.</returns>
        public ScalarMap Clone()
        {
            return new ScalarMap(Width, Height, _data);
        }

        /// <summary>Assigns the same value to every cell.</summary>
        /// <param name="value">The value to assign.</param>
        public void Fill(float value)
        {
            Array.Fill(_data, value);
        }

        /// <summary>Sets every cell to zero.</summary>
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

        private static int ValidateDimensions(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentException("Width must be greater than zero.",
                    nameof(width));

            if (height <= 0)
                throw new ArgumentException("Height must be greater than zero.",
                    nameof(height));

            return checked(width * height);
        }
    }
}
