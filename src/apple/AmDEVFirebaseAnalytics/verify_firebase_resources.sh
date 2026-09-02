#!/bin/bash
# Read-only validation for an archived or embedded wrapper framework.
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)"
FRAMEWORK="${1:?Usage: verify_firebase_resources.sh path/to/AmDEVFirebaseAnalytics.framework}"
RESOURCE_DIR="$FRAMEWORK"
if [[ -d "$FRAMEWORK/Versions/A/Resources" ]]; then
    RESOURCE_DIR="$FRAMEWORK/Versions/A/Resources"
fi
if [[ ! -s "$RESOURCE_DIR/PrivacyInfo.xcprivacy" ]]; then
    echo "Wrapper privacy manifest is missing: $RESOURCE_DIR" >&2
    exit 1
fi
/usr/bin/plutil -lint "$RESOURCE_DIR/PrivacyInfo.xcprivacy"
shopt -s nullglob
while IFS= read -r target || [[ -n "$target" ]]; do
    [[ -z "$target" || "$target" == \#* ]] && continue
    matches=("$RESOURCE_DIR/"*_"$target.bundle")
    if [[ "${#matches[@]}" -eq 0 ]]; then
        matches=("$RESOURCE_DIR/"*_"${target//-/_}.bundle")
    fi
    if [[ "${#matches[@]}" -ne 1 ]]; then
        echo "Missing or ambiguous resource bundle for $target: $RESOURCE_DIR" >&2
        exit 1
    fi
    manifest="${matches[0]}/PrivacyInfo.xcprivacy"
    if [[ ! -f "$manifest" ]]; then
        manifest="${matches[0]}/Contents/Resources/PrivacyInfo.xcprivacy"
    fi
    if [[ ! -s "$manifest" ]]; then
        echo "Privacy manifest is missing from ${matches[0]}." >&2
        exit 1
    fi
    /usr/bin/plutil -lint "$manifest"
done < "$SCRIPT_DIR/required_resource_bundles.txt"
