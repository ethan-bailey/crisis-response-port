# Recovered Unity assets — source-preserving compatibility audit (2026-10-15)

## Scope and provenance

Read-only analysis of private AssetRipper recovery artifact ID `11903811738`, preserved independently as private release `recovery-preservation-2026-10-15`. Do not make the repository public while proprietary recovered assets remain attached. Neither recovered binaries nor proprietary asset payloads are checked into Git history.

## Audio import failures: 250 malformed WAV lengths, original PCM intact

Exactly **296 `Assets/AudioClip/*.wav` files** were inspected directly from the recovery archive.

| Condition | Count |
| --- | ---: |
| Correct RIFF/data lengths | 46 |
| RIFF header length = 0 and `data` chunk length = 0 | **250** |
| Malformed or nonstandard PCM headers beyond two zero lengths | **0** |

All 250 affected files contain 44-byte standard RIFF/WAVE headers, a 16-byte PCM `fmt ` chunk, mono (200) or stereo (50) 16-bit PCM, and an intact sample-data segment. The RIFF size should be `file_length - 8`, and the `data` chunk size should be `file_length - 44`. Those are exactly the two missing 4-byte fields.

**Verification:** All 250 files were patched **in memory only** and successfully opened with Python's `wave` decoder. Frame counts, PCM payload bytes, channel count and sample rate were checked. The original archive was not modified. This exactly matches the 250 observed Unity `AudioClip` import errors (`FSBTool`: invalid source file format).

Implementation ready: `scripts/repair_recovered_wav_headers.py` with an exact-count guard, checksum sampling and decoder validation. The main headless Unity workflow has an optional `UNITY_SPIKE_FIX_WAV_HEADERS=1` setting, **off by default** for the WebGL-template-only comparison. When used, changes apply only in the ephemeral CI extraction, never in repository history.

## Marksman and Interior prefab import warnings

The two Unity 2022 prefab-import warnings are:
- `Assets/GameObject/Marksman mode.prefab`, GameObject **Marksman camera**, component **index 3**
- `Assets/GameObject/Interior mode.prefab`, GameObject **Interior camera**, component **index 2**

Direct serialized-YAML inspection identifies both entries as **`!u!92 GUILayer`** (removed Unity legacy built-in GUI component), **not missing custom MonoBehaviour scripts**. All component fileIDs referenced by the two prefabs are present in their YAML, and the prefabs' MonoBehaviour script GUIDs are present among recovered `.meta` files.

Unity 2022 already discards those two unsupported components during import. No game-system rewrite or arbitrary prefab editing has been performed. Scene visual/UI parity still requires runtime verification.

## Build implications

- Clean C# compilation remains possible with previously documented seven legacy-API adaptations and two compile-only exclusions of original UNet-specific Behavior Designer tasks.
- The single-scene WebGL postprocessor `IndexOutOfRangeException` at `BuildPostprocessor.cs:176` matches a known Unity 2022.3 issue with an **empty `webGLTemplate`**. The recovery archive confirms `webGLTemplate:` is blank. Its `QualitySettings.asset` lists six quality profiles and WebGL index 3 (valid).
- `scripts/unity_webgl_template_fix.py` makes the single guarded ephemeral settings update `webGLTemplate: APPLICATION:Default`. Quality indices and original project assets remain unchanged.
- WebGL template-only retry: [GitHub Actions run 38784669819](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38784669819). Confirm result from its eventual `status.env` and `webgl.log`; don't infer a successful player from GitHub workflow status.
- Once WebGL compilation succeeds, run an independent test with the guarded WAV header repair, then test a private/local served build using the generated Unity loader. A technical WebGL build does not establish behavioral parity or complete audio/rendering correctness.

No original game asset bytes are included in this Markdown audit.
