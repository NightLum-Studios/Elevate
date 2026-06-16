# Elevate Benchmarks

These are the first Stage 5 benchmark scenarios for the `ScalarMap` MVP API. They are intentionally simple and focus on detecting obvious performance regressions in `Composer.Compose`.

## Scope

Measure composition time for:

- map sizes: `512x512`, `1024x1024`, `2048x2048`;
- layer counts: `1`, `4`, `8`, `16`;
- with and without influence maps;
- mixed `Add`, `Blend`, `Max`, and `Min` modes.

## Manual Runner Sketch

```csharp
using System;
using System.Diagnostics;
using NightLum.Elevate.Composition;
using NightLum.Elevate.Core;

static void Run(int size, int layerCount, bool useInfluence)
{
    ScalarMap baseMap = CreateMap(size, 0.25f);
    Layer[] layers = new Layer[layerCount];

    for (int i = 0; i < layerCount; i++)
    {
        layers[i] = new Layer
        {
            Source = CreateMap(size, 0.1f + i * 0.01f),
            Influence = useInfluence ? CreateMap(size, 0.5f) : null,
            Weight = 0.5f,
            Priority = i,
            Mode = (BlendMode)(i % 4)
        };
    }

    Stopwatch stopwatch = Stopwatch.StartNew();
    ScalarMap result = Composer.Compose(baseMap, layers, new ComposeSettings());
    stopwatch.Stop();

    Console.WriteLine($"{size}x{size}, layers={layerCount}, influence={useInfluence}: {stopwatch.ElapsedMilliseconds} ms, result={result.Get(0, 0)}");
}

static ScalarMap CreateMap(int size, float value)
{
    ScalarMap map = new ScalarMap(size, size);
    map.Fill(value);
    return map;
}
```

## Current Results

Record machine, Unity version, scripting backend and timings here when benchmarks are run.

| Size | Layers | Influence | Time |
| --- | ---: | --- | ---: |
| 512x512 | 1 | no | TBD |
| 512x512 | 4 | yes | TBD |
| 1024x1024 | 8 | yes | TBD |
| 2048x2048 | 16 | yes | TBD |

## Stage 5 Constraints Checked In Code

- `Composer` uses raw `float[]` data in the per-pixel loop.
- No LINQ is used in the composition hot path.
- Layers are filtered and sorted once before pixel processing.
- No temporary `ScalarMap` is allocated per layer; only the final output map is allocated.
