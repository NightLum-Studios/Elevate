using System;

class HeightMap
{
    readonly int width;
    readonly int height;
    float[] data; // One-dimensional array to store height values
    public int Length => data.Length; // Length of the point array

    // Constructor: Creates a new empty map of the specified size
    public HeightMap(int width, int height)
    {
        this.width = width;
        this.height = height;

        CheckBounds();

        // Allocate memory for the array
        this.data = new float[width * height];
    }

    // Constructor: Creates a map based on an already existing data array
    public HeightMap(int width, int height, float[] data)
    {

        this.width = width;
        this.height = height;

        CheckBounds();

        if (data.Length != width * height)
            throw new ArgumentException("Data size mismatch");

        this.data = data;
    }

    // Provides the height from a specific point
    public float Get(int x, int y)
    {
        CheckBounds(x, y);

        return (data[Index(x, y)]);
    }

    // Sets the height at a specific point
    public void Set(int x, int y, float value)
    {
        CheckBounds(x, y);

        data[Index(x, y)] = value;
    }

    // Provides a list of all points
    public ReadOnlySpan<float> GetRawData()
    {
        return data.AsSpan();
    }

    // Creates a copy of the height map
    public HeightMap Clone()
    {
        float[] newData = new float[data.Length]; // Allocate memory for the new array
        Array.Copy(data, newData, data.Length); // Copy numbers into the new array

        return new HeightMap(width, height, newData);
    }

    // Copies height values from this map to another existing map
    public void CopyTo(HeightMap target)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));

        // Check if both maps have the same dimensions
        if (target.width != width || target.height != height)
            throw new ArgumentException("Size mismatch");

        // Copy data into the target map's array
        Array.Copy(data, target.data, data.Length);
    }

    #region Helper Methods
    // Converts two-dimensional coordinates (x, y) into a single array index
    int Index(int x, int y)
    {
        return (y * width + x);
    }

    // Check if the coordinates are out of the map bounds
    void CheckBounds()
    {
        if(width <= 0 || height <= 0)
        {
            throw new ArgumentException($"HeightMap size out of range");
        }
    }

    void CheckBounds(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            throw new ArgumentOutOfRangeException($"Coordinates ({x},{y}) out of bounds ({width},{height})");
        }
    }
    #endregion
}

class Program 
{
    public static void Main(string[] args)
    {
        HeightMap map = new HeightMap(10, 10);
        map.Set(9, 9, 5.5f);
        Console.WriteLine(map.Get(9, 9));
    }
}
