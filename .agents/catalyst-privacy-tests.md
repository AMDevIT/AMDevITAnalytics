# Catalyst, privacy resources, documentation, and tests (2026-09-02)

## Objective and status

Implemented the confirmed scope in source: net10.0-maccatalyst, project branding,
README Crashlytics usage including TaskScheduler.UnobservedTaskException, release/build
documentation, privacy/resource packaging, and Swift/.NET test suites.

No restore, build, pack, Sharpie run, platform test execution, publication, or binary
regeneration was performed. The user's prohibition on restore/build remains active.

## Decisions

- Project display name: AMDev.IT Analytics. GitHub repository: AMDevITAnalytics.
  Package IDs and namespaces stay AMDevIT.Analytics.*; version remains 0.0.6.
- Core adds Catalyst; BindingApple and ManagedApple multitarget iOS/Catalyst with
  SupportedOSPlatformVersion 15.6. Xcode wrapper/project defaults align to 15.6.
  The Swift test target uses 17.0 independently for Swift Testing.
- Firebase remains included through the existing Xcode/SPM wrapper. No optional runtime
  NuGet, binary aggregation, separate SDK linking, or coexistence promise was introduced.
- Audited pinned Package.swift dependency edges and the four relevant Google binary ZIPs.
  Seventeen source targets declare privacy-resource bundles. The four binary archives
  contain no external privacy manifests/bundles. Full provenance is in PRIVACY.md.
- Xcode copies those generated bundles into the wrapper before packaging. Shell checks
  validate the wrapper/vendor manifests in both iOS and versioned Catalyst layouts and
  fail for missing/ambiguous bundles. Original dependency resources are not rewritten.
- The wrapper manifest describes only the forwarding layer, not Firebase or host privacy
  behavior. Final app privacy/disclosure review remains mandatory.
- Swift closures allow capturing SDK calls while preserving Objective-C signatures.
  Managed test seams use interfaces: the generated native headers have
  objc_subclassing_restricted, so fake binding subclasses would be invalid.
- FirebaseAppleInitialization preserves the synchronized once-only startup/adoption
  policy while permitting isolated tests of main-thread checks and failed retries.
- Apple cancellation now completes the original callback TCS as canceled, preventing a
  late native error from faulting an abandoned task.
- ILogger DisposeAsync now limits its extra post-cancellation wait to 250 ms, matching
  synchronous disposal, rather than hanging indefinitely on an uncooperative source.
- README reports unobserved tasks via the common crash API on the thread pool, catches
  reporting failures, and leaves SetObserved and the original API-33 policy to the host.
- Added 43 MSTest methods across desktop/Core/logging (22), Apple (18), and Android (3);
  added 10 Swift test methods (one parameterized over two report states).
  Platform tests use real Foundation/Android runtimes and fake SDK-facing managers.
  A shared Xcode scheme and two .NET executable test hosts make them runnable.

## Affected areas

- Root README.md, RELEASING.md, TESTING.md, .gitignore, assets/icons/README.md.
- Apple managers/backends, PrivacyInfo.xcprivacy, Xcode project/shared scheme, Swift tests.
- Apple copy/verify resource scripts, required-resource list, build_xcframework.sh,
  BUILDING.md, PRIVACY.md, DocC overview.
- Core/BindingApple/ManagedApple TFMs, ManagedApple internal adapters/startup/callbacks,
  ManagedDroid friend-assembly declaration, ILogger disposal.
- Three .NET projects under Tests, shared platform runner, solution entries.

## Checks and results

- Git fetch succeeded; branch Task-Apple-Library matched upstream (0 ahead / 0 behind);
  working tree was clean before this step. No pull or commit.
- Reviewed exact pinned upstream package manifests and ZIP entry listings in memory;
  network reads did not restore packages or build anything.
- Reviewed binding signatures, objc_subclassing_restricted in the native header, and
  installed Foundation/MSTest reference metadata.
- C# syntax parsed with PowerShell's loaded Roslyn 5.0 parser (no compilation/emit).
  Loading SDK Roslyn directly initially encountered an assembly-version conflict;
  the already-loaded parser was then used explicitly and passed.
- Project/solution/manifest/scheme XML parsed successfully. Bash -n passed independently
  for build_xcframework.sh, copy_firebase_resources.sh, verify_firebase_resources.sh.
- Reviewed README/DocC multiline invocation alignment and public API preservation.
- git diff --check passed; CRLF conversion notices are Git configuration warnings,
  not whitespace failures.

## Open issues and next step

Update on 2026-09-03: commit `80d0d8a` regenerated the checked-in XCFramework. Repository
inspection now finds the expected three slices, 54 privacy manifests, 51 dependency bundles,
and three dSYMs. The original limitation below still applies to execution evidence: validate
the actual generated SPM bundle paths, Swift tests, binding/managed builds, platform test
reports, and final Release app privacy/symbolication/upload behavior for the release candidate.

The added tests are source-reviewed, not proven passing; there is no claim of complete
Firebase SDK coverage or compatibility between independently embedded Firebase copies.
Follow TESTING.md and RELEASING.md for the authorized validation/release run.
