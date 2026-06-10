# Elevate Technical Specification (Russian, corrected version)

## Document Purpose

This document defines the requirements for the first version (MVP) of the Elevate library. It describes what the library does and does not do, architectural constraints, public API structure, implementation stages, and completion criteria. The document is intended for developers implementing Elevate. It focuses on facts, not opinions.

## Library Summary

Elevate is a backend library for blending two-dimensional scalar fields (heightmaps, influence maps, density, etc.).

**Core responsibility:** Accept a base map, apply a set of layers (each with blend mode, weight, and optional influence mask), and return the final map.

**Typical usage:**
```csharp
var baseMap = new ScalarMap(1024, 1024);
var layer = new Layer(baseMap, BlendMode.Add, weight: 0.5f);
var result = Composer.Compose(baseMap, layers, settings);
```

**What Elevate is NOT responsible for:** terrain, mesh, voxel, or any visual output; scene objects, materials, or UI; data acquisition (noise, textures, files) - that belongs to adapters built on top.

**What Elevate IS responsible for:** storing 2D float data (ScalarMap); describing a layer (Layer); blending modes (BlendMode); composing multiple layers deterministically; validating input sizes; clamping output values; providing basic tests and benchmarks.

## Architectural Principle

Elevate is a pure data-processing core. It has no knowledge of where data comes from or where the result goes.

External system -> (optional adapter) -> ScalarMap -> Layer -> Composer -> final ScalarMap -> External system uses result.

The core must not depend on: Unity Editor API; any specific noise library (FastNoiseLite, etc.); Texture2D or file I/O; visualization or rendering code.

## Stage 1 - Core Data Model: ScalarMap

**Goal:** Create a fixed-size 2D container for float values. The name ScalarMap (not HeightMap) reflects the library's future use for any scalar field.

### Class: ScalarMap

### Storage
internal private readonly float[] _data (1D array, row-major order). Index formula: index = y * Width + x.

### Properties:
public int Width get;
public int Height get;
public int Length get; // Width * Height

### Constructors:
public ScalarMap(int width, int height);
public ScalarMap(int width, int height, float[] data); // copies data

### Validation
width > 0 and height > 0. If data is provided: data.Length == width * height. Violation causes ArgumentException.

### Element access:
public float Get(int x, int y);
public void Set(int x, int y, float value);

Valid x: 0 .. Width-1, valid y: 0 .. Height-1. In DEBUG builds: out-of-range -> ArgumentOutOfRangeException. In RELEASE builds: no bounds check (caller responsibility for performance).

Zero is a valid coordinate. The first element is at (0,0).

### Bulk operations:
public float[] GetRawData(); // returns internal array (direct, unsafe)
public Span<float> AsSpan(); // safe span without copy
public void CopyTo(ScalarMap target); // sizes must match
public ScalarMap Clone(); // deep copy
public void Fill(float value);
public void Clear(); // Fill(0)

### Performance note
GetRawData() and AsSpan() allow zero-allocation bulk processing. Modifying the returned array directly affects the map - this is intentional for performance, but documented as unsafe for general use.

No public parameterless constructor. Every map must have explicit dimensions.

## Stage 1 Completion Criteria:
- ScalarMap exists with all above members
- All validation works
- No dependency on Unity Editor or external generators
- Tests cover creation, access, copy, fill, and clone

## Stage 2 - Layer

**Goal:** Define one layer that can be applied during composition.

### Class: Layer

```csharp
public class Layer
{
public ScalarMap Source get; set; // required, not null
public ScalarMap Influence get; set; // optional, can be null
public float Weight get; set; = 1f;
public bool Enabled get; set; = true;
public int Priority get; set; = 0;
public BlendMode Mode get; set;
}
```

### Notes:
- Source must be non-null before composition.
- If Influence is null -> treated as full influence (1.0 everywhere).
- Influence values are expected in [0, 1]. The final strength value is clamped by the library before applying blend formulas.

### Weight and influence combination:
effectiveStrength = influenceValue * Weight.

This effectiveStrength is clamped to [0, 1] before being used in blend formulas (see Stage 3).

### Layer Sorting:
Sorting is performed only during composition:
- Primary key: Priority (ascending, lower = earlier)
- Secondary key: original index in the input list (stable sort)

No SourceOrder field - it is removed to reduce API surface. Stable sorting by original order is sufficient and less error-prone.

