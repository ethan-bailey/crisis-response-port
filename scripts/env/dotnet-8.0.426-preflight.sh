# Source this file from the repository root before any .NET operation:
#   source scripts/env/dotnet-8.0.426-preflight.sh
# It exports the verified SDK and runner-only process shim when present.

_cpr_sdk_candidates=(
  "${CPR_DOTNET_ROOT:-}/dotnet"
  /tmp/codex-dotnet-8.0.426/dotnet
  /root/.dotnet/dotnet
)
_cpr_dotnet=""
for _cpr_candidate in "${_cpr_sdk_candidates[@]}"; do
  if [[ -n "$_cpr_candidate" && -x "$_cpr_candidate" ]]; then
    _cpr_dotnet="$_cpr_candidate"
    break
  fi
done
if [[ -z "$_cpr_dotnet" ]]; then
  printf 'Pinned .NET SDK 8.0.426 executable not found in known locations.\n' >&2
  return 1 2>/dev/null || exit 1
fi

export DOTNET_ROOT="$(dirname "$_cpr_dotnet")"
export PATH="$DOTNET_ROOT:$PATH"
for _cpr_shim in /workspace/libprocshim.so /tmp/libprocshim.so; do
  if [[ -f "$_cpr_shim" ]]; then
    export LD_PRELOAD="${LD_PRELOAD:+$LD_PRELOAD:}$_cpr_shim"
    break
  fi
done

_cpr_version="$(dotnet --version 2>&1)" || {
  printf 'Pinned dotnet executable failed during startup: %s\n' "$_cpr_version" >&2
  return 1 2>/dev/null || exit 1
}
if [[ "$_cpr_version" != "8.0.426" ]]; then
  printf 'Expected .NET SDK 8.0.426, found %s at %s\n' "$_cpr_version" "$_cpr_dotnet" >&2
  return 1 2>/dev/null || exit 1
fi
printf 'Verified .NET SDK %s at %s\n' "$_cpr_version" "$_cpr_dotnet"
unset _cpr_sdk_candidates _cpr_dotnet _cpr_candidate _cpr_shim _cpr_version
