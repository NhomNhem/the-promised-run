# UI VFX Shader Library

This directory contains Shader Graph shaders for UI visual effects in Clumsy Dungeon.

## Overview

**Unity 6.3+** - UI Toolkit now supports Shader Graph directly without RenderTexture!

See: https://docs.unity3d.com/6000.3/Documentation/Manual/ui-systems/ui-shader-graph.html

## Quick Start

### 1. Create UI Shader Graph

```
Right-click → Create → Shader Graph → URP → UI Shader Graph
```

### 2. Add Nodes

UI Shader Graph requires **Render Type Branch** node:
- Connect your effects to the Solid input
- Output to Fragment → Base Color

### 3. Apply to UI

| Method | Code/Setup |
|--------|------------|
| USS | `-unity-material: resource("Materials/Mat_UIVFX_Pulse.mat");` |
| C# | `element.style.unityMaterial = material;` |
| UI Builder | Inspector → Material dropdown |

## Available Shaders

| Shader Name | Effect | Best For |
|-------------|--------|----------|
| UIVFX_Pulse | Rhythmic brightness/scale | Logo animation, warning states |
| UIVFX_Glow | Emissive glow | Button hover, active elements |
| UIVFX_Shimmer | Gradient sweep | Achievement notifications |
| UIVFX_Ripple | Circular wave | Button click, damage feedback |
| UIVFX_Feedback | Color flash | Damage (red), Heal (green) |

## Properties

All shaders can expose:

| Property | Type | Range | Description |
|----------|------|-------|-------------|
| `_Intensity` | Float | 0-1 | Effect strength |
| `_Speed` | Float | 0.1-3 | Animation speed |
| `_Color` | Color | - | Effect tint |

## Creating Materials

1. Right-click shader → **Create → Material**
2. Name: `Mat_UIVFX_Pulse`, etc.
3. Apply via USS or C#

## File Structure

```
Assets/_Art/Shaders/UI/
├── UIVFX_Pulse.shadergraph
├── UIVFX_Glow.shadergraph
├── UIVFX_Shimmer.shadergraph
├── UIVFX_Ripple.shadergraph
├── UIVFX_Feedback.shadergraph
├── ShaderGraph_Tutorial.md
└── README.md

Assets/
└── Materials/  (create these)
    ├── Mat_UIVFX_Pulse.mat
    └── ...
```

## Dependencies

- Unity 6.3+ (for direct Shader Graph support)
- URP (Universal Render Pipeline)
- UI Toolkit

## Notes

- No RenderTexture needed in Unity 6.3+
- Use **UI Shader Graph** template (not VFX or Unlit)
- Include **Render Type Branch** node for UI compatibility
