#!/usr/bin/env bash
# Recreate the repository's pinned SDK and Playwright Chromium after runner wipes.
set -uo pipefail

_bootstrap_script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
_bootstrap_root="$(git -C "$_bootstrap_script_dir/../.." rev-parse --show-toplevel 2>/dev/null)" || {
  printf 'FAIL bootstrap: could not locate the repository root from %s\n' "$_bootstrap_script_dir" >&2
  exit 1
}

fail() {
  printf 'FAIL %s: %s\n' "$1" "$2" >&2
  exit 1
}

# Keep all installer artifacts outside the checkout and remove them on exit.
_bootstrap_tmp="$(mktemp -d "${TMPDIR:-/tmp}/cpr-bootstrap.XXXXXX")" || fail 'bootstrap' 'mktemp failed'
trap 'rm -rf "$_bootstrap_tmp"' EXIT

# Some runners expose process statistics incompatibly to .NET. Build the
# repository's documented runner shim when no usable copy is already present.
if [[ ! -f /workspace/libprocshim.so && ! -f /tmp/libprocshim.so && -f "$_bootstrap_root/scripts/env/procstat-namespace-shim.c" ]] && command -v gcc >/dev/null 2>&1; then
  if gcc -shared -fPIC -O2 -Wall -Wextra -o /tmp/libprocshim.so \
    "$_bootstrap_root/scripts/env/procstat-namespace-shim.c" -ldl >"$_bootstrap_tmp/procshim-build.log" 2>&1; then
    export LD_PRELOAD="${LD_PRELOAD:+$LD_PRELOAD:}/tmp/libprocshim.so"
  else
    printf 'WARN runner process-stat shim build failed: %s\n' "$(cat "$_bootstrap_tmp/procshim-build.log")" >&2
  fi
elif [[ -f /workspace/libprocshim.so ]]; then
  export LD_PRELOAD="${LD_PRELOAD:+$LD_PRELOAD:}/workspace/libprocshim.so"
elif [[ -f /tmp/libprocshim.so ]]; then
  export LD_PRELOAD="${LD_PRELOAD:+$LD_PRELOAD:}/tmp/libprocshim.so"
fi

# .NET SDK 8.0.426: accept an already-available exact version, otherwise install
# the exact pinned SDK under ~/.dotnet with Microsoft's installer.
_sdk_path=""
_sdk_version=""
_sdk_last_error='no dotnet executable found on PATH or in known user-local locations'
for _candidate in "$(command -v dotnet 2>/dev/null || true)" "$HOME/.dotnet/dotnet" /tmp/codex-dotnet-8.0.426/dotnet; do
  [[ -n "$_candidate" && -x "$_candidate" ]] || continue
  _candidate_version="$("$_candidate" --version 2>&1)"
  _candidate_status=$?
  if [[ $_candidate_status -eq 0 && "$_candidate_version" == '8.0.426' ]]; then
    _sdk_path="$_candidate"
    _sdk_version="$_candidate_version"
    break
  fi
  _sdk_last_error="$_candidate --version failed (exit $_candidate_status): $_candidate_version"
done

if [[ -z "$_sdk_path" ]]; then
  _installer="$_bootstrap_tmp/dotnet-install.sh"
  if ! curl -fL --retry 2 --retry-delay 1 https://dot.net/v1/dotnet-install.sh -o "$_installer" >"$_bootstrap_tmp/dotnet-download.log" 2>&1; then
    _error="$(cat "$_bootstrap_tmp/dotnet-download.log")"
    fail '.NET SDK 8.0.426' "failed downloading Microsoft's dotnet-install script: ${_error:-curl failed}"
  fi
  # This runner disallows archive owner restoration. Keep the official
  # dotnet-install script, but make its tar extraction use --no-same-owner.
  _real_tar="$(command -v tar 2>/dev/null)"
  [[ -n "$_real_tar" ]] || fail '.NET SDK 8.0.426' 'tar is unavailable for installer extraction'
  mkdir -p "$_bootstrap_tmp/bin"
  cat >"$_bootstrap_tmp/bin/tar" <<'TAR_WRAPPER'
#!/bin/sh
exec "$CPR_BOOTSTRAP_REAL_TAR" --no-same-owner "$@"
TAR_WRAPPER
  chmod +x "$_bootstrap_tmp/bin/tar"
  if ! CPR_BOOTSTRAP_REAL_TAR="$_real_tar" PATH="$_bootstrap_tmp/bin:$PATH" bash "$_installer" --version 8.0.426 --install-dir "$HOME/.dotnet" --no-path >"$_bootstrap_tmp/dotnet-install.log" 2>&1; then
    _error="$(tail -n 30 "$_bootstrap_tmp/dotnet-install.log")"
    fail '.NET SDK 8.0.426' "Microsoft dotnet-install failed: ${_error:-installer failed}"
  fi
  export DOTNET_ROOT="$HOME/.dotnet"
  export PATH="$DOTNET_ROOT:$PATH"
  _sdk_path="$DOTNET_ROOT/dotnet"
  if [[ ! -x "$_sdk_path" ]]; then
    _install_log="$(cat "$_bootstrap_tmp/dotnet-install.log")"
    fail '.NET SDK 8.0.426' "installer completed without executable at $_sdk_path: $_install_log"
  fi
  _sdk_version="$("$_sdk_path" --version 2>&1)"
  _sdk_status=$?
  if [[ $_sdk_status -ne 0 || "$_sdk_version" != '8.0.426' ]]; then
    fail '.NET SDK 8.0.426' "post-install verification failed (exit $_sdk_status): ${_sdk_version:-empty version output}"
  fi
