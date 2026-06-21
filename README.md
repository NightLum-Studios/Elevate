# Elevate

Elevate is a standalone .NET/Unity runtime library for blending two-dimensional scalar fields such as heightmaps, influence maps, and density maps.

It is a pure data-processing core. The runtime assembly has no dependencies on UnityEngine, UnityEditor, Texture2D, file I/O, graph frameworks, or noise libraries.

## Installation

The Elevate folder is a valid Unity Package Manager package. Add it as a local package, a Git dependency, or copy it into a project's `Packages` directory.

Projects that embed Elevate under `Assets` can reference the `Elevate.Runtime` assembly definition directly. Non-Unity .NET projects can compile the C# files under `Runtime`.

## Quick Start

```csharp
using NightLum.Elevate.Composition;
using NightLum.Elevate.Core;

var baseMap = new ScalarMap(1024, 1024);
var source = new ScalarMap(1024, 1024);
var layer = new Layer
{
    Source = source,
    Mode = BlendMode.Add,
    Weight = 0.3f
};
var settings = new ComposeSettings
{
    ClampOutput = true,
    MaxValue = 100f
};

ScalarMap result = Composer.Compose(baseMap, new[] { layer }, settings);
```

## Source Adapters

Source adapters belong outside the runtime core. An adapter only needs to create and populate a `ScalarMap`; adding support for Terrain Graph, textures, noise, or files requires no changes to Elevate.

```csharp
// This type belongs in an external integration assembly.
public static class ExampleSourceAdapter
{
    public static ScalarMap Convert(float[,] source)
    {
        int width = source.GetLength(0);
        int height = source.GetLength(1);
        var result = new ScalarMap(width, height);

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            result.Set(x, y, source[x, y]);

        return result;
    }
}
```

## Public API

The MVP surface consists of `ScalarMap`, `Layer`, `BlendMode`, `ComposeSettings`, `Composer`, and the `BlendModes` formula helper. All public members include XML documentation.

The full MVP specification is in [Roadmap.md](Roadmap.md).

## Development Rules

- No LINQ, reflection, delegates, or per-pixel allocation in composition hot paths.
- No temporary `ScalarMap` allocation per layer.
- No hidden normalization.
- Input maps are never modified.

## License

NightLum Studios License (NSL) v1.2
