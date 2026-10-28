# Source from any directory inside the checkout to verify the complete runner:
#   source scripts/env/environment-preflight.sh
# This continues through SDK, Chromium, and push-auth checks after a failure.

_cpr_env_preflight_root="$(git rev-parse --show-toplevel 2>/dev/null)"
_cpr_env_preflight_fail=0

if [[ -z "$_cpr_env_preflight_root" ]]; then
  printf 'Checkout root unavailable; cannot run the complete environment preflight.\n' >&2
  _cpr_env_preflight_fail=1
else
  if source "$_cpr_env_preflight_root/scripts/env/dotnet-8.0.426-preflight.sh"; then
    :
  else
    _cpr_env_preflight_fail=1
  fi

  _cpr_browser_metadata="$_cpr_env_preflight_root/game/node_modules/playwright-core/browsers.json"
  _cpr_chromium_revision=""
  _cpr_chromium_expected=""
  if [[ -f "$_cpr_browser_metadata" ]] && command -v node >/dev/null 2>&1; then
    _cpr_chromium_info="$(node -e 'const b=require(process.argv[1]).browsers.find(x=>x.name==="chromium"); if(b) process.stdout.write(`${b.revision}\t${b.browserVersion}`)' "$_cpr_browser_metadata" 2>&1)" || _cpr_chromium_info=""
    IFS=$'\t' read -r _cpr_chromium_revision _cpr_chromium_expected <<< "$_cpr_chromium_info"
  fi

  _cpr_chromium_root="${PLAYWRIGHT_BROWSERS_PATH:-$HOME/.cache/ms-playwright}"
  _cpr_chromium_path="${CPR_CHROMIUM_PATH:-}"
  if [[ -z "$_cpr_chromium_path" && -n "$_cpr_chromium_revision" ]]; then
    _cpr_chromium_path="$_cpr_chromium_root/chromium-$_cpr_chromium_revision/chrome-linux64/chrome"
  fi
  if [[ ! -x "$_cpr_chromium_path" ]]; then
    for _cpr_browser_name in chromium chromium-browser google-chrome google-chrome-stable; do
      if command -v "$_cpr_browser_name" >/dev/null 2>&1; then
        _cpr_chromium_path="$(command -v "$_cpr_browser_name")"
        break
      fi
    done
  fi
  if [[ -x "$_cpr_chromium_path" ]]; then
    _cpr_chromium_version="$("$_cpr_chromium_path" --headless --no-sandbox --disable-gpu --version 2>&1)"
    _cpr_chromium_status=$?
    if [[ $_cpr_chromium_status -eq 0 && -n "$_cpr_chromium_expected" && "$_cpr_chromium_version" == *"$_cpr_chromium_expected"* ]]; then
      printf 'Verified Chromium %s at %s\n' "$_cpr_chromium_expected" "$_cpr_chromium_path"
    elif [[ $_cpr_chromium_status -eq 0 && -z "$_cpr_chromium_expected" ]]; then
      printf 'Verified Chromium at %s: %s\n' "$_cpr_chromium_path" "$_cpr_chromium_version"
    else
      printf 'Chromium preflight failed at %s: expected %s; got %s\n' "$_cpr_chromium_path" "${_cpr_chromium_expected:-a Playwright-pinned build}" "$_cpr_chromium_version" >&2
      _cpr_env_preflight_fail=1
    fi
  else
    printf 'Chromium executable not found; expected Playwright revision %s under %s.\n' "${_cpr_chromium_revision:-unknown}" "$_cpr_chromium_root" >&2
    _cpr_env_preflight_fail=1
  fi

  _cpr_current_branch="$(git -C "$_cpr_env_preflight_root" branch --show-current 2>&1)"
  if [[ "$_cpr_current_branch" != "develop/wasm" ]]; then
    printf 'Push-auth preflight requires branch develop/wasm; current branch is %s.\n' "$_cpr_current_branch" >&2
    _cpr_env_preflight_fail=1
  else
    _cpr_push_output="$(git -C "$_cpr_env_preflight_root" push --dry-run origin HEAD:refs/heads/develop/wasm 2>&1)"
    _cpr_push_status=$?
    _cpr_push_output="$(printf '%s\n' "$_cpr_push_output" | sed -E 's#https://x-access-token:[^/@[:space:]]+@#https://x-access-token:[REDACTED]@#g')"
    if [[ $_cpr_push_status -eq 0 ]]; then
      printf 'Verified git push authentication (dry-run): %s\n' "$_cpr_push_output"
    else
      printf 'Git push-auth preflight failed (exit %s): %s\n' "$_cpr_push_status" "$_cpr_push_output" >&2
      _cpr_env_preflight_fail=1
    fi
  fi
fi

unset _cpr_env_preflight_root _cpr_browser_metadata \
  _cpr_chromium_revision _cpr_chromium_expected _cpr_chromium_info \
  _cpr_chromium_root _cpr_chromium_path _cpr_browser_name _cpr_chromium_version \
  _cpr_chromium_status _cpr_current_branch _cpr_push_output _cpr_push_status

if [[ $_cpr_env_preflight_fail -eq 0 ]]; then
  unset _cpr_env_preflight_fail
  return 0 2>/dev/null || exit 0
fi
unset _cpr_env_preflight_fail
return 1 2>/dev/null || exit 1
