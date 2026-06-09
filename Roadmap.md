# Elevate Technical Specification

## Document Purpose

This document describes the requirements for the first version of the Elevate library. After reading the specification, the purpose of the library, its area of responsibility, architectural constraints, expected API structure, implementation stages, readiness criteria, and the minimum MVP scope should be clear.
Elevate must be designed as an independent library for working with heightmaps, scalar fields, influence layers, and data composition. The library must not be tied to one specific tool, editor, visual system, or output method.

---

## Library Summary

Elevate is a backend library for blending heightmaps and other two-dimensional value fields.
The main task of the library is to accept one base heightmap, apply a set of layers with different blend modes to it, take into account the weight and influence map of each layer, and then return the final heightmap.
The library must be suitable for use in different systems:
    terrain generation based on heightmap
    voxel terrain
    procedural world generation
    imported heightmap data
    third-party noise generators
    simulations
    editor tools
    runtime tools for preparing world data
Elevate must not independently create terrain, mesh, voxel chunks, materials, scene objects, or user interfaces. It is responsible only for storing, validating, blending, and returning data. The only exception is tests, where creating terrain, mesh, and voxel chunks is allowed.

---

## Architectural Principle

Elevate must be a separate data-processing core.
The main workflow of the library:
    input data maps
    height layers
    composition parameters
    composition process
    final heightmap
An external system must pass data into Elevate, receive the result, and decide on its own how to use the final map. This can be mesh output, Unity Terrain output, voxel output, saving to a file, or further processing.
Elevate must not know who calls it or what final output the result is used for.

---

## Responsibility Boundaries

Elevate is responsible for:
    storing heightmaps
    reading and writing map values
    validating input data sizes
    describing a height layer
    applying an influence map
    applying layer weight
    applying a blend mode
    sorting layers by priority
    composing multiple layers into the final map
    limiting final values through settings
    deterministic results
    basic tests
    basic benchmark

Important rule: external data sources can be adapters around Elevate, but they must not become dependencies of its core.

---

## Goals of the First Version

The first version must provide a stable backend core that can be safely connected to other systems.
The first version must implement:
    HeightMap
    HeightLayer
    BlendMode
    HeightComposeSettings
    HeightComposer
    basic blend modes
    Influence Map support
    Weight support
    Enabled support
    Priority support
    SourceOrder support
    ClampOutput
    tests for the main logic
    benchmark on large maps
The first version must not try to solve all future tasks. Its task is to provide a simple, predictable, and extensible foundation.

---

# Stage 1 — Core Data Model

## Stage Goal

Create a basic data model for storing a two-dimensional value map.
At this stage, it is important to make not a beautiful wrapper, but a reliable foundation. All future composition systems will depend on how carefully the base data type is implemented.

---

## HeightMap

HeightMap is the main data type of the first version.
It must represent a fixed-size two-dimensional map of float values. In the first version, HeightMap is used as a heightmap, but the architecture must not prevent future expansion into a more universal scalar field.
HeightMap requirements:
    data is stored in a one-dimensional float[] array
    two-dimensional access is calculated through an index
    the float[,] array is not used
    the map has Width
    the map has Height
    the map has Length or Count for the total number of elements
    the map supports reading a value by x, y coordinates
    the map supports writing a value by x, y coordinates
    the map provides access to the internal array only in a controlled way
    the map must not create unnecessary allocations during normal reading and writing
    the map must be suitable for sizes 512x512, 1024x1024, 2048x2048 and higher
Indexing formula:
    index = y * width + x
Validation:
    Width must be greater than 0
    Height must be greater than 0
    array size must match Width * Height
    accessing outside the map bounds must be handled predictably
    if sizes do not match, a clear error must be returned
API recommendation:
    constructor HeightMap(width, height)
    constructor HeightMap(width, height, data)
    Get(x, y)
    Set(x, y, value)
    GetRawData()
    Clone()
    CopyTo(target)
GetRawData nuance:
    If the method returns the internal array directly, this must be explicitly documented.
    A safer option is to provide a method for reading Span or ReadOnlySpan, if the selected C# version allows it.
    If the project remains in Unity and the C# version is limited, float[] can be used, but the behavior must be explicitly described.

---

## Field

