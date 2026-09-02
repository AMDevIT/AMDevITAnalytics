# ``AmDEVFirebaseAnalytics``

The AMDev.IT Analytics Objective-C-compatible wrapper for Firebase Analytics and Crashlytics on iOS and Mac Catalyst.

## Overview

The public API uses Foundation types, an integer-backed consent enum, and callback blocks so
that a .NET binding does not need to expose Firebase's Swift types.

The host application must supply its `GoogleService-Info.plist` and configure Firebase on
the main thread before calling either manager. The current ``FirebaseCoreManager`` must
be called only once, and must not be called if the host already configured Firebase.

## Analytics

``AnalyticsManager`` supports events, user identifiers and properties, collection control,
consent, default event parameters, local data reset, session timeout, session identifier
retrieval, and app instance identifier retrieval.

```swift
let analytics = AnalyticsManager()
analytics.setUserID("user-123")
analytics.setUserProperty("premium", forName: "account_tier")
analytics.setConsent(analyticsStorage: .granted,
                     adStorage: .denied,
                     adUserData: .denied,
                     adPersonalization: .denied)
analytics.logEvent(name: "checkout_started", parameters: ["item_count": 2])
```

Consent arguments set to ``AnalyticsConsentStatus/unchanged`` leave that category untouched.
Swift defaults do not create Objective-C overloads: binding callers must pass all four
consent values explicitly. Configure the host's default collection and consent settings
before Firebase starts; these examples are not a consent policy.

Pass `nil` to clear a user identifier or property. For default event parameters, `nil`
clears all defaults and `NSNull()` removes an individual key. Event parameters override
defaults with the same name. Firebase's restrictions on event names and values still apply.

`sessionID(completion:)` returns an `NSNumber` containing an Int64 identifier on success,
or `nil` plus an `NSError` on failure. Its callback is not guaranteed to run on the main
thread. `appInstanceID()` can return `nil`. Session timeout values are in seconds.
`resetAnalyticsData()` clears local data; it does not delete previously uploaded events.

## Crashlytics

``CrashlyticsManager`` supports diagnostic logs, native errors with optional event metadata,
managed exception stack traces, custom keys, user identifiers, collection settings,
previous-crash detection, and checking, sending, or deleting pending reports.

```swift
let crashlytics = CrashlyticsManager()
let frame = CrashlyticsStackFrame(symbol: "CheckoutService.Submit",
                                  file: "CheckoutService.cs",
                                  line: 42)
crashlytics.log(message: "Submitting checkout")
crashlytics.recordException(name: "System.InvalidOperationException",
                            reason: "Checkout is not ready.",
                            stackTrace: [frame])
```

The managed caller must extract and supply stack frames, starting at the throw site.
Use an empty file name and line zero when source information is unavailable. Supplying an
empty array is also supported. This method records a non-fatal exception; the wrapper does
not install a managed unhandled-exception handler.

`log(message:)` only adds context to reports. Custom keys apply to subsequent reports;
use `record(error:userInfo:)` for metadata that belongs only to one native error.

For manual reporting, disable automatic collection in the host's `Info.plist` using
`FirebaseCrashlyticsCollectionEnabled` before Firebase starts. Runtime collection changes
follow Firebase's persisted-setting behavior and are not an immediate deletion mechanism.
Call `checkForUnsentReports(completion:)` once per launch with automatic collection disabled,
then call `sendUnsentReports()` or `deleteUnsentReports()` according to the host's decision.
Callbacks use Firebase's queue and timing. Sending queues an upload; it does not confirm
delivery. Deleting affects pending local reports, not reports already uploaded.

## Integration status

The Xcode project exports the Objective-C header and enables Mac Catalyst. The build script
archives iOS device, simulator, and Catalyst variants and packages the wrapper dSYMs.
The wrapper deployment target is 15.6. See `BUILDING.md` beside the Xcode project for
build and Objective Sharpie instructions, prerequisites, and output locations.

The .NET binding and managed adapter target iOS and Mac Catalyst. Internal capture backends
support Swift tests without configuring Firebase; they are not part of the Objective-C API.
The Xcode resource phase preserves dependency bundles and privacy manifests, and archive
checks reject missing resources. See `PRIVACY.md` for the pinned dependency audit.

The previously committed XCFramework still needs regeneration with these resources and
dSYMs. The new tests, build phase, native linking, signing, and host Crashlytics symbol
uploads need an authorized macOS validation run. The wrapper does not install managed
exception handlers; the root README shows the common .NET UnobservedTaskException pattern.

## References

- [Firebase Analytics API](https://firebase.google.com/docs/reference/swift/firebaseanalytics/api/reference/Classes/Analytics)
- [Firebase consent API](https://firebase.google.com/docs/reference/swift/firebaseanalytics/api/reference/Categories/FIRAnalytics%28Consent%29)
- [Firebase Crashlytics API](https://firebase.google.com/docs/reference/swift/firebasecrashlytics/api/reference/Classes/Crashlytics)
- [Firebase exception models](https://firebase.google.com/docs/reference/swift/firebasecrashlytics/api/reference/Classes/ExceptionModel)

## Topics

### Managers

- ``FirebaseCoreManager``
- ``AnalyticsManager``
- ``CrashlyticsManager``

### Binding types

- ``AnalyticsConsentStatus``
- ``CrashlyticsStackFrame``
