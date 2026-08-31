#!/bin/bash

# Usage: bash extract_sharpie.sh [XCFramework path] [dependency framework directory ...]
# Extra directories contain the device .framework slices, not the parent .xcframework.
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)"
XCFRAMEWORK_NAME="AmDEVFirebaseAnalytics"
XCFRAMEWORK_PATH="${1:-$SCRIPT_DIR/build/$XCFRAMEWORK_NAME.xcframework}"
OUTPUT_PATH="$SCRIPT_DIR/sharpie-output"
PLIST_BUDDY="/usr/libexec/PlistBuddy"
if [[ $# -gt 0 ]]; then
    shift
fi

if [[ "$(uname -s)" != Darwin ]] || ! command -v xcrun >/dev/null 2>&1; then
    echo "This script requires macOS and a selected full Xcode installation." >&2
    exit 1
fi
if ! command -v sharpie >/dev/null 2>&1; then
    echo "Install Objective Sharpie and the .NET iOS workload before running this script." >&2
    exit 1
fi
if [[ ! -d "$XCFRAMEWORK_PATH" ]]; then
    echo "XCFramework not found: $XCFRAMEWORK_PATH. Run build_xcframework.sh first." >&2
    exit 1
fi
XCFRAMEWORK_PATH="$(cd -- "$XCFRAMEWORK_PATH" && pwd -P)"
if [[ ! -f "$XCFRAMEWORK_PATH/Info.plist" ]]; then
    echo "XCFramework Info.plist not found: $XCFRAMEWORK_PATH" >&2
    exit 1
fi

# Read the actual library identifier rather than assuming the directory is ios-arm64.
FRAMEWORK_PATH=""
library_index=0
while library_id="$("$PLIST_BUDDY" -c "Print :AvailableLibraries:$library_index:LibraryIdentifier" "$XCFRAMEWORK_PATH/Info.plist" 2>/dev/null)"; do
    platform="$("$PLIST_BUDDY" -c "Print :AvailableLibraries:$library_index:SupportedPlatform" "$XCFRAMEWORK_PATH/Info.plist")"
    variant="$("$PLIST_BUDDY" -c "Print :AvailableLibraries:$library_index:SupportedPlatformVariant" "$XCFRAMEWORK_PATH/Info.plist" 2>/dev/null || true)"
    architectures="$("$PLIST_BUDDY" -c "Print :AvailableLibraries:$library_index:SupportedArchitectures" "$XCFRAMEWORK_PATH/Info.plist")"
    library_path="$("$PLIST_BUDDY" -c "Print :AvailableLibraries:$library_index:LibraryPath" "$XCFRAMEWORK_PATH/Info.plist")"

    if [[ "$platform" == ios && -z "$variant" && "$architectures" == *arm64* ]]; then
        if [[ "$library_id" == */* || "$library_id" == . || "$library_id" == .. || "$library_path" != "$XCFRAMEWORK_NAME.framework" ]]; then
            echo "Unexpected XCFramework library path: $library_id/$library_path" >&2
            exit 1
        fi
        FRAMEWORK_PATH="$XCFRAMEWORK_PATH/$library_id/$library_path"
        break
    fi
    library_index=$((library_index + 1))
done

HEADER_PATH="$FRAMEWORK_PATH/Headers/$XCFRAMEWORK_NAME-Swift.h"
if [[ -z "$FRAMEWORK_PATH" || ! -s "$HEADER_PATH" ]]; then
    echo "No iOS arm64 framework with a generated Swift header was found." >&2
    exit 1
fi

CLANG_ARGS=(-F "$(dirname -- "$FRAMEWORK_PATH")" -fmodules -arch arm64)
for dependency_dir in "$@"; do
    if [[ ! -d "$dependency_dir" ]]; then
        echo "Dependency framework directory not found: $dependency_dir" >&2
        exit 1
    fi
    dependency_dir="$(cd -- "$dependency_dir" && pwd -P)"
    CLANG_ARGS+=(-F "$dependency_dir")
done
SDK_VERSION="$(xcrun --sdk iphoneos --show-sdk-version)"

# Generate into a fresh directory so stale C# files cannot hide a failed extraction.
if [[ -L "$OUTPUT_PATH" ]]; then
    echo "Refusing to replace a symlink: $OUTPUT_PATH" >&2
    exit 1
fi
STAGING_PATH="$(mktemp -d "$SCRIPT_DIR/.sharpie-output.XXXXXX")"
echo "Extracting $HEADER_PATH"
echo "Generation directory and diagnostic log: $STAGING_PATH"
if ! sharpie bind --header "$HEADER_PATH" --scope "$FRAMEWORK_PATH/Headers" \
    -o "$STAGING_PATH" -n "AMDevIT.Analytics.Firebase.BindingApple" \
    -sdk "iphoneos$SDK_VERSION" -v -c "${CLANG_ARGS[@]}" 2>&1 | tee "$STAGING_PATH/sharpie.log"; then
    echo "Sharpie failed; the previous output was preserved. See $STAGING_PATH/sharpie.log." >&2
    echo "If an imported module is missing, supply its device framework directory explicitly." >&2
    exit 1
fi

if [[ ! -s "$STAGING_PATH/ApiDefinitions.cs" && ! -s "$STAGING_PATH/ApiDefinition.cs" ]]; then
    echo "Sharpie did not produce API definitions. Inspect $STAGING_PATH/sharpie.log." >&2
    exit 1
fi

rm -rf -- "$OUTPUT_PATH"
mv -- "$STAGING_PATH" "$OUTPUT_PATH"
echo "Generated binding files: $OUTPUT_PATH"
echo "Review selectors, nullability, enums, callbacks, and Verify attributes before integration."