Field is a future, more universal abstraction for a two-dimensional scalar field.
The first version does not have to implement a full Field system. The main thing is not to design HeightMap in a way that makes it impossible to expand the library later.
Recommended approach:
    implement HeightMap as the main working type in the first version
    do not introduce unnecessary abstractions without need
    do not overcomplicate the code prematurely
    leave an architectural possibility to add IScalarField or Field later
The Field decision must be pragmatic. If the abstraction improves architecture without complicating implementation, it can be added. If it starts slowing down development of the first version, it must be postponed.

---

## Stage Result

The stage is considered complete if:
    a HeightMap of the required size can be created
    values can be read
    values can be written
    data is stored in float[]
    size validation exists
    error behavior is clear
    the code does not depend on Unity Editor
    the code does not depend on external data generators

---

# Stage 2 — Height Layer

## Stage Goal

Add a description of a height layer that can be applied to the final composition.
HeightLayer must not be a UI element or an editor object. It is a pure data model describing one data layer and the rules for applying it.

---

## HeightLayer

HeightLayer must contain:
    Source
    Influence
    Weight
    Enabled
    Priority
    Mode
    SourceOrder
Field descriptions:
    Source — required HeightMap containing layer values
    Influence — optional HeightMap defining the strength of the layer influence at each point
    Weight — general multiplier of the layer strength
    Enabled — layer enable flag
    Priority — layer application order
    Mode — layer blend mode
    SourceOrder — additional order for stable sorting of layers with the same Priority

---

## Layer Logic

If Enabled is false, the layer does not participate in composition.
If Influence is absent, the layer is considered to affect the entire map.
If Influence is present, its values are used as the local strength of the layer influence.
The final layer strength is calculated as follows:
    strength = influenceValue * Weight
If Influence is absent:
    strength = Weight
Expected Influence range:
    0 means the layer does not affect the point
    1 means the layer affects the point with full Weight strength
Influence values outside 0..1 must be handled predictably. In the first version, it is recommended not to perform hidden clamp inside each layer unless this is explicitly specified. It is better to document the expected range and leave responsibility for data preparation to the caller.

---

## Purpose of Influence Map

Influence Map is needed for spatial control of the layer.
Usage examples:
    mountain layer is applied only in a selected area
    crater layer is applied only inside the crater mask
    detail layer is applied only on required areas
    smoothing layer is applied only in lowlands
    riverbed layer is applied only along a precomputed mask
Influence Map must be a regular value map of the same size as Source and baseHeight.

---

## HeightLayer Validation

During composition, it is necessary to validate:
    Source must not be null
    Source size must match baseHeight size
    if Influence is set, its size must match baseHeight size
    Weight must be handled predictably
    Mode must be a valid BlendMode value
Behavior for Weight less than 0 must be explicitly defined. For the first version, it is recommended to allow negative Weight only if it is intentionally supported by the formulas. A safer option is to treat negative Weight as an error and throw a clear exception.

---

## Stage Result

The stage is considered complete if:
    the HeightLayer type exists
    a layer can be enabled and disabled
    a layer can have Source
    a layer can have Influence
    a layer can have Weight
    a layer can have Priority
    a layer can have SourceOrder
    a layer can have BlendMode
    Composer can use the layer without dependency on UI or an external system

---

# Stage 3 — Blend Modes

## Stage Goal

Implement basic heightmap blending modes.
BlendMode must define how a layer value is applied to the current value of the final map at each point.
The first version only needs modes that have clear meaning for terrain workflows and do not duplicate each other.

---

## BlendMode of the First Version

The first version must implement:
    Add
    Blend
    Max
    Min
Do not include Replace in the first version.
Reason: with the formula result = lerp(current, layer, strength), Replace fully duplicates Blend and creates a duplicated mode without separate value. Replace can be returned later, when its difference is clearly defined: hard replacement, threshold replacement, region replacement, or priority-based overwrite.

---

## Add

Purpose:
    Add is used to add shape on top of the current map.
Formula:
    result = current + layer * strength
Typical scenarios:
    adding hills
    adding mountains
    adding small details
    adding procedural noise
Nuance:
    Add can easily move the result outside the expected height range. If the result must be limited, this must be done through HeightComposeSettings, not hidden inside Add.

---

## Blend

Purpose:
    Blend is used for a soft transition from the current map to the layer map.
Formula:
    result = lerp(current, layer, strength)
Typical scenarios:
    soft shape replacement
    blending two heightmaps
    smooth transition between the base map and detailing
