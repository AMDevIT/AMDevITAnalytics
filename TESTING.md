# Testing AMDev.IT Analytics

The test projects received follow-up compilation fixes, but the repository does not store
a complete durable test report for every suite and platform. The commands below define the
run that must be recorded for a release candidate; commit messages alone are not test evidence.

## Suites

| Suite | Coverage | Runtime |
| --- | --- | --- |
| Swift Testing, AmDEVFirebaseAnalyticsTests | Events, nullable identity/defaults, consent conversion, collection/reset/session controls, Int64/error callbacks, native errors, managed exception/frame conversion, crash context/report controls, core delegation | Xcode, iOS Simulator and Mac Catalyst |
| AMDevIT.Analytics.Tests | Core routing, overloads, provider initialization/deduplication, parallel fan-out, aggregated failures/cancellation, DI; ILogger routing/filtering, snapshots/scopes, limits, queue overflow, failure isolation, flushing | .NET 10 desktop |
| AMDevIT.Analytics.Tests.Apple | Foundation scalar/items/null conversion, lazy lifecycle/concurrency/disposal, managed Analytics/Crashlytics controls and errors, managed stack traces, callbacks/cancellation/late results, DI | .NET 10 iOS or Mac Catalyst app |
| AMDevIT.Analytics.Tests.Android | Android Bundle scalar conversion and invalid values, provider DI registration | .NET 10 Android app |

Unit tests use local capture backends/managers and do not configure Firebase or send
telemetry. The Swift exception tests instantiate Firebase exception/frame value objects,
but do not call the Crashlytics singleton. Internal injection constructors are not
public API and do not change production startup ownership. Managed spies implement
internal interfaces instead of subclassing the Swift managers, whose generated
Objective-C headers prohibit subclassing.

The platform test apps use MSTest assertions and an explicit small runner to execute
parameterless `[TestMethod]` methods. They are executable apps, not desktop VSTest
assemblies; `dotnet test` alone cannot provide Foundation or the Android runtime.
The runner preserves reflected test methods for trimming, continues after failures,
prints individual PASS/FAIL entries, and writes `test-results.txt`. Success requires
a nonzero TOTAL and `FAILED 0`; a launched app alone is not a passing result.
The report is in Documents on Apple and the app's private files directory on Android.
The report is also displayed in the app and printed to the console.

## Desktop .NET tests

From the repository root, after approval to restore/build:

```powershell
dotnet test src/dotnet/AMDevIT.Analytics/Tests/AMDevIT.Analytics.Tests/AMDevIT.Analytics.Tests.csproj -c Release --logger trx
```

This project targets only `net10.0`. Its project references select Core's plain .NET
target; native tests are separate. An installed Apple/Android workload may still be
required when restoring the complete multitargeted solution.

## Swift tests

On macOS with Xcode selected, the shared `AmDEVFirebaseAnalytics` scheme includes the
test target. Choose an installed Simulator identifier from `xcrun simctl list devices`.
The Swift test target uses a minimum platform version of 17.0 for Swift Testing;
the shipping wrapper retains its independent 15.6 minimum.

```bash
xcodebuild test \
    -project src/apple/AmDEVFirebaseAnalytics/AmDEVFirebaseAnalytics.xcodeproj \
    -scheme AmDEVFirebaseAnalytics \
    -destination 'platform=iOS Simulator,id=YOUR_SIMULATOR_UDID' \
    -resultBundlePath /tmp/AMDevITAnalytics-iOS-tests.xcresult \
    CODE_SIGNING_ALLOWED=NO

xcodebuild test \
    -project src/apple/AmDEVFirebaseAnalytics/AmDEVFirebaseAnalytics.xcodeproj \
    -scheme AmDEVFirebaseAnalytics \
    -destination 'platform=macOS,variant=Mac Catalyst' \
    -resultBundlePath /tmp/AMDevITAnalytics-Catalyst-tests.xcresult
```

Use fresh result paths for each run. Configure your own signing team if Xcode requires
one for the chosen destination. The resource phase also runs during these builds.
Do not remove its checks to make tests pass.

## .NET platform tests

First regenerate the wrapper as described in the [Apple build guide](src/apple/AmDEVFirebaseAnalytics/BUILDING.md)
and replace the entire binding XCFramework. Use a Mac checkout that preserves symlinks.

Open the solution and run `AMDevIT.Analytics.Tests.Apple` with `net10.0-ios` on a
Simulator, then with `net10.0-maccatalyst` on the Mac. Set the appropriate runtime
identifier and signing configuration for your machine. Record the full test report.
No GoogleService-Info.plist is required for these fake-manager tests.

Run `AMDevIT.Analytics.Tests.Android` on an emulator/device with the Android workload.
Include at least one supported device below API 33 and one API 33 or newer when validating
the application scenario discussed in the README. No google-services configuration
is required for the Bundle/DI tests.

Build and run both Debug and Release app configurations before release, including
trimming/AOT settings matching the consumer. Do not use the platform test runner as
a production crash handler.

## Real Firebase integration gate

These checks require a separate test Firebase project and explicit telemetry consent.
They are not part of the credential-free unit suites.

- Supply the correct host Firebase configuration and initialize before background calls.
  Check managed repeated initialization and the explicit existing-app adoption path
  separately, in fresh app processes.
- Send an analytics event and a caught managed exception through IAnalyticsInstance.
  Verify the event name, values, exception name/reason, and available stack frames.
- Exercise the README UnobservedTaskException handler in a disposable test harness.
  Event timing is nondeterministic; do not assert that every discarded task produces
  a report. Do not force garbage collection in production code.
- Test collection disabled from first launch, consent changes, pending-report check,
  send and delete, native error callbacks, cancellation, and disposal during pending
  callbacks. Cancellation stops the managed wait, not Firebase's operation.
- Check on-device Release symbolication and the final app's resources/privacy report,
  following [PRIVACY.md](src/apple/AmDEVFirebaseAnalytics/PRIVACY.md).
- Test native crash capture only in a dedicated test app, detached from the debugger,
  and verify delivery on the next launch. It differs from recording a non-fatal exception.
- If the consumer embeds another Firebase integration, validate that exact app separately.
  These tests do not establish compatibility between independently linked Firebase copies.
