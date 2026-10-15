#!/usr/bin/env bash
# Bounded, source-preserving Unity compatibility diagnosis.
set -uo pipefail
ROOT="$PWD"; WORK="$ROOT/work/unity-spike"; mkdir -p "$WORK"
STATUS="$WORK/status.env"; : > "$STATUS"
note(){ printf '%s=%s\n' "$1" "$2" >> "$STATUS"; }
SHA=cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d
TAG=recovery-preservation-2026-10-15
ZIP="$WORK/crisis-response-recovery-direct-v3-1.zip"
note "runner" "$(uname -srm | tr ' ' '_')"
note "disk_available_kb" "$(df -Pk "$ROOT" | awk 'NR==2{print $4}')"
note "source_verified" false; note "preserved" false
note "import_attempted" false; note "editor_detected" false
note "license_present" false; note "build_attempted" false
note "build_skipped_reason" import_not_run
df -h "$ROOT" > "$WORK/disk-before.txt" 2>&1 || true
PRIVATE="$(gh api "repos/$GITHUB_REPOSITORY" --jq '.private' 2>"$WORK/github-error.txt")"
if [[ "$PRIVATE" != true ]]; then note preserve_error repo_privacy_not_verified; exit 0; fi
if ! curl -fsSL --retry 3 --retry-all-errors \
    -H "Authorization: Bearer $GH_TOKEN" -H "Accept: application/vnd.github+json" \
    -o "$ZIP" "https://api.github.com/repos/$GITHUB_REPOSITORY/actions/artifacts/11903811738/zip" \
    >"$WORK/download.stdout.log" 2>"$WORK/download.stderr.log"; then
  note preserve_error artifact_download_failed; exit 0
fi
ACTUAL="$(sha256sum "$ZIP" | awk '{print $1}')"
note downloaded_sha256 "$ACTUAL"
if [[ "$ACTUAL" != "$SHA" ]]; then note preserve_error artifact_digest_mismatch; exit 0; fi
note source_verified true
if ! unzip -tq "$ZIP" > "$WORK/archive-integrity.log" 2>&1; then
  note preserve_error corrupt_archive; exit 0
fi
if ! gh release view "$TAG" >/dev/null 2>&1; then
  gh release create "$TAG" --target "$GITHUB_SHA" --prerelease --latest=false \
    --title "PRIVATE Crisis Response recovery backup" \
    --notes "PRIVATE third-party recovered assets. Never make this repository public while attached. Source Actions artifact ID 11903811738, original expiry 2026-10-28." \
    > "$WORK/release-create.log" 2>&1 ||
    { note preserve_error release_create_failed; exit 0; }
fi
if ! gh release view "$TAG" --json assets --jq '.assets[].name' | grep -Fxq "$(basename "$ZIP")"; then
  gh release upload "$TAG" "$ZIP" > "$WORK/release-upload.log" 2>&1 ||
    { note preserve_error release_upload_failed; exit 0; }
fi
mkdir -p "$WORK/verify-release"
if gh release download "$TAG" -p "$(basename "$ZIP")" -D "$WORK/verify-release" \
    > "$WORK/release-verify.log" 2>&1 &&
   (cd "$WORK/verify-release" && printf '%s  %s\n' "$SHA" "$(basename "$ZIP")" | sha256sum -c -); then
  note preserved true
else note preserve_error release_download_or_integrity_failed; exit 0; fi
rm -rf "$WORK/verify-release"
echo "Private release preservation verified: $TAG"

mkdir -p "$WORK/recovery"
if ! unzip -q "$ZIP" -d "$WORK/recovery"; then note preserve_error extract_failed; exit 0; fi
PROJECT="$WORK/recovery/export/ExportedProject"
if [[ ! -d "$PROJECT/Assets" || ! -f "$PROJECT/ProjectSettings/ProjectVersion.txt" ]]; then
  note preserve_error project_root_missing
  find "$WORK/recovery" -maxdepth 5 -name ProjectVersion.txt > "$WORK/project-candidates.txt"
  exit 0
fi
note project_detected true
find "$PROJECT/Assets" -name '*.unity' -type f | sort > "$WORK/scene-inventory.txt"
find "$PROJECT/Assets" -name '*.cs' -type f | wc -l > "$WORK/project-script-count.txt"
cat "$PROJECT/ProjectSettings/ProjectVersion.txt" > "$WORK/recovered-project-version.txt"
du -sh "$PROJECT" > "$WORK/recovered-project-size.txt"