Nuance:
    Blend does not add the layer on top of the current value. It pulls the current value toward the layer value.

---

## Max

Purpose:
    Max is used when the layer must only raise the shape and must not lower the current height.
Formula:
    target = max(current, layer)
    result = lerp(current, target, strength)
Typical scenarios:
    adding mountain shapes without destroying the existing terrain
    strengthening elevations
    combining maps where maximum values need to be preserved

---

## Min

Purpose:
    Min is used when the layer must only lower the shape and must not raise the current height.
Formula:
    target = min(current, layer)
    result = lerp(current, target, strength)
Typical scenarios:
    valleys
    riverbeds
    cuts
    pits
    lowering terrain in specified areas

---

## BlendMode Extensions

Do not implement in the first version, but do not break the architectural possibility to add later:
    Multiply
    Subtract
    Overlay
    Difference
    Replace
    Custom
Custom blend mode must not be added too early. A custom blending formula can complicate the API, testing, and determinism. It must not be added in the first version.

---

## Stage Result

The stage is considered complete if:
    an enum or similar BlendMode type is implemented
    Add is implemented
    Blend is implemented
    Max is implemented
    Min is implemented
    each mode is covered by tests
    formulas produce predictable results
    modes do not perform hidden normalization

---

# Stage 4 — Height Composer

## Stage Goal

Create the main backend class that performs heightmap composition.
HeightComposer must accept a base map, a set of layers, and composition settings. As output, it must return the final heightmap.

---

## HeightComposer

Main method:
    Compose(baseHeight, layers, settings)
Result:
    outputHeight
Requirements:
    the method must not modify baseHeight
    the method must not modify Source inside HeightLayer
    the method must not modify Influence inside HeightLayer
    the result must be written into a separate HeightMap
    layer application order must be stable
    the result must be deterministic

---

## Composition Algorithm

Composer must perform the following actions:
    1. Validate baseHeight
    2. Validate settings
    3. Validate the layers list
    4. Skip disabled layers
    5. Validate Source and Influence sizes for enabled layers
    6. Sort layers by Priority
    7. For layers with the same Priority, use SourceOrder
    8. Copy baseHeight into the output buffer
    9. Apply each layer sequentially
    10. Apply output settings
    11. Return the final HeightMap
Sorting must be stable and predictable. If Priority and SourceOrder are the same, the order must either preserve the original order or be explicitly defined in the documentation.

---

## HeightComposeSettings

HeightComposeSettings must describe the final result settings.
Fields of the first version:
    ClampOutput
    MinValue
    MaxValue
Optional field that can be postponed:
    NormalizeOutput
ClampOutput:
    if ClampOutput is true, final values must be clamped to the MinValue..MaxValue range
    if ClampOutput is false, final values are not clamped
MinValue and MaxValue:
    MinValue must be less than or equal to MaxValue
    if ClampOutput is enabled and the range is invalid, there must be a clear error
NormalizeOutput:
    do not implement in the first version if there is no real need
    do not enable hidden normalization
    if NormalizeOutput appears later, it must be an explicitly enabled parameter

---

## Important Constraints

Composer must not:
    create a temporary HeightMap for each layer
    use LINQ in the hot path
    perform hidden normalization
    depend on Unity Editor
    depend on a specific data source
    modify input maps
    secretly change layer order

---

## Stage Result

The stage is considered complete if:
    HeightComposer exists
    Composer accepts baseHeight
    Composer accepts a list of HeightLayer
    Composer accepts HeightComposeSettings
    Composer returns a new HeightMap
    sorting by Priority and SourceOrder is supported
    Add, Blend, Max, Min are supported
    Influence is supported
    Weight is supported
    ClampOutput is supported
    input data is not modified

---

# Stage 5 — Performance Pass

## Stage Goal

Prepare the implementation for working with large heightmaps.
Elevate must be designed not only for small test maps. The minimum practical reference point is stable work with 1024x1024 and 2048x2048 maps.

---

## Performance Requirements

The hot path must not use:
    LINQ
    reflection
    temporary arrays inside the pixel loop
    creating a new HeightMap for each layer
    unnecessary boxing operations
    extra delegates inside the inner loop
    hidden allocations for each map element
The hot path must:
    use float[]
    iterate linearly over the array
    validate map sizes in advance
    sort layers in advance
    filter disabled layers in advance
    minimize bounds checks where safe
    not perform unnecessary checks inside the deepest loop