## Stage 2 Completion Criteria:
- Layer class exists with all fields
- Validation: Source not null, sizes match base map (during composition)
- Influence optional, null = full influence
- Weight and influence combined correctly
- Sorting uses Priority + original index (no SourceOrder)

## Stage 3 - Blend Modes

**Goal:** Implement the four basic blending operations. Each mode uses the current accumulated value and the layer's value after scaling.

### Notation:
- current: value from the output map before this layer
- layer: value from Source at the same coordinate
- rawStrength = influenceValue * Weight
- strength = clamp(rawStrength, 0, 1)

### Important
For all modes, if rawStrength <= 0, the layer has no effect. If rawStrength >= 1, the mode behaves as if fully applied. Blend formulas always receive strength clamped to [0, 1].

Strength is clamped to [0,1] per pixel before use.

### Mode: Add
**Formula:** result = current + layer * strength
**Use case:** Adding hills, mountains, noise on top of existing terrain.
Note: Can push values far outside original range - use ClampOutput in settings if needed.

### Mode: Blend (linear interpolation)
**Formula:** result = (1 - strength) * current + strength * layer
**Use case:** Smooth transition from current to layer.

### Mode: Max (upward only)
**Formula:** target = max(current, layer) then result = (1 - strength) * current + strength * target
**Effect:** Gradually raises values toward the maximum of current and layer, never lowers.
Strength controls how much to move from current toward target (max).
**Use case:** Adding mountain shapes without destroying valleys.

### Mode: Min (downward only)
**Formula:** target = min(current, layer) then result = (1 - strength) * current + strength * target
**Effect:** Gradually lowers values toward the minimum, never raises.
Strength controls how much to move from current toward target (min).
**Use case:** Valleys, riverbeds, cuts.

Why this form for Max/Min? The two-step formula ensures that when strength=1, the result is exactly max(current,layer) or min(current,layer). When strength=0, no change. This matches typical terrain blending expectations.

### Enumeration:
```csharp
public enum BlendMode Add, Blend, Max, Min
```

### Not in MVP
Multiply, Subtract, Overlay, Difference, Replace, Custom.

## Stage 3 Completion Criteria:
- All four modes implemented as described
- Each mode covered by tests (edge cases: strength 0, 0.5, 1, >1, negative)
- No hidden normalization; strength clamping is explicit and happens before applying modes

## Stage 4 - Composer

**Goal:** Perform the actual composition of base map + layers -> final map.

### Class: Composer

```csharp
public static class Composer
{
public static ScalarMap Compose(
ScalarMap baseMap,
IReadOnlyList<Layer> layers,
ComposeSettings settings
);
}
```

Composer is static because it has no state.

### Behaviour:
1. Validate inputs: baseMap not null, Width/Height > 0; settings not null; layers not null (empty list is allowed)
2. Filter Enabled == true
3. For each enabled layer: Source not null; Source.Width == baseMap.Width and Source.Height == baseMap.Height; if Influence not null, its size must match baseMap
4. Sort layers by Priority then original index
5. Allocate output: new ScalarMap of same size, copy baseMap into it
6. Apply each layer sequentially: for each pixel (linear loop over float[]): get influenceValue (1.0 if no influence map); rawStrength = influenceValue * layer.Weight; strength = clamp(rawStrength, 0, 1); apply layer.Mode to output[y*W + x] using layer.Source[y*W + x] and strength
7. Apply settings (clamping)
8. Return output map

### Constraints:
- No temporary ScalarMap per layer - apply in-place to output buffer
- No LINQ in the per-pixel loop
- No reflection
- No hidden normalization
- Input maps (baseMap, Source, Influence) are never modified

### Class: ComposeSettings

```csharp
public class ComposeSettings
{
public bool ClampOutput get; set; = false;
public float MinValue get; set; = 0f;
public float MaxValue get; set; = 1f;
// Not in MVP: NormalizeOutput, OutputMin, OutputMax
}
```

### Validation
if ClampOutput is true and MinValue > MaxValue -> ArgumentException.

### Validation errors:
| Case | Error |
| --- | --- |
| baseMap is null | ArgumentNullException |
| layers is null | ArgumentNullException |
| settings is null | ArgumentNullException |
| Layer.Source is null | ArgumentException |
| Layer.Source size does not match baseMap | ArgumentException |
| Layer.Influence size does not match baseMap | ArgumentException |
| ClampOutput is true and MinValue > MaxValue | ArgumentException |

