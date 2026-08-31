# Building the Apple wrapper and generating .NET bindings

## Prerequisites

- macOS with full Xcode selected through `xcode-select` and its license accepted.
- An Xcode/SDK version supporting this project's iOS 26.5 deployment target and the
  Firebase version pinned in `Package.resolved`.
- Network access for the first Swift Package Manager resolution.
- Objective Sharpie with the `--header` and `--scope` options, and the .NET iOS workload,
  for C# generation. Install these separately; the scripts do not install tools.
- `xcpretty` is optional. Full build logs are retained with or without it.

## Build

From the repository root:

```bash
bash src/apple/AmDEVFirebaseAnalytics/build_xcframework.sh
```

The scripts resolve their default paths relative to their own location. They can also
be invoked using an absolute path from any directory, including directories with spaces.

The build script resolves the committed package versions, then creates unsigned Release
archives for iOS devices, iOS Simulator, and Mac Catalyst. It enables module/header export,
library distribution, dSYM generation, and all architectures supported by the selected
destination. It prints the resulting XCFramework slices instead of assuming Intel
simulator support in every Xcode release.

The output is `build/AmDEVFirebaseAnalytics.xcframework`, including each wrapper dSYM.
Full archives, package checkouts, DerivedData, and logs are retained under a fresh
`build/archives.XXXXXX` directory per invocation. These directories can be large; remove
obsolete runs manually after retaining any required symbols. Failed runs retain their
logs and leave the previous XCFramework untouched. Only a completed package replaces it.
Do not run multiple builds concurrently in the same checkout.

The artifacts are unsigned to avoid depending on the original author's development team.
Signing for distribution and signing/embedding in the final app remain release steps.
No package or framework is published by these scripts.

## Generate Objective Sharpie definitions

After a successful build:

```bash
bash src/apple/AmDEVFirebaseAnalytics/extract_sharpie.sh
```

The extractor reads the device slice from the XCFramework's `Info.plist`, uses its
`AmDEVFirebaseAnalytics-Swift.h`, limits extraction to the wrapper headers, and selects
the installed iOS SDK with an arm64 Clang architecture. It does not invoke a project
build, resolve Swift packages, or search the producing machine's DerivedData cache.

To use a copied XCFramework, pass its path as the first argument. If its generated
header imports another framework, supply directories containing the matching iOS device
dependency frameworks as subsequent arguments. They become explicit Clang `-F` paths:

```bash
bash src/apple/AmDEVFirebaseAnalytics/extract_sharpie.sh \
    "/path/to/AmDEVFirebaseAnalytics.xcframework" \
    "/path/to/dependency.xcframework/ios-arm64"
```

These are framework search directories, not library binaries or simulator slices. Any
dependencies needed by imported headers must travel with the artifact to another machine.
The public wrapper API intentionally uses Foundation and wrapper-owned types; dependency
search paths are optional and are never inferred from an unrelated local package cache.

Generation occurs in a new temporary directory. Only successful generation with a
nonempty API-definition file replaces `sharpie-output/`; failures retain their diagnostic
log and preserve the previous output. Treat `sharpie-output/` as generated content: copy
definitions into the binding project before editing them, because a successful rerun
replaces the entire output directory. Do not run concurrent extractions in one checkout.

## Complete the .NET integration

Objective Sharpie creates a starting point, not a validated binding. Review generated
Objective-C names/selectors, nullability, enum widths, constructors, callback signatures,
and any `Verify` attributes before integrating the definitions into `BindingApple`.
Add the XCFramework as a `NativeReference` and complete the managed adapter separately.
The current .NET projects still target iOS; using the Catalyst slice also requires the
corresponding .NET target and a compatible managed dependency graph.

The XCFramework command packages the wrapper and its dSYMs, not every Firebase resource
or transitive dependency automatically. Inspect the linked binaries, Swift runtime needs,
privacy manifests, resource bundles, and any other Firebase usage in the host before
building the NuGet package. Avoid embedding duplicate static Firebase implementations.
Configure the host's Firebase settings and Crashlytics symbol uploads for its final app.

Compilation, Sharpie execution, symbol inspection, and device/Release integration tests
have not been performed in the Windows editing environment. They require an authorized
run on macOS/Xcode; the existing Firebase initializer still needs separate lifecycle work.

## References

- [Apple: creating an XCFramework](https://developer.apple.com/documentation/xcode/creating-a-multi-platform-binary-framework-bundle)
- [Microsoft: Objective Sharpie options](https://learn.microsoft.com/en-us/dotnet/maui/ios/objective-sharpie/tools?view=net-maui-10.0)
- [Firebase: using Firebase inside libraries](https://github.com/firebase/firebase-ios-sdk/blob/main/docs/firebase_in_libraries.md)
