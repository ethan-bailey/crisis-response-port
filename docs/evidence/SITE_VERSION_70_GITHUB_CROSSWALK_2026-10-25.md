# Site version 70 source and archive crosswalk — 2026-10-25

## Identity

- Historical Site source commit recorded by its build manifest:
  `d65cdb2fbd1c50f55fbd5c7ab96eb12a242ca684`.
- This commit is absent from public `ethan-bailey/crisis-response-port`
  history. The corresponding published code checkpoint identified in the
  handoff is public `develop/wasm` commit
  `df7aad51045b1e03537ab7a3a49334122eb40831`.
- Site archive: `/tmp/cpr-site-runtime-d65cdb2.tar.gz`, 138,782,955 bytes,
  SHA-256 `a8603142e5591fa6c1b767aa03a1992391d74441e382dda45df159f9b89c7fda`.
- Archive build manifest: `dist/build-manifest.json`; the bounded source
  profile is `global-ui-mission-operator`, 1,919 assets, 97,767,549 compressed
  bytes and 205,514,345 source bytes.

## Crosswalk result

An independent streaming audit compared every listed archive payload entry to
its manifest byte length and SHA-256, then compared each manifest source
inventory path/hash against the public GitHub commit:

- Archive payload: **2,684/2,684** files match size and SHA-256.
- Public source inventory: **1,209/1,209** tracked inputs match the GitHub
  `df7aad51045b1e03537ab7a3a49334122eb40831` file bytes and SHA-256; no missing
  paths or mismatches.
- Additional archive file outside the payload manifest:
  `.openai/hosting.json`, 106 bytes, SHA-256
  `9bdeb8411029e4fbfd4d3289c0c54b86fc53711ef168af5c018816d4fe3ca0ce`.
  This is Site-specific configuration and is not present in the public GitHub
  source commit; it is reported as such, not as a source mismatch.
- `dist/build-manifest.json` is the expected manifest self-reference omission.

The checked archive SHA-256 is
`a8603142e5591fa6c1b767aa03a1992391d74441e382dda45df159f9b89c7fda`, matching
the prior recorded local archive digest. The payload audit therefore connects
the saved version 70 package to the public source checkpoint at the file level,
while preserving the distinct Site commit ID and Site-only configuration.

## Reproduction

The audit opened the gzip tar stream without extracting or modifying it,
streamed SHA-256 for every payload file, and used `git show <public-commit>:<path>`
to compare the source inventory bytes. The original command output was:

```json
{
  "manifestSourceCommit": "d65cdb2fbd1c50f55fbd5c7ab96eb12a242ca684",
  "publicCrosswalkCommit": "df7aad51045b1e03537ab7a3a49334122eb40831",
  "manifestPayloadFiles": 2684,
  "payloadFailureCount": 0,
  "sourceInventoryCount": 1209,
  "sourceCrosswalkFailureCount": 0,
  "unexpectedArchiveFiles": [".openai/hosting.json"]
}
```

This is historical source/package evidence only. No Site was opened, changed,
saved, deployed, or published during this audit. It does not prove version 70
passes current Mission cache changes, current browser acceptance, or iPhone
Safari behavior.
