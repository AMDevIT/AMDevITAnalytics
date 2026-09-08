# AMDev.IT Analytics: Apple privacy and resources

## Audit and release status

The initial audit on 2026-09-02 inspected the dependency graph at the revisions in
`AmDEVFirebaseAnalytics.xcodeproj/project.xcworkspace/xcshareddata/swiftpm/Package.resolved`.
Commit `80d0d8a` subsequently regenerated the checked-in XCFramework. Its three slices
now contain 54 privacy manifests in total (the wrapper plus 17 dependency bundles per
slice), 51 dependency bundles, and three wrapper dSYMs. This repository inspection does
not replace final app-archive privacy, signing, UUID, symbolication, or runtime validation.

Firebase packaging remains unchanged: Xcode builds the Swift Package dependencies into
the wrapper. This change preserves resources; it does not extract or relink Firebase
libraries, introduce a runtime NuGet, or promise coexistence with another Firebase copy.

## Resources required by the pinned graph

`required_resource_bundles.txt` lists SPM **target names**, not guessed full bundle names.
The Xcode resource phase accepts the generated package prefix and the normal sanitized
target spelling. It requires exactly one matching bundle for every target.

| Pinned package | Resource-bearing targets used by Analytics/Crashlytics |
| --- | --- |
| Firebase 12.18.0 | FirebaseCore, FirebaseCoreExtension, FirebaseCoreInternal, FirebaseCrashlytics, FirebaseInstallations |
| GoogleUtilities 8.1.3 | GoogleUtilities-AppDelegateSwizzler, -Environment, -Logger, -MethodSwizzler, -Network, -NSData, -Reachability, -UserDefaults |
| GoogleDataTransport 10.1.1 | GoogleDataTransport |
| Promises 2.4.1 | FBLPromises, Promises |
| nanopb 2.30910.1 | nanopb |

These 17 targets declare processed privacy-manifest resources. FirebaseSessions is a
Crashlytics dependency and brings in CoreExtension and Promises, but does not declare
a separate resource bundle in this pinned Package.swift. Other products in the Firebase
repository are not automatically dependencies of this wrapper.

The pinned FirebaseAnalytics, GoogleAppMeasurement, GoogleAppMeasurementIdentitySupport,
and GoogleAdsOnDeviceConversion SPM binary archives were inspected in memory: none
contained an external `PrivacyInfo.xcprivacy` or `.bundle` resource. Do not manufacture
replacement declarations for them. GoogleAdsOnDeviceConversion's package revision is
3.6.1 and references a 3.6.0 binary archive; it is conditional on iOS, not Catalyst.
Re-audit both source targets and binary archives whenever Package.resolved changes.

## Packaging

The Xcode target's `Preserve Firebase resource bundles` phase runs after Resources.
`copy_firebase_resources.sh` copies the required bundles from that build's
`BUILT_PRODUCTS_DIR` into `UNLOCALIZED_RESOURCES_FOLDER_PATH`, preserving their names
and contents. It never searches another project's cache. Xcode script sandboxing is
disabled only for the wrapper target because SPM's generated bundle names are resolved
at build time; review this phase as executable build code.

The wrapper's own `PrivacyInfo.xcprivacy` is included by the synchronized Xcode source
group. It declares no additional collection, tracking, or required-reason API use by
the forwarding layer itself. **Its empty arrays do not describe Firebase or the host
application.** Vendor declarations remain in the vendor bundles, unmodified.

`verify_firebase_resources.sh` requires the wrapper manifest and all 17 vendor bundles,
checks both flat iOS and versioned Catalyst framework layouts, and validates every plist
with `plutil`. Both the Xcode phase and `build_xcframework.sh` invoke it. Missing or
ambiguous bundles fail the build instead of silently producing an incomplete artifact.

These paths and lookup behavior must still be exercised with Xcode. If a new toolchain
changes SPM bundle layout, fix the explicit packaging rule; do not bypass the verifier.
Use a clean build when updating dependencies so obsolete bundles cannot survive.

## Final application checks

1. Rebuild the wrapper and replace the complete binding artifact, preserving Catalyst
   symlinks and dSYMs. Inspect the actual NuGet payload, not just the source tree.
2. Verify the embedded wrapper in the final iOS and Catalyst app still contains all
   manifests and resource bundles. Run the verifier on that embedded framework.
3. Generate and inspect Xcode's privacy report for the final app archive. Confirm the
   nested manifests appear and review any App Store validation diagnostics.
4. Review actual Analytics, Crashlytics, Sessions, custom-key, user-ID, and event usage.
   Collection and consent defaults must be configured before Firebase starts.
5. Supply accurate host privacy declarations and App Store disclosures; SDK manifests
   do not cover every optional feature or application-supplied value. Firebase describes
   these responsibilities in its [Apple data disclosure guide](https://firebase.google.com/docs/ios/app-store-data-collection).
6. Validate real reports on a test Firebase project, including release symbolication.
   Unit tests with fake backends do not establish resource lookup or upload behavior.

## Pinned source provenance

- [Firebase Package.swift, revision 346daa9](https://github.com/firebase/firebase-ios-sdk/blob/346daa9f46316aa372b35b317e18224acc2e9063/Package.swift)
- [GoogleUtilities Package.swift, revision 92c8f6d](https://github.com/google/GoogleUtilities/blob/92c8f6dc3ac375d6febdfcb3db68bc3d10633db3/Package.swift)
- [GoogleDataTransport Package.swift, revision ba3358d](https://github.com/google/GoogleDataTransport/blob/ba3358d3c3dbae8ef230b58a46b97ad65e84e974/Package.swift)
- [Promises Package.swift, revision f4a19a3](https://github.com/google/promises/blob/f4a19a3c313dc2616c70bb49d29a799fb16be837/Package.swift)
- [nanopb Package.swift, revision 3851d94](https://github.com/firebase/nanopb/blob/3851d94a41890dea16dc3db34caf60e585cb4163/Package.swift)
- [GoogleAppMeasurement Package.swift, revision f04760d](https://github.com/google/GoogleAppMeasurement/blob/f04760d460296cc0fa430935a7be212e5bd67fc5/Package.swift)
- [GoogleAdsOnDeviceConversion Package.swift, revision dc39082](https://github.com/googleads/google-ads-on-device-conversion-ios-sdk/blob/dc39082d8881109d35b94b1c122164c0e8d08a55/Package.swift)
- [Apple privacy-manifest documentation](https://developer.apple.com/documentation/bundleresources/adding-a-privacy-manifest-to-your-app-or-third-party-sdk)

The build uses the original dependency resources under their existing licenses.
No third-party manifest has been copied into this repository or rewritten as a claim
about all consumers of AMDev.IT Analytics.
