# Progressive context

## Objective and status

Implement the provider-source architecture and the optional Microsoft.Extensions.Logging bridge agreed with the
user. The logging bridge implementation is complete pending restore and build verification, for which repository
instructions require separate user approval.

## Decisions made

- `AnalyticsInstance` owns and fans out to all registered source instances.
- Analytics and crash sources remain separate contracts and share `IAnalyticsSource` lifecycle behavior.
- Source initialization is explicit through `IAnalyticsInstance.InitializeAsync` and lazy during logging.
- DI uses `AddAMDevITAnalytics` and a fluent `AnalyticsBuilder`.
- Firebase offers combined and separate analytics/Crashlytics registrations.
- Provider failures are isolated, identified, and aggregated.
- Provider-specific parameters do not leak into the Core initialization contract.
- `ILogger.Log` never performs asynchronous provider work or blocks on a full queue.
- Exceptions are routed to crash reporting; regular logs require explicit configuration or an `Analytics.` event.
- Internal analytics categories are excluded to prevent recursion.

## Affected files

- `README.md`
- `.agents/analytics-source-architecture.md`
- `.agents/context.md`
- `src/dotnet/AMDevIT.Analytics/AMDevIT.Analytics.Abstractions/*`
- `src/dotnet/AMDevIT.Analytics/AMDevIT.Analytics.Core/*`
- `src/dotnet/AMDevIT.Analytics/AMDevIT.Analytics.Firebase.ManagedDroid/*`
- `src/dotnet/AMDevIT.Analytics/AMDevIT.Analytics.Microsoft.Extensions.Logging/*`
- `.agents/microsoft-extensions-logging-bridge.md`

## Checks performed

- Fetched remote Git references; `main` was aligned with `origin/main` before editing.
- Inspected the local Xamarin Firebase binding packages and Android reference documentation.
- Searched for obsolete initializer and logger references.
- Ran `git diff --check`; no whitespace errors were reported.
- Reviewed the logging bridge for synchronous-state materialization, bounded queue behavior, recursion, and disposal.
- Restore and build have not been run because approval is still required.

## Open issues and recommended next step

Obtain approval to run restore and build. Resolve any compiler diagnostics, then add tests covering logger routing,
structured state materialization, queue saturation, disposal, multiple-source fan-out, cancellation, and aggregated
failures.