# Gate the larger WebGL image on free disk before image extraction.
FREE_GB="$(df -Pk "$ROOT" | awk 'NR==2{printf "%.0f",$4/1048576}')"
IMAGE="unityci/editor:ubuntu-2022.3.13f1-base-3"; WEBGL=false
if (( FREE_GB >= 24 )); then
  IMAGE="unityci/editor:ubuntu-2022.3.13f1-webgl-3"; WEBGL=true
fi
note docker_image "$IMAGE"; note webgl_requested "$WEBGL"
echo "Pulling editor image: $IMAGE (free disk $FREE_GB GiB)"
timeout 900 docker pull "$IMAGE" > "$WORK/docker-pull.log" 2>&1
PULL=$?; note docker_exit "$PULL"
if [[ $PULL -ne 0 ]]; then note build_skipped_reason editor_image_pull_failed; exit 0; fi

mkdir -p "$WORK/license"; chmod 700 "$WORK/license"
if [[ -n "$UNITY_LICENSE" ]]; then
  printf '%s' "$UNITY_LICENSE" > "$WORK/license/Unity_lic.ulf"
  chmod 600 "$WORK/license/Unity_lic.ulf"
  note license_present true
fi
touch "$WORK/import.log"
note import_attempted true
timeout 900 docker run --rm --name unity-import-spike -v "$WORK:/spike" -w /spike \
  -v "$WORK/license:/root/.local/share/unity3d/Unity" \
  --entrypoint /bin/bash "$IMAGE" -lc '
    if [[ ! -x /opt/unity/Editor/Unity ]]; then
      echo "Editor binary not found: /opt/unity/Editor/Unity"; exit 127
    fi
    touch /spike/editor-found
    /opt/unity/Editor/Unity -batchmode -nographics -quit \
      -projectPath /spike/recovery/export/ExportedProject \
      -logFile /spike/import.log
  ' > "$WORK/docker-import.log" 2>&1
IMPORT=$?; note import_exit "$IMPORT"
[[ -f "$WORK/editor-found" ]] && note editor_detected true
python3 scripts/classify_unity_spike.py --log "$WORK/import.log" --status-file "$STATUS" \
  --out "$WORK/pre-build.json" > "$WORK/classify.stdout.log" 2>&1 || true
ERRORS="$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1]))["counts"]["compile"])' "$WORK/pre-build.json" 2>/dev/null || echo 999)"
if [[ "$IMPORT" -ne 0 || "$ERRORS" -gt 0 ]]; then
  note build_skipped_reason import_or_compilation_failed; exit 0
fi
if [[ "$WEBGL" != true ]]; then note build_skipped_reason disk_prevented_webgl_module; exit 0; fi

# Transient editor-only test driver. NEVER save it to the source repository.
mkdir -p "$PROJECT/Assets/Editor"
cat > "$PROJECT/Assets/Editor/SpikeWebGL.cs" <<'CSHARP'
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class SpikeWebGL {
  public static void BuildMarksman() {
    string[] scenes=AssetDatabase.FindAssets("t:Scene")
      .Select(AssetDatabase.GUIDToAssetPath)
      .Where(p=>Path.GetFileNameWithoutExtension(p)
        .IndexOf("Marksman",StringComparison.OrdinalIgnoreCase)>=0).ToArray();
    if(scenes.Length!=1) throw new Exception("MARKSMAN_SCENE_COUNT="+scenes.Length);
    var opt=new BuildPlayerOptions {
      scenes=new []{scenes[0]},locationPathName="/spike/webgl",
      target=BuildTarget.WebGL,options=BuildOptions.None
    };
    BuildReport report=BuildPipeline.BuildPlayer(opt);
    if(report.summary.result!=BuildResult.Succeeded)
      throw new Exception("MARKSMAN_WEBGL_RESULT="+report.summary.result);
  }
}
CSHARP
note build_attempted true
note build_skipped_reason none
timeout 750 docker run --rm --name unity-marksman-webgl-spike -v "$WORK:/spike" -w /spike \
  -v "$WORK/license:/root/.local/share/unity3d/Unity" \
  --entrypoint /opt/unity/Editor/Unity "$IMAGE" \
  -batchmode -nographics -quit -projectPath /spike/recovery/export/ExportedProject \
  -executeMethod SpikeWebGL.BuildMarksman -logFile /spike/webgl.log \
  > "$WORK/docker-webgl.log" 2>&1
RC=$?; note build_exit "$RC"
echo "Marksman WebGL test finished with exit $RC"
exit 0
