# develop/wasm local-to-remote commit replay — 2026-10-27

The primary checkout could not authenticate a shell `git push` (`fatal: could not read Username for 'https://github.com': No such device or address`). To preserve the 15 local commits, their trees and original messages were replayed as an ordered fast-forward chain through the connected GitHub Git Data API, then `develop/wasm` was moved with expected-HEAD compare-and-swap from `7b2f83d17443e87cf37fd53eb7ce0b862098c5b9`. Each remote commit tree SHA was verified equal to its corresponding local commit tree SHA before the ref move. The GitHub API generated new commit SHAs and actor timestamps, so the original local commit object IDs were **not** preserved; this crosswalk prevents duplicate/repeated pushes and keeps every source tree represented.

| # | Local commit | Remote replay commit | Tree (identical) | Original message |
|---:|---|---|---|---|
| 1 | `a0f4c8de01ade477d0ecb3c512175937056a9be4` | `c2fcc8ba466ed19e4f0f422ac9db50f737a0e109` | `edd0b86b9814cec09a998e4c8ae27beb9de3f8dd` | test: close particle collision and impact audio sources |
| 2 | `aee7f92f719ccce0471f2ded5185e3e2040ebda8` | `19fc969df0387507fa5a3eed5afb59fd6d963f7e` | `f5073f4343dfe1bcd05c11b02facd475f40a3167` | fix: root original TimeManager in Mission profile |
| 3 | `6ee94f689b9e97160bb2010e708f86efd3f3cd8b` | `d082afb2ed4ed7bcbdf0235024e1528daf42aed4` | `064fb8fbf8526d51ee2e50d1adc575d931a9e6e2` | feat: preserve Mission reverb zone definitions |
| 4 | `d14cc0a2325e51ea7d1267590200ef5b3df5ad9a` | `ecd1e54d46e3616fb11fac9a890ad976020d52fa` | `495969ef98243155ecbfdc50df69c1a9fd6c72e9` | docs: record recovered checkout gate batches |
| 5 | `b6169ec44dd00de5f0f10969bf5b69c85d81c525` | `0ede0a76ec65599593d040b93d7099677264e9a2` | `2462dcb53b7ea699e224c9d26491fb6916f93955` | fix: align source runtime with browser frame clock |
| 6 | `1b3989fed63bdd6011916311ed63c61a8746f290` | `f59e5a37280232522e693f03c863babb53ce0b75` | `632675187ec7d27aacf7234e5f3b9bd2658eb6db` | feat: preserve source audio motion provenance |
| 7 | `5b722e63c5026e493b802b47ed8b1a8cdb081f88` | `907036f6fcff226168dd37d613b58ca14aeb0da4` | `a0d5365cf9653cba5104c3fb4135cbbd752e1f43` | test: add version-locked audio spatial default probe |
| 8 | `9e620494f81b611a6e75b6b5d6f5d2ee232c0a80` | `6264ba0c3d1ecbfcdf0b3bdc6b59c601cb465d2b` | `bb959096d375eadca86029e35980e8e8b2414f1c` | test: add version-locked particle collision probe |
| 9 | `9f4ee9407e4586eb837fe54b08a73cca93565c40` | `e727b9c7a550fa6ed6457567a2346e37a5142075` | `1d6d18d701a89689f13d2d8a153764f9d9f6c8f5` | test: isolate packaged reload acceptance |
| 10 | `47d8adfe08ad660b1d257661d20f98b85dbba9ec` | `374cb1299058d8a720d33438528983d4a67b509f` | `3aeec5b55d8623900b0990ce058e8356703d1073` | docs: checkpoint nine local gate batches |
| 11 | `cc3aac984e10720dfac8d10912bfe04217274f77` | `d760d9696d968695672f1a3bfb5227b91f539916` | `6f03eece7e8d6b1e58620b77726cc2fbc751dd8d` | test: lock recovered camera source identity |
| 12 | `86978ee65a934d1ad319051f72e582641a012db6` | `c6390c3815b09fa6cc2fdf88eb86ce93935e30bc` | `4881b531f2e9490c995fc97d97c3ee56f33c4a9a` | docs: advance Gate 7 camera checkpoint |
| 13 | `85a70f47f053663a859e41db2170d4e2dd9fad4f` | `b526238efa3d2d748b4ad6b3961ebc765e0f2459` | `a66c01abc401e3ba37b36ecf4e6ef081721f2ee7` | chore: require pinned SDK preflight first |
| 14 | `e4581a08834cc48c0374ce1cf89de5fad98e68b0` | `2b8396cff0f457cb456e0353afa3ba156a7b6731` | `017a59ad7c564cfc0b4509e8ec277b72515350f0` | test: extend projectile compile boundary |
| 15 | `5c533f249c17ae21df89ac911003071d24680f25` | `dca6c97c8291813f9881a72c7565916c3d7b3cde` | `750afe43d951c52205a0ed18f3b5278557a6cc25` | docs: record pinned SDK and WASM validation |

