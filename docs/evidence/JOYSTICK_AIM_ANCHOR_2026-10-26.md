# Joystick aim anchoring — 2026-10-26

## Change

Source checkpoint: `ccc8b517a6d675ab030972e9dc857e7bf705b77e` on `develop/wasm`.

The movement stick previously wrote its center-relative vertical position directly into the aim target on every pointer event. Touching or sweeping horizontally along the neutral row therefore reset a previously selected aim point to the screen center. The movement adapter now reports the vertical stick value at pointer capture. The Mission UI stores the current aim and stick value as the gesture baseline; each later event maps the vertical offset from that baseline to an absolute aim coordinate. Horizontal displacement does not participate in the aim calculation. Releasing the stick leaves the final aim target in place.

## Verification

- `node --test scripts/browser_input_test.mjs scripts/aim_direction_test.mjs scripts/aim_fire_gate_test.mjs` — **30/30 passed** at the tree committed as `ccc8b517a6d675ab030972e9dc857e7bf705b77e`.
- `git diff --check` — passed.
- GitHub Actions run [#34](https://github.com/ethan-bailey/crisis-response-port/actions/runs/40275621429), exact SHA `ccc8b517a6d675ab030972e9dc857e7bf705b77e` — **success**. Contracts/Release WASM job `121417628876` and Chromium IndexedDB job `121417628127` both completed successfully, including browser installation and the production-loader fixture.

## Acceptance boundary

These checks establish the input mapping and source/build contracts. They do not establish touch feel or full gameplay acceptance on iPhone Safari. Site version 71 remains the live version; its source identifier still does not map to a commit in the available GitHub account, and no new Site package was built or deployed for this change. Verify the exact source on the owner's device after the provenance and package gates are closed.
