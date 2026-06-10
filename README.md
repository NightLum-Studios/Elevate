# Elevate

Elevate is a standalone backend library for blending two-dimensional scalar fields (heightmaps, influence maps, density, etc.).
It is designed as a pure data‑processing core for procedural generation systems: terrain, voxel worlds, biomes, simulations, and runtime tools.
Elevate has no dependencies on Unity Editor, specific noise libraries, Texture2D, file I/O, or any visual output system.

## Documentation

The only technical specification for the first version (MVP) is:

**[Roadmap.md](Roadmap.md)**

It contains:
- Public API requirements
- Data model (`ScalarMap`, `Layer`)
- Blend modes (`Add`, `Blend`, `Max`, `Min`)
- Composition rules and validation
- Performance constraints
- Tests and benchmarks

## Repository Structure (MVP)

Only these folders are required for the first version:

    Runtime/
    ScalarMap.cs
    Layer.cs
    BlendMode.cs
    ComposeSettings.cs
    Composer.cs
    Tests/
    Benchmarks/

## Development Rules (short)

All requirements are defined in `Roadmap.md`.  
When in doubt, follow the specification in `Roadmap.md`.

Prohibited in MVP:
- LINQ, reflection, delegates in the per‑pixel loop
- Temporary `ScalarMap` allocations per layer
- Hidden normalization or clamping
- Modifying input maps

## License
NightLum Studios License (NSL) v1.2
