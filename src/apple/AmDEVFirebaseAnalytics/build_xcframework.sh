#!/bin/bash

# Build unsigned Release archives and package the wrapper with its debug symbols.
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)"
SCHEME="AmDEVFirebaseAnalytics"
PROJECT_PATH="$SCRIPT_DIR/AmDEVFirebaseAnalytics.xcodeproj"
OUTPUT_DIR="$SCRIPT_DIR/build"
XCFRAMEWORK_NAME="AmDEVFirebaseAnalytics"
PLIST_BUDDY="/usr/libexec/PlistBuddy"

if [[ "$(uname -s)" != Darwin ]] || ! command -v xcodebuild >/dev/null 2>&1; then
    echo "This script requires macOS and a selected full Xcode installation." >&2
    exit 1
fi
if [[ ! -d "$PROJECT_PATH" ]]; then
    echo "Xcode project not found: $PROJECT_PATH" >&2
    exit 1
fi

# Do not clean or write through a redirected output directory.
if [[ -L "$OUTPUT_DIR" ]]; then
    echo "Refusing to use a symlink as the build directory: $OUTPUT_DIR" >&2
    exit 1
fi
mkdir -p "$OUTPUT_DIR"
OUTPUT_DIR="$(cd -- "$OUTPUT_DIR" && pwd -P)"
if [[ "$OUTPUT_DIR" != "$SCRIPT_DIR/build" ]]; then
    echo "Build directory is outside the expected project directory." >&2
    exit 1
fi

# Keep per-run archives and logs for diagnostics and symbol upload.
RUN_DIR="$(mktemp -d "$OUTPUT_DIR/archives.XXXXXX")"
echo "Archives and full build logs: $RUN_DIR"

run_xcodebuild() {
    local log_path="$1"
    shift
    if command -v xcpretty >/dev/null 2>&1; then
        xcodebuild "$@" 2>&1 | tee "$log_path" | xcpretty
    else
        xcodebuild "$@" 2>&1 | tee "$log_path"
    fi
}

COMMON_ARGS=(-project "$PROJECT_PATH"
             -scheme "$SCHEME"
             -configuration Release
             -derivedDataPath "$RUN_DIR/DerivedData"
             -clonedSourcePackagesDirPath "$RUN_DIR/SourcePackages")

echo "Resolving the package versions committed in Package.resolved..."
run_xcodebuild "$RUN_DIR/resolve.log" -resolvePackageDependencies "${COMMON_ARGS[@]}" \
    -onlyUsePackageVersionsFromResolvedFile

CREATE_ARGS=()
archive_platform() {
    local platform_name="$1"
    local destination="$2"
    local archive_path="$RUN_DIR/$XCFRAMEWORK_NAME-$platform_name.xcarchive"
    local framework_path="$archive_path/Products/Library/Frameworks/$XCFRAMEWORK_NAME.framework"
    local dsym_path="$archive_path/dSYMs/$XCFRAMEWORK_NAME.framework.dSYM"

    echo "Archiving $platform_name..."
    run_xcodebuild "$RUN_DIR/$platform_name.log" archive "${COMMON_ARGS[@]}" \
        -destination "$destination" \
        -archivePath "$archive_path" \
        -disableAutomaticPackageResolution \
        SKIP_INSTALL=NO \
        BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
        SWIFT_INSTALL_OBJC_HEADER=YES \
        DEFINES_MODULE=YES \
        SUPPORTS_MACCATALYST=YES \
        ONLY_ACTIVE_ARCH=NO \
        DEBUG_INFORMATION_FORMAT=dwarf-with-dsym \
        CODE_SIGNING_ALLOWED=NO

    # Catalyst uses a versioned framework layout; Headers resolves through its symlink.
    if [[ ! -s "$framework_path/Headers/$XCFRAMEWORK_NAME-Swift.h" || ! -d "$dsym_path" ]]; then
        echo "Archive is missing the generated Objective-C header or dSYM: $archive_path" >&2
        exit 1
    fi
    bash "$SCRIPT_DIR/verify_firebase_resources.sh" "$framework_path"
    CREATE_ARGS+=(-framework "$framework_path" -debug-symbols "$dsym_path")
}

archive_platform iOS "generic/platform=iOS"
archive_platform iOSSimulator "generic/platform=iOS Simulator"
archive_platform MacCatalyst "generic/platform=macOS,variant=Mac Catalyst"

STAGED_FRAMEWORK="$RUN_DIR/$XCFRAMEWORK_NAME.xcframework"
run_xcodebuild "$RUN_DIR/package.log" -create-xcframework "${CREATE_ARGS[@]}" \
    -output "$STAGED_FRAMEWORK"

if [[ ! -f "$STAGED_FRAMEWORK/Info.plist" ]]; then
    echo "XCFramework creation did not produce an Info.plist." >&2
    exit 1
fi

# Display the actual slices; do not promise architectures unsupported by this Xcode.
while IFS= read -r -d '' framework_path; do
    bash "$SCRIPT_DIR/verify_firebase_resources.sh" "$framework_path"
done < <(find "$STAGED_FRAMEWORK" -type d -name "$XCFRAMEWORK_NAME.framework" -print0)

"$PLIST_BUDDY" -c 'Print :AvailableLibraries' "$STAGED_FRAMEWORK/Info.plist"

# Only replace the last successful artifact after every archive and packaging step succeeds.
FINAL_FRAMEWORK="$OUTPUT_DIR/$XCFRAMEWORK_NAME.xcframework"
if [[ -L "$FINAL_FRAMEWORK" ]]; then
    echo "Refusing to replace a symlink: $FINAL_FRAMEWORK" >&2
    exit 1
fi
rm -rf -- "$FINAL_FRAMEWORK"
mv -- "$STAGED_FRAMEWORK" "$FINAL_FRAMEWORK"
echo "XCFramework created at $FINAL_FRAMEWORK"
echo "Archives and logs retained at $RUN_DIR"
