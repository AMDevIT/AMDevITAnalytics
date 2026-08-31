# Apple XCFramework and Objective Sharpie scripts

## Objective and status

On 2026-08-31 the user approved correcting all findings from the build-script review.
The scripts, Xcode settings, and documentation have been updated. Source review is complete;
macOS execution and generated-binding validation remain pending explicit approval.

## Decisions

- Resolve default paths from each script's physical directory, quote paths, reject output
  symlinks, and replace only the designated generated output after successful generation.
- Keep per-run archives, full logs, DerivedData, and package checkouts under build/archives.*.
  Retained runs can be large and require manual cleanup. Concurrent runs are unsupported.
- Resolve committed package versions once, then disable automatic resolution for archives.
- Archive unsigned Release variants for device, simulator, and Mac Catalyst with headers
  and dSYMs. Include each dSYM in create-xcframework using its absolute path.
- Let Xcode select supported destination architectures with ONLY_ACTIVE_ARCH=NO; display the
  resulting slices instead of claiming x86_64 support unconditionally.
- Use xcpretty only when already installed; never install tools as part of the build.
- Sharpie reads the iOS arm64 slice from the XCFramework plist, scopes generation to wrapper
  headers, and takes optional explicit device-framework search directories.
- Remove the showBuildSettings/awk pipeline and all mandatory DerivedData/SwiftPM cache
  lookup from Sharpie. Copied frameworks can be used without the original Xcode project.
- Generate C# in a fresh directory and require a nonempty API-definition file before
  replacing output. Failures preserve previous output and retain diagnostics.
- Enable DEFINES_MODULE, SWIFT_INSTALL_OBJC_HEADER, and SUPPORTS_MACCATALYST, and disable
  SKIP_INSTALL in both framework configurations. Preserve the iOS 26.5 minimum.
- Scope Git attributes to Apple shell scripts (LF) and ignore generated Sharpie output.

## Affected files

Under src/apple/AmDEVFirebaseAnalytics/:

- build_xcframework.sh
- extract_sharpie.sh
- AmDEVFirebaseAnalytics.xcodeproj/project.pbxproj
- AmDEVFirebaseAnalytics/AmDEVFirebaseAnalytics.docc/AmDEVFirebaseAnalytics.md
- BUILDING.md (new)
- .gitattributes (new)
- .gitignore (new)

Context: .agents/context.md, .agents/apple-native-api.md, and this file.

## Checks performed

- Git fetch succeeded; Task-Apple-Library still has no upstream and HEAD matched origin/main.
- Preserved the pre-existing uncommitted API additions and context changes.
- Read the official Apple XCFramework/dSYM and Microsoft Sharpie documentation.
- Manually reviewed script control flow, quoting, pipeline status handling, output paths,
  generated-output replacement, build-setting changes, and documentation.
- No scripts, syntax checks, restore, compilation, tests, or Sharpie generation were run.
  AGENTS.md requires separate approval, and this Windows host has no macOS/Xcode execution.

## Open issues and next step

With explicit approval on macOS, run from the repository root and a path containing spaces.
Check all three slices, exported headers, matching dSYMs, and failure preservation. Run
Sharpie with both local and copied artifacts, then review generated selectors, nullability,
enums, callbacks, and Verify attributes before integrating the .NET binding.

This step does not finish NuGet packaging: native dependencies, resource bundles, privacy
manifests, signing, host symbol upload, and coexistence with other Firebase consumers need
an integration audit. .NET projects still target iOS, and FirebaseCoreManager remains
non-idempotent. Neither managed targets nor the initialization API changed in this step.
