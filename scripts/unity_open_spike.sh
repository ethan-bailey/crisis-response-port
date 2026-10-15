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
# Artifact has already been SHA-verified into a PRIVATE GitHub release.
# Read it without creating a new release or uploading proprietary assets.
if ! gh release download "$TAG" -p "$(basename "$ZIP")" -D "$WORK" \
     > "$WORK/release-download.log" 2>&1; then
  note preserve_error private_release_download_failed; exit 0
fi
ACTUAL="$(sha256sum "$ZIP" | awk '{print $1}')"
note downloaded_sha256 "$ACTUAL"
if [[ "$ACTUAL" != "$SHA" ]]; then note preserve_error private_release_sha_mismatch; exit 0; fi
note source_verified true; note preserved true
if ! unzip -tq "$ZIP" > "$WORK/archive-integrity.log" 2>&1; then
  note preserve_error corrupt_archive; exit 0
fi

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

# Personal seat activation, import, and return occur on SAME Docker container.
# Signals/failure call return, and a separate Actions "always()" step covers cancellation.
CONTAINER="crisis-unity-personal-$GITHUB_RUN_ID"
ACTIVE=false
scrub() {
  test -f "$1" || return 0
  python3 - "$1" "$2" <<'PY'
import os,sys,re
s=open(sys.argv[1],errors="replace").read()
for k in ('UNITY_EMAIL','UNITY_PASSWORD'):
    secret=os.environ.get(k,'')
    if secret:s=s.replace(secret,'[REDACTED]')
s=re.sub(r'(?im)^.*(?:bearer |access.?token|refresh.?token|id.?token).*$','[REDACTED TOKEN]',s)
s=re.sub(r'[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}','[EMAIL]',s)
open(sys.argv[2],'w').write(s[:150000])
PY
  rm -f "$1"
}
return_seat() {
  if [[ "$ACTIVE" == true ]]; then
    note license_return_attempted true
    timeout 110 docker exec "$CONTAINER" /bin/bash -lc '
      c="$(command -v Unity.Licensing.Client || find /opt/unity -type f -name Unity.Licensing.Client -print -quit)"
      test -n "$c" || { echo "LICENSE_CLIENT_MISSING"; exit 127; }
      "$c" --return-ulf
    ' >"$WORK/return.raw" 2>&1
    rc=$?; note license_return_exit "$rc"
    scrub "$WORK/return.raw" "$WORK/license-return.log"
    if [[ $rc -ne 0 ]]; then
      timeout 80 docker exec "$CONTAINER" /opt/unity/Editor/Unity \
        -batchmode -nographics -quit -returnlicense -logFile /spike/return-editor.log \
        >"$WORK/return-editor.raw" 2>&1
      note license_return_legacy_exit "$?"
      scrub "$WORK/return-editor.raw" "$WORK/return-editor-stdout.log"
    fi
    docker rm -f "$CONTAINER" >/dev/null 2>&1 || true
    ACTIVE=false
  fi
}
trap 'return_seat' EXIT
trap 'exit 130' INT
trap 'exit 143' TERM
if [[ -z "$UNITY_EMAIL" || -z "$UNITY_PASSWORD" ]]; then
  note activation_error github_personal_credentials_absent
  note build_skipped_reason no_personal_credentials; exit 0
fi
note license_present true
activate_editor() {
  IMAGE="$1"; note docker_image "$IMAGE"
  timeout 900 docker pull "$IMAGE" > "$WORK/docker-pull.log" 2>&1
  rc=$?; note docker_exit "$rc"
  if [[ $rc -ne 0 ]]; then note activation_error docker_pull_failed; return 2; fi
  docker run -d --name "$CONTAINER" \
    -v "$WORK:/spike" -w /spike -e UNITY_EMAIL -e UNITY_PASSWORD \
    --entrypoint /bin/bash "$IMAGE" -lc 'while :; do sleep 5; done' \
    > "$WORK/container-start.log" 2>&1 || return 2
  ACTIVE=true
  timeout 150 docker exec "$CONTAINER" /bin/bash -lc '
    c="$(command -v Unity.Licensing.Client || find /opt/unity -type f -name Unity.Licensing.Client -print -quit)"
    test -n "$c" || { echo "LICENSE_CLIENT_MISSING"; exit 127; }
    "$c" --activate-all --include-personal \
      --username "$UNITY_EMAIL" --password "$UNITY_PASSWORD"
  ' > "$WORK/activation.raw" 2>&1
  rc=$?; note personal_activation_exit "$rc"
  scrub "$WORK/activation.raw" "$WORK/license-activation.log"
  if [[ $rc -ne 0 ]]; then note activation_error unity_personal_activation_failed; return 1; fi
  note personal_activation_succeeded true
  return 0
}

# 2022.3.13f1 was tried in run 38763803865 and rejected
# --activate-all --include-personal with CommandLine.UnknownOptionError.
# Use the verified Personal activation capable LTS patch directly.
IMAGE="unityci/editor:ubuntu-2022.3.62f1-base-3"
note fallback_editor_version 2022.3.62f1
note webgl_requested false
activate_editor "$IMAGE" || {
  note build_skipped_reason personal_62_activation_failed
  exit 0
}

touch "$WORK/import.log"
note import_attempted true; note editor_detected true
timeout 900 docker exec "$CONTAINER" /opt/unity/Editor/Unity \
  -batchmode -nographics -quit \
  -projectPath /spike/recovery/export/ExportedProject \
  -logFile /spike/import.log \
  >"$WORK/docker-import.log" 2>&1
IMPORT=$?; note import_exit "$IMPORT"
python3 scripts/classify_unity_spike.py --log "$WORK/import.log" \
  --status-file "$STATUS" --out "$WORK/pre-build.json" \
  > "$WORK/classify.stdout.log" 2>&1 || true
ERRORS="$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1]))["counts"]["compile"])' "$WORK/pre-build.json" 2>/dev/null || echo 999)"
if [[ "$IMPORT" -ne 0 || "$ERRORS" -gt 0 ]]; then
  note build_skipped_reason import_or_compilation_failed; exit 0
fi

# WebGL module can require substantial disk. Keep source original untouched.
FREE_GB="$(df -Pk "$ROOT" | awk 'NR==2{printf "%.0f",$4/1048576}')"
if (( FREE_GB < 12 )); then
  note build_skipped_reason disk_prevented_webgl_module; exit 0
fi
WEBGL_IMAGE="$(printf '%s' "$IMAGE" | sed 's/-base-3$/-webgl-3/')"
return_seat
note webgl_requested true
activate_editor "$WEBGL_IMAGE" || {
  note build_skipped_reason webgl_image_or_activation_failed; exit 0
}

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
timeout 750 docker exec "$CONTAINER" /opt/unity/Editor/Unity \
  -batchmode -nographics -quit -projectPath /spike/recovery/export/ExportedProject \
  -executeMethod SpikeWebGL.BuildMarksman -logFile /spike/webgl.log \
  > "$WORK/docker-webgl.log" 2>&1
RC=$?; note build_exit "$RC"
echo "Marksman WebGL test finished with exit $RC"
exit 0
