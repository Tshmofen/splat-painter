# Godot 4 - Splat Painter (.NET 8.0)

Simple tool to paint splat texture directly on the Mesh surface in Godot editor. Created for personal & educational purposes to replicate specific feature of amazing [Zylann/Heightmap](https://github.com/Zylann/godot_heightmap_plugin) plugin.

![Edit Menu](/Images/editor_example.png)

## Features
- Shader for pixel art grounds with support for up to 4 ground textures and splat map (`Albedo` + `Metallic` alpha, `Normal` + `Roughness` alpha for each texture).
- Ground depth blending depending on the `Metallic` channel of each texture.
- Simple tool for painting each single channel (`RGBA`) with `Size` and `Force` settings.
- Includes simple `MeshUvTool` that can be used to map `GlobalPoint` & `Normal` to `UV` position on the `ArrayMesh`.
- Quick input actions for editor: `R` -> Red Channel, `G` -> Green, `B` -> Blue, `A` -> Alpha, `C` -> Clean Channels.
- Support for `UndoRedo`.

## Context
For my purposes Zylann's plugin is too complex & rigid because of heightmap, it's not really suitable for small low-poly locations with complex (meaning that just lower the ground is not enough) vertical changes and some physics involved (was constantly getting glitches with relatively small bodies on heightmap collider).

So, it is just easier (& better) for me to just manually create low-poly ground meshes, but that plugin contains amazing splat depth blending system that was preventing me from migrating from it.
But, finally, here's my simple replacement for that specific system.

## Notes
This repository is meant to be a personal storage, so it's not published anywhere as actual Godot addon. I have just created it like that for possible personal re-uses later on.

Though, as you are already here, feel free to use it in any way you want - `MeshUvTool` tool might be pretty useful for others. Anyway, thanks for visiting :)

<b>2024 (c) tshmofen</b>
