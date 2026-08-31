# Apple native Firebase API

## Objective and status

On 2026-08-31, the user authorized adding the missing Analytics and Crashlytics APIs,
including all optional APIs listed in the preceding review. Implementation and manual
source review are complete; compilation, automated checks, and runtime tests are pending.

## Decisions

- Keep changes in the Apple wrapper and its documentation; do not expand .NET contracts.
- Preserve the existing event and log signatures and implement the empty Crashlytics log.
- Analytics now exposes user ID/properties, collection control, four-category consent,
  default parameters, local reset, session timeout, session ID, and app instance ID.
- AnalyticsConsentStatus is an Objective-C enum with unchanged, granted, and denied values.
  Unchanged categories are omitted from the Firebase update; no consent is granted implicitly.
- Session retrieval uses a callback with NSNumber/NSError so failure has no fabricated ID.
- Crashlytics exposes NSError recording with and without event metadata, managed exception
  recording, custom keys, user ID, collection state, previous-crash detection, and pending
  report check/send/delete operations.
- CrashlyticsStackFrame carries symbol/file/line data. The wrapper creates Firebase's
  ExceptionModel internally, keeping Firebase types out of the public Objective-C API.
- The caller must supply managed stack frames. No managed exception handler is installed.
- Initialization remains the host's responsibility. Manual report operations retain Firebase
  semantics and require automatic collection to be disabled.

## Affected files

Under src/apple/AmDEVFirebaseAnalytics/AmDEVFirebaseAnalytics/:

- AnalyticsManager.swift
- AnalyticsConsentStatus.swift (new)
- CrashlyticsManager.swift
- CrashlyticsStackFrame.swift (new)
- AmDEVFirebaseAnalytics.docc/AmDEVFirebaseAnalytics.md

Also updated .agents/context.md and added this context file.

## Checks and limitations

- Git fetch succeeded. Task-Apple-Library has no upstream; HEAD matched origin/main (0/0).
- The working tree was clean before this task.
- Consulted official Firebase Analytics, consent, Crashlytics, ExceptionModel, and StackFrame
  references. Manually inspected the resulting Swift sources and documentation examples.
- No build, restore, automated checks, or tests were run: explicit approval was not given,
  and this Windows host cannot compile the Apple framework with Xcode.
- No test files were changed. Existing template tests remain placeholders.

## Next step

With approval and a macOS/Xcode environment, compile against the resolved Firebase 12.18.0
package and inspect the generated Objective-C API. Exercise nullable values, partial consent,
session callbacks, managed stack frames, and pending-report flows in an initialized host app.
The earlier review issues remain outside this change: non-idempotent FirebaseCoreManager,
disabled SWIFT_INSTALL_OBJC_HEADER, SKIP_INSTALL=YES, iOS 26.5 minimum, missing Mac Catalyst
configuration, and unfinished .NET binding/packaging. Configure host dSYM uploads as well.

## Follow-up: build scripts (2026-08-31)

The later script-hardening step enables header export, archive installation, and Mac Catalyst,
and adds three-platform archives plus dSYM packaging. It supersedes the corresponding open
Xcode-setting issues above. See apple-build-scripts.md. The iOS minimum, initialization
lifecycle, .NET binding, and NuGet packaging still require separate work.
