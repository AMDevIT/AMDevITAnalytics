#!/bin/bash
# Xcode build phase: keep SPM-produced resource bundles inside the wrapper.
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)"
: "${BUILT_PRODUCTS_DIR:?Run this script from the wrapper Xcode target}"
: "${TARGET_BUILD_DIR:?Missing Xcode target directory}"
: "${UNLOCALIZED_RESOURCES_FOLDER_PATH:?Missing framework resources directory}"
RESOURCE_DIR="$TARGET_BUILD_DIR/$UNLOCALIZED_RESOURCES_FOLDER_PATH"

if [[ "${PRODUCT_NAME:-}" != AmDEVFirebaseAnalytics ]]; then
    echo "Resource packaging is only supported for AmDEVFirebaseAnalytics." >&2
    exit 1
fi
mkdir -p "$RESOURCE_DIR"
shopt -s nullglob
while IFS= read -r target || [[ -n "$target" ]]; do
    [[ -z "$target" || "$target" == \#* ]] && continue
    # Xcode versions can sanitize '-' in the generated SPM bundle name.
    matches=("$BUILT_PRODUCTS_DIR/"*_"$target.bundle")
    if [[ "${#matches[@]}" -eq 0 ]]; then
        matches=("$BUILT_PRODUCTS_DIR/"*_"${target//-/_}.bundle")
    fi
    if [[ "${#matches[@]}" -ne 1 || ! -d "${matches[0]}" ]]; then
        echo "Expected exactly one SPM resource bundle for $target in $BUILT_PRODUCTS_DIR." >&2
        exit 1
    fi
    bundle="${matches[0]}"
    /usr/bin/ditto "$bundle" "$RESOURCE_DIR/$(basename -- "$bundle")"
done < "$SCRIPT_DIR/required_resource_bundles.txt"

bash "$SCRIPT_DIR/verify_firebase_resources.sh" "$TARGET_BUILD_DIR/$WRAPPER_NAME"
