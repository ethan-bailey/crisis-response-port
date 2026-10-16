# Configured grip contact, reload occlusion and chamber display

The iPhone report showed hands below the M4 and the magazine crossing the wrist during reload. The earlier hand-restoration check verified rendering and mode activation but did not establish grip contact.

## Causes and changes

Original `WeaponBase.Initialize` assigns both hand anchors from `WeaponArmConfiguration` before arm targeting. The browser already did this, but the C# host retained serialized prefab anchors. The source M4 configuration sets right `(0.26, 0.005)` and left `(0.5, 0.085)`; prefab values were right `(0.23000002, -0.060000002)` and left `(0.48000002, 0)`. The host now applies the same recovered configuration before constructing the source IK solvers. No arbitrary grip correction was introduced.

The transport renderer also disabled limb depth writes and rendered the magazine as a late transparent overlay. Original operator skin and magazine materials specify `_ZWrite: 1`, `_SrcBlend: 1`, `_DstBlend: 0`, and `_Cutoff: 0.5`. `scripts/recover_source_cutout_materials.py` recovers their original material values and source hashes into `game/assets/source-cutout-materials.json`. The limb and magazine renderers now use these cutout/depth settings so nearer limb surfaces can occlude the magazine. This restores depth/alpha settings, not original custom shader shading; native shader parity remains false.

The chamber HUD now presents a horizontal cartridge in a labeled chamber slot above the magazine. It remains outside the moving magazine body during drag/reload, and its visibility comes only from the source `chamberLive` state. This is a HUD mark, not replacement game ammunition or an added round. The numeric count still means rounds ready to fire.

## Verification

Local native engine, original sequence, original hand activation, M4 firing, empty bolt lock and full reload contracts pass. Recovered depth metadata reproduces byte for byte from the original project.

Initial grip/chamber source `5c34a6eb404aaaba3887ef70fe2060cb177c7757` passed native, .NET 8 WASM, source/package and both-browser gameplay checks in Actions `38941300573`. Evidence artifact `12063218011`, ZIP SHA256 `721de90c38d1df4907a90d0fbb06b52356e3d094355a523f642beacb9762f523`. Chromium/WebKit aiming and reload screenshots were inspected; configured targets match rendered anchors and held magazine IK points match original hand grab points.

Final depth-corrected source `7a6b8804e2212b20422f23b2343a96601bded3f0` passed full native/.NET8 WASM/Chromium/WebKit verification in Actions `38941678900` during wrap-up; final screenshots still require review before publication. Do not substitute the earlier artifact for this version. New browser assertions also require 19 real source cutout materials (17 operator skins and two magazine faces), all depth-writing with the recovered alpha cutoff.

Physical iPhone Safari acceptance and native Unity shader/animation parity remain unverified. This focused correction does not complete the separate physical ArmController seven-step dependency closure.