---

## Benchmark

A benchmark must be prepared for sizes:
    512x512
    1024x1024
    2048x2048
The benchmark must measure:
    composition time
    number of layers
    effect of enabled Influence Map
    effect of absent Influence Map
    different BlendMode
    ClampOutput behavior
Recommended benchmark scenarios:
    baseHeight plus 1 layer
    baseHeight plus 4 layers
    baseHeight plus 8 layers
    baseHeight plus 16 layers
    layers without Influence
    layers with Influence
    mixed BlendMode set
Benchmark results must be saved in README, a separate benchmark file, or a comment in the test project. The goal of the benchmark is not absolute numbers, but understanding that the implementation has no obvious allocation and scaling issues.

---

## Stage Result

The stage is considered complete if:
    composition works on 512x512
    composition works on 1024x1024
    composition works on 2048x2048
    there are no obvious unnecessary allocations in the hot path
    benchmark shows predictable scaling
    performance does not degrade because of architectural mistakes

---

# Stage 6 — Tests

## Stage Goal

Verify the correctness of the main logic before integrating the library into other systems.
Tests must cover not only positive scenarios, but also input data errors. This is especially important because Elevate will be used as a backend, and errors must be detected at the core level rather than appear later in output systems.

---

## Required Tests

Need to cover:
    HeightMap creation
    HeightMap reading
    HeightMap writing
    indexing validation
    Add mode
    Blend mode
    Max mode
    Min mode
    Weight
    Influence Map
    layer without Influence
    disabled layer
    Priority order
    SourceOrder
    size mismatch between baseHeight and Source
    size mismatch between baseHeight and Influence
    ClampOutput
    absence of hidden normalization
    immutability of baseHeight after Compose
    immutability of Source after Compose

---

## Test Format

If the library remains inside a Unity package, Unity Test Framework can be used.
The test runner decision must match the project structure. The main requirement is that tests must be accessible and run without complex manual environment preparation.

---

## Stage Result

The stage is considered complete if:
    main formulas are covered by tests
    size errors are covered by tests
    layer order is covered by tests
    influence maps are covered by tests
    output settings are covered by tests
    tests can be run repeatedly and produce the same result

---

# Stage 7 — Source Adapters

## Stage Goal

Prepare the library for the fact that heightmaps can come from different sources.
At this stage, it is not required to implement all real adapters. It is necessary to define the architectural approach so that future integrations do not break the core.

---

## Possible Data Sources

In the future, Elevate must be able to receive HeightMap from different sources:
    heightmap image
    Texture2D from Unity
    third-party noise library
    FastNoiseLite
    Gaea export
    World Machine export
    procedural generator
    user float[] array
    data prepared by another system

---

## Correct Scheme

The Elevate core must not depend on a specific data source.
Correct scheme:
    External source -> adapter -> HeightMap -> HeightComposer
Incorrect scheme:
    HeightComposer directly depends on FastNoiseLite
    HeightComposer directly depends on Texture2D
    HeightComposer directly depends on Unity API
    core module directly depends on the format of a specific generator

---

## API Recommendation

An interface for future data sources can be provided:
    IHeightMapSource
Possible responsibility of such an interface:
    return Width
    return Height
    create HeightMap
    fill an existing HeightMap
At the first stage, it is enough to describe the interface or leave space for it. Real integration with Texture2D, FastNoiseLite, or file import is better moved into separate modules later.

---

## Stage Result

The stage is considered complete if:
    core does not depend on specific data sources
    there is a clear place for future adapters
    architecture does not prevent connecting a Texture2D adapter later
    architecture does not prevent connecting a FastNoiseLite adapter later
    architecture does not prevent importing float[] directly

---

# Stage 8 — Integration Readiness

## Stage Goal

Prepare Elevate for connection to other world data generation tools.
The library must be easy to integrate. An external system must be able to prepare HeightMap, create a set of HeightLayer, call HeightComposer, and receive the final HeightMap without knowing the internal implementation.

---

## Runtime API Requirements

Runtime API must be:
    clean
    predictable
    independent of Unity Editor
    independent of a specific tool
    independent of a specific output backend
    understandable without reading all source code
    suitable for reuse
    suitable for testing
API must not require the external system to create unnecessary intermediate objects without need.