else
  export DOTNET_ROOT="$(dirname -- "$_sdk_path")"
  export PATH="$DOTNET_ROOT:$PATH"
fi
printf 'PASS .NET SDK %s at %s\n' "$_sdk_version" "$_sdk_path"

# Install exact package-lock dependencies if Playwright's pinned metadata is not
# present, then derive the browser revision and version from that metadata.
_playwright_metadata="$_bootstrap_root/game/node_modules/playwright-core/browsers.json"
if [[ ! -f "$_playwright_metadata" ]]; then
  if ! command -v npm >/dev/null 2>&1; then
    fail 'Playwright Chromium' 'pinned Playwright dependencies are absent and npm is unavailable'
  fi
  if ! npm ci --prefix "$_bootstrap_root/game" >"$_bootstrap_tmp/npm-ci.log" 2>&1; then
    _error="$(cat "$_bootstrap_tmp/npm-ci.log")"
    fail 'Playwright Chromium' "npm ci failed: ${_error:-npm ci failed}"
  fi
fi
if [[ ! -f "$_playwright_metadata" ]]; then
  fail 'Playwright Chromium' "pinned Playwright metadata is missing after npm ci: $_playwright_metadata"
fi
if ! command -v node >/dev/null 2>&1; then
  fail 'Playwright Chromium' 'node is unavailable to read the pinned Playwright browser metadata'
fi
_browser_info="$(node -e 'const b=require(process.argv[1]).browsers.find(x=>x.name==="chromium"); if(!b) process.exit(2); process.stdout.write(`${b.revision}\t${b.browserVersion}`)' "$_playwright_metadata" 2>&1)" || fail 'Playwright Chromium' "could not read pinned Chromium revision/version: $_browser_info"
IFS=$'\t' read -r _browser_revision _browser_expected <<< "$_browser_info"
[[ -n "$_browser_revision" && -n "$_browser_expected" ]] || fail 'Playwright Chromium' "incomplete pinned browser metadata: $_browser_info"
_browser_root="${PLAYWRIGHT_BROWSERS_PATH:-$HOME/.cache/ms-playwright}"
_browser_executable="$_browser_root/chromium-$_browser_revision/chrome-linux64/chrome"

if [[ ! -x "$_browser_executable" ]]; then
  _playwright_cli="$_bootstrap_root/game/node_modules/.bin/playwright"
  if [[ ! -x "$_playwright_cli" ]]; then
    fail 'Playwright Chromium' "pinned Playwright CLI is missing: $_playwright_cli"
  fi
  if ! "$_playwright_cli" install chromium >"$_bootstrap_tmp/playwright-install-default.log" 2>&1; then
    if ! PLAYWRIGHT_DOWNLOAD_HOST=https://playwright-akamai.azureedge.net \
      "$_playwright_cli" install chromium >"$_bootstrap_tmp/playwright-install-akamai.log" 2>&1; then
      _default_error="$(tail -n 12 "$_bootstrap_tmp/playwright-install-default.log")"
      _akamai_error="$(tail -n 12 "$_bootstrap_tmp/playwright-install-akamai.log")"
      fail 'Playwright Chromium' "pinned install failed on default host: ${_default_error:-no output}; Akamai mirror failed: ${_akamai_error:-no output}"
    fi
  fi
fi
if [[ ! -x "$_browser_executable" ]]; then
  fail 'Playwright Chromium' "pinned install did not create expected executable: $_browser_executable"
fi
_browser_version="$("$_browser_executable" --headless --no-sandbox --disable-gpu --version 2>&1)"
_browser_status=$?
if [[ $_browser_status -ne 0 || "$_browser_version" != *"$_browser_expected"* ]]; then
  fail 'Playwright Chromium' "expected $_browser_expected at $_browser_executable; launch/version check exit $_browser_status: $_browser_version"
fi
printf 'PASS Playwright Chromium %s at %s\n' "$_browser_expected" "$_browser_executable"

unset _bootstrap_script_dir _bootstrap_root _installer _real_tar _sdk_path \
  _sdk_version _sdk_last_error _candidate _candidate_version _candidate_status \
  _install_log _sdk_status _playwright_metadata _error _default_error \
  _akamai_error _browser_info \
  _browser_revision _browser_expected _browser_root _browser_executable \
  _playwright_cli _browser_version _browser_status
