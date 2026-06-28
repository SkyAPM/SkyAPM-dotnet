#!/usr/bin/env bash
# Set VersionMajor/Minor/Patch in build/version.props (VersionPrefix is derived).
# Usage: build/set-version.sh X.Y.Z [path-to-version.props]
# Portable across GNU/BSD sed (writes via a temp file, not sed -i).
set -euo pipefail

v="${1:?usage: set-version.sh X.Y.Z [version.props]}"
f="${2:-"$(dirname "$0")/version.props"}"

IFS=. read -r mj mn pt <<< "$v"
{ [ -n "${mj:-}" ] && [ -n "${mn:-}" ] && [ -n "${pt:-}" ]; } \
  || { echo "set-version: version must be X.Y.Z (got '$v')" >&2; exit 1; }
case "${mj}${mn}${pt}" in *[!0-9]*) echo "set-version: version parts must be numeric (got '$v')" >&2; exit 1;; esac
[ -f "$f" ] || { echo "set-version: file not found: $f" >&2; exit 1; }

tmp="$(mktemp)"
sed -E \
  -e "s#<VersionMajor>[0-9]+</VersionMajor>#<VersionMajor>${mj}</VersionMajor>#" \
  -e "s#<VersionMinor>[0-9]+</VersionMinor>#<VersionMinor>${mn}</VersionMinor>#" \
  -e "s#<VersionPatch>[0-9]+</VersionPatch>#<VersionPatch>${pt}</VersionPatch>#" \
  "$f" > "$tmp" && mv "$tmp" "$f"

echo "set $(basename "$f") to ${mj}.${mn}.${pt}"