---

## Usage Scenario

Typical scenario:
    external system creates baseHeight
    external system creates one or more HeightLayer
    each HeightLayer receives Source
    if necessary, HeightLayer receives Influence
    external system sets Weight, Priority, and BlendMode
    external system creates HeightComposeSettings
    external system calls HeightComposer.Compose
    Elevate returns the final HeightMap
    external system uses the final HeightMap for its output

---

## Output-Agnostic Approach

The same Elevate result must be suitable for different outputs:
    Mesh Output
    Unity Terrain Output
    Voxel Output
    export to file
    further procedural processing
Elevate must not know which output will be used after composition.

---

## Stage Result

The stage is considered complete if:
    Elevate can be connected as a backend library
    external code can create HeightMap
    external code can create HeightLayer
    external code can call Compose
    external code can receive Final HeightMap
    Runtime API does not depend on Editor API
    Runtime API does not depend on a specific output

---

# Stage 9 — Optional Extensions

## Stage Purpose

This section describes tasks that are not included in the first version, but that must be considered during design.

---

## Possible Extensions

    Normalize Output
    Multiply blend mode
    Subtract blend mode
    Replace blend mode
    Custom blend mode
    tiled heightmap processing
    partial region composition
    multi-threading
    Unity Jobs module
    Burst module
    FastNoiseLite adapter
    Texture2D adapter
    image import
    image export
    erosion module
    GPU compute module

---

## What Is Important Not to Do Prematurely

Do not add complexity to the first version for hypothetical future scenarios.
Do not immediately build a universal framework for everything. The first version must be compact, working, and testable.
Correct approach:
    first stable core
    then tests
    then benchmark
    then integration
    then extensions based on real needs

---

# Project Structure

## Recommended Structure

Recommended project structure:
    Runtime/
        Core/
        Composition/
        Fields/
        Interfaces/
        Internal/
    Editor/
    Tests/
    Samples~/
    Documentation~/

---

## Runtime/Core

Runtime/Core contains base data types and common structures.
Examples:
    HeightMap
    base errors
    base settings
    common utility types, if needed

---

## Runtime/Composition

Runtime/Composition contains composition logic.
Examples:
    HeightLayer
    BlendMode
    HeightComposeSettings
    HeightComposer
    internal methods for applying blend modes

---

## Runtime/Fields

Runtime/Fields contains types related to scalar fields and the future expansion of HeightMap.
In the first version, this section can be minimal.

---

## Runtime/Interfaces

Runtime/Interfaces contains contracts for future adapters and extensions.
Examples:
    IHeightMapSource
    IHeightMapWriter
    IHeightMapAdapter
Interfaces should be added only if they actually help the API. Do not create unnecessary interfaces without practical use.

---

## Runtime/Internal

Runtime/Internal contains internal optimized helpers.
This section must not become public API. Everything inside Internal can be changed without maintaining backward compatibility.

---

## Editor

Editor may exist in the project, but the first version of Elevate must not require Editor code for the runtime core to work.
If the Editor folder appears, it must not be a dependency of Runtime.

---

## Tests

Tests contains core tests.
Tests must be separated from runtime code and must not interfere with using the library in other projects.

---

## Samples~

Samples~ may contain simple examples of using the library.
Examples are not a mandatory part of the MVP, but they are useful for validating the API.

---

## Documentation~

Documentation~ contains documentation, call examples, and descriptions of decisions.
Documentation must explain controversial points:
    why Replace is not included in the first version
    how Influence works
    how Weight works
    how layers are sorted
    why there is no hidden normalization
    what limitations the first version has

---

# Development Rules

## Prohibited

In the first version, it is prohibited to:
    add a dependency on a specific external tool
    add Unity Editor API to Runtime
    create UI
    create a visual editor
    create terrain rendering
    create mesh generation
    create voxel generation
    connect FastNoiseLite in core
    connect Texture2D in core, if this creates a dependency on UnityEngine
    perform hidden normalization
    use LINQ in the hot path
    create temporary arrays inside the pixel loop
    modify input maps inside Compose
    add complex extensions without need

---

## Required

In the first version, it is required to:
    write portable code
    keep the API simple
    preserve determinism
    consider large data
    write tests
    provide clear errors
    document controversial decisions
    not mix core and integration code
    not add dependencies without reason
    design so that the library can be used in different projects
