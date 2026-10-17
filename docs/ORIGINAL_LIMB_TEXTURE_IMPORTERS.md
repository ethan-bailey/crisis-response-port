# Original Limb Skin TextureImporter Provenance

Recovered from the checksum-pinned Unity export archive SHA-256
`cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.

`source-reference/Original/LimbSkin/original-texture-importers.json` includes the original
Unity importer configuration, complete asset and `.meta` SHA256 identities and
source paths for all **15 original Operator limb skin texture GUIDs**, including
alternate head textures not currently selected by the 11 existing browser PNGs.

All 15 original importers have serialized `wrapU: 1`, `wrapV: 1`, and
`wrapW: 1`. The source metadata records the numeric Unity values without
inventing renderer behavior.

**Important distinction:** `LimbRenderer.PixelTrace` does not sample these
original skin textures directly. It samples `LimbTextureData.staticDamage`,
created by `new Texture2D(Width, Height, TextureFormat.RGBA32, false)` and
initialized from each skin's alpha. The original imported skin wrap settings
do **not** independently prove the runtime-created damage texture wrap mode.
Do not apply the imported skin's `wrapU/V` to the native damage-mask trace
without verifying the Unity 2017-era Texture2D constructor defaults.

This is evidence preservation, not the full original projectile hit/wound path.
The live C# source skin trace and source-backed physiology require their own
WASM browser checks; the complete recovered original `CharacterBody.cs` is
still a separate draft compiler closure.