### Why no NormalizeOutput in MVP? Normalization requires two passes (find min/max, then remap). It is not free and is not always needed. Postpone until real demand appears. Caller can easily implement
value = (value - min) / (max - min).

## Stage 4 Completion Criteria:
- Composer.Compose works as specified
- Sorting by Priority + original index
- Supports all 4 blend modes, influence, weight
- Supports ClampOutput
- Does not modify inputs
- Tests for all validation errors, order, clamping

## Stage 5 - Performance

**Goal:** Ensure the library can handle 2048x2048 maps at reasonable speed without unnecessary allocations.

### Non-negotiable rules for hot path (per-pixel loop):
- Use float[] directly, not Get/Set calls inside the loop
- Do not allocate any new objects per pixel
- Do not use delegates or virtual calls per pixel
- Do not use LINQ anywhere inside composition
- Pre-filter and pre-sort layers once, before the pixel loop

### Benchmark requirements:
Measure composition time for:
- Sizes: 512x512, 1024x1024, 2048x2048
- Layer counts: 1, 4, 8, 16
- With and without influence maps
- Mixed blend modes

Record results in Benchmarks/README.md. The goal is not absolute speed records but to detect regressions and obviously poor scaling.

## Stage 5 Completion Criteria:
- Composition works without crashes on 2048x2048
- No per-layer temporary ScalarMap allocations
- No LINQ, no reflection, no per-pixel delegates
- Benchmark results documented

## Stage 6 - Tests

**Goal:** Cover correctness and error handling. Tests must be deterministic and runnable without manual setup.

### Required test categories:

ScalarMap: create, access, bounds in DEBUG, copy, clone, fill
Blend modes: strength 0, 0.5, 1, >1, negative (if allowed)
Layers: null Source, missing influence, influence size mismatch
Sorting: priority order, same priority keeps original order
Composer: base map unchanged, outputs separate map, clamping
Edge cases: empty layer list, zero-weight layers, disabled layers

### Prohibited in tests
creating meshes, terrain, voxels, or any visual output. Tests work only with float values and ScalarMap.

## Stage 6 Completion Criteria:
- All above categories covered
- Tests pass deterministically
- No visual/rendering dependencies

## Stage 7 - Source Adapters (Architecture Only)

**Goal:** Ensure the core does not depend on any specific data source, leaving space for adapters.

### Allowed in core
ScalarMap, Layer, Composer, BlendMode, ComposeSettings.
### Not allowed in core
Texture2D, FastNoiseLite, file paths, UnityEngine.Object.

### Example adapter pattern (not implemented in MVP):
```csharp
public static class Texture2DAdapter
{
public static ScalarMap ToScalarMap(Texture2D tex) ...
}
```

The adapter lives outside the core (separate assembly or folder).

## Stage 7 Completion Criteria:
- Core has zero references to UnityEngine, Texture2D, noise libraries
- It is possible to write an adapter without modifying core code

## Stage 8 - Integration Readiness (MVP Done)

**Goal:** The library can be used as a NuGet / Unity package dependency.

### Minimal external code to use Elevate:
```csharp
var baseMap = new ScalarMap(1024, 1024);
var layer = new Layer { Source = someOtherMap, Mode = BlendMode.Add, Weight = 0.3f };
var settings = new ComposeSettings { ClampOutput = true, MaxValue = 100f };
var result = Composer.Compose(baseMap, new[] { layer }, settings);
```

### Requirements:
- No Editor API required at runtime
- No static constructors that fail outside Unity
- All public API documented with XML comments

## Stage 8 Completion Criteria:
- All runtime code compiles without Editor symbols
- A separate empty Unity project can reference the package and run composition
- Public API is clean and minimal

## Out of MVP (Explicitly Postponed)

The following are not part of the first version:
- NormalizeOutput in settings
- Multiply, Subtract, Overlay, Difference, Replace, Custom blend modes
- Multi-threading / Jobs / Burst
- GPU compute
- Tiled or partial composition
- Erosion, simulation, or any high-level procedural generation
- Texture2D, FastNoiseLite, or file adapters
- Editor UI or inspectors

### Important
These features are deliberately excluded to keep MVP focused. The architecture allows them to be added later without breaking changes.