- Remote branch after replay: `develop/wasm` at `dca6c97c8291813f9881a72c7565916c3d7b3cde`, tree `750afe43d951c52205a0ed18f3b5278557a6cc25`.
- Local checkout still points to original local history at `5c533f249c17ae21df89ac911003071d24680f25`, the exact same tree. `origin/develop/wasm` in this checkout is stale; do not use its reported ahead count as pushable work or replay these changes again.
- Worktree untracked `scene-data/.crisis-response-scenes.json.gz.jYCb21` was excluded and remains protected.

## Profile and reverb follow-up replays — 2026-10-27

These additional local commits were pushed as ordered, tree-identical remote commits using the same GitHub Git Data API and expected-HEAD CAS. The primary local checkout is now aligned to the remote branch; the original local-only commit chain is retained under `codex/local-replay-history-backup-20261027` so no local objects were abandoned.

| Local commit | Remote commit | Identical tree | Message |
|---|---|---|---|
| `f31fbac415a5156c58b21be8cf5213ce0c349bd5` | `1e1da39892beb83fe93305db84472956ffbe5c19` | `1579e89ec65bcfd1f6fc1b415f15b0af7e40bfa0` | docs: inventory recovered mission profile and replayed commits |
| `2b52a8289c887004267f4773003b8fa381dc212c` | `3a33a6df7d74681cf4074976ec9ca9d3845a56ea` | `49e905df226b5a794b2c15970bf434514c1e6e02` | Fix authored Mission reverb zone field import |
| `7ef07ed0606f58f63fafd7d6a1bff5818d2f24a5` | `458320dc51d6469339cacbd0667ba8c59d8e6475` | `9bdf86189ff8b00487ead4e856298292fbb9fab7` | docs: record verified profile reverb repair checkpoint |
| `9b84bb1200f92441f27bed0ee30fedad3b2eaf8d` | `a671cebf9dfb28232e0544b0883bc7ada4b6db7f` | `a481e257e1b62f37327723744aab2fc6a55bd035` | docs: inventory unresolved full-source 308 mesh edges |
| `7f0cea926085b5b45ffe0d9331af9dc6ca1bc1f8` | `6425b858af8e542de2be85cfad66c47d59e17569` | `21fd297505c250e3a9af273a94c183140d4fe7f3` | docs: map follow-up source commits and checkout sync |

Remote `develop/wasm` at the time of this crosswalk update was `a671cebf9dfb28232e0544b0883bc7ada4b6db7f`, tree `a481e257e1b62f37327723744aab2fc6a55bd035`; the local tracking checkout was fetched and confirmed at the same commit/tree. The untracked scene gzip remains excluded.


## Packaged-browser and inventory handoff replay — 2026-10-27

The local documentation commit `99cd21af171e4ff32366a5ded82a7c3486bd60a4` was replayed to the live branch as remote commit `a3656cbe48fbc1e8df28c274bd614672e8843727`. Both point to tree `fd0412a414d5150552b7f1873b607d41241824b1`; the remote expected parent was `6425b858af8e542de2be85cfad66c47d59e17569` and the ref now resolves to `a3656cbe48fbc1e8df28c274bd614672e8843727`. This records the latest existing source/docs commits as pushed; new work in this run must be separately committed and CAS-updated.


## Authoritative inventory checkpoint replay — 2026-10-27

The local documentation commit `e0655f06d39c87ddb5f5d1b37e2803f937883ac6` was pushed as remote `d4522f7dad1e88f2dae0ffd39f057edbdef26b2c` by an expected-parent update from `a3656cbe48fbc1e8df28c274bd614672e8843727`. Both have tree `4137bfa2dd3748652e26af30048c9ed577511327`. This batch makes `NEXT_AGENT.md` and the dependency index authoritative about the recovered profile, missing `.308` mesh edges, pushed-commit accounting and next action.
