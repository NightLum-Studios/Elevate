# Elevate Architecture

---

## Overview

    Elevate is a library for working with scalar fields, heightmaps, influence maps, and data composition systems.
    The library is being developed as a standalone backend module.
    The primary goal of Elevate is to provide a universal foundation for procedural generation systems.

## Goals

    Provide a flexible and extensible framework for field processing.
    Support composition of multiple layers into a single result.
    Ensure deterministic behavior.
    Be suitable for terrain, voxel, world generation, and other procedural systems.
    Remain independent from visual graph frameworks and editor tools.

## Planned Concepts

---

### ScalarMap

    A source-agnostic two-dimensional scalar field.

### Layer

    A data source that contributes to a final composed result.

### Influence Map

    A scalar field that defines where and how strongly a layer contributes during composition.

### Blend Mode

    A rule that defines how a layer is combined with existing data.

Example modes:

    Add
    Blend
    Max
    Min

### Composition

    The process of combining layers, influence maps, and blend rules into a final output field.

## Design Principles

    Data-oriented approach where appropriate.
    Deterministic results.
    Extensible architecture.
    Minimal external dependencies.
    Support for processing large datasets.

## Source Adapter Boundary

Elevate runtime code accepts only scalar data and composition settings. It does not reference UnityEngine, UnityEditor, textures, files, graph frameworks, or noise generators.

Adapters are separate assemblies or folders that translate external data into `ScalarMap`. They may depend on their source system while referencing `Elevate.Runtime`; the runtime assembly never references adapters.

Dependency direction:

    Terrain Graph adapter -> Elevate.Runtime
    Texture adapter       -> Elevate.Runtime
    Elevate.Runtime       -> System only

An adapter can populate a map through `Set`, `AsSpan`, or `GetRawData`, then pass it to `Composer.Compose`. Adding a source requires no changes to the Elevate core.
