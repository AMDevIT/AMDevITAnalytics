# AMDev.IT Analytics

Provider-neutral analytics and crash reporting for .NET, with Firebase integrations and an optional `Microsoft.Extensions.Logging` bridge.

Register multiple providers behind a single `IAnalyticsInstance`, send structured events and exceptions, and keep vendor-specific initialization out of application code.

## Packages and implementation status

The repository currently declares version **0.0.6**. This is the source version, not a guarantee that every package is available on NuGet.org. The library is under development; platform support differs between packages.

| Package / project | Target frameworks | Purpose and current status |
| --- | --- | --- |
| `AMDevIT.Analytics.Abstractions` | `net10.0` | Event records and provider lifecycle, analytics, and crash-reporting contracts. |
| `AMDevIT.Analytics.Core` | `net10.0`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-android` | Provider orchestration, dependency injection, and aggregated failures. |
| `AMDevIT.Analytics.Firebase.ManagedDroid` | `net10.0-android` | Implemented Firebase Analytics and Crashlytics sources, with combined or separate DI registrations. |
| `AMDevIT.Analytics.Firebase.BindingApple` | `net10.0-ios`, `net10.0-maccatalyst` | Low-level binding to the bundled native Apple wrapper. The new targets and native packaging still require build and host validation. |
| `AMDevIT.Analytics.Firebase.ManagedApple` | `net10.0-ios`, `net10.0-maccatalyst` | Managed Analytics and Crashlytics sources over the Apple binding. Device and Catalyst runtime validation are still required. |
| `AMDevIT.Analytics.Microsoft.Extensions.Logging` | `net10.0` | Optional queued `ILoggerProvider` that forwards selected logs and exceptions. |

The Apple projects declare iOS and Mac Catalyst support with a minimum platform version of 15.6. The Xcode build produces device, Simulator, and Catalyst slices. These declarations do not replace testing the final application. **AMDev.IT Analytics** is the project name; **AMDevITAnalytics** is the GitHub repository name. Existing package IDs and namespaces remain `AMDevIT.Analytics.*`.

## Getting started on Android

Use .NET 10 and the Android workload. Add `AMDevIT.Analytics.Firebase.ManagedDroid` to the Android application; it references Core and Abstractions transitively. Add `AMDevIT.Analytics.Microsoft.Extensions.Logging` only if the logging bridge is needed.

For versions available in your configured package feed:

```powershell
dotnet add package AMDevIT.Analytics.Firebase.ManagedDroid
dotnet add package AMDevIT.Analytics.Microsoft.Extensions.Logging
```

When working with unpublished changes, use project references to the corresponding projects under `src/dotnet/AMDevIT.Analytics` instead.

Configure Firebase in the host Android app before initializing the sources, including the app-specific Firebase configuration and any required Crashlytics setup. The library does not create a Firebase project, supply credentials, or configure symbol uploads. Its Android dependencies are `Xamarin.Firebase.Analytics` and `Xamarin.Firebase.Crashlytics`.

### Register the sources

In your host's service-registration code, where `services` is its `IServiceCollection`:

```csharp
using AMDevIT.Analytics.Core.Extensions;
using AMDevIT.Analytics.Firebase.ManagedDroid.Extensions;

services.AddAMDevITAnalytics()
        .AddFirebase();
```

To register Analytics or Crashlytics independently, choose the corresponding extension. This example registers both separately:

```csharp
services.AddAMDevITAnalytics()
        .AddFirebaseAnalytics()
        .AddFirebaseCrashlytics();
```

Both managed Firebase providers expose these registration extensions. In a multitargeted host, select the Android or Apple namespace with platform-specific imports; do not import both at once.

### Initialize and record events

Resolve `IAnalyticsInstance` from your host's service provider, or inject it into application services. The following example assumes `serviceProvider` and `cancellationToken` are supplied by the host:

```csharp
using AMDevIT.Analytics.Core;
using Microsoft.Extensions.DependencyInjection;

IAnalyticsInstance analytics = serviceProvider.GetRequiredService<IAnalyticsInstance>();
Dictionary<string, object?> parameters = new()
{
    ["product_id"] = "premium_monthly",
    ["quantity"] = 1
};

await analytics.InitializeAsync(cancellationToken);

await analytics.LogEventAsync("checkout_completed",
                              message: "Checkout completed",
                              parameters: parameters,
                              cancellationToken: cancellationToken);
```

For a caught exception, pass it explicitly to crash reporting:

```csharp
await analytics.LogErrorAsync(exception,
                              eventID: "checkout_failed",
                              message: "Checkout could not be completed",
                              cancellationToken: cancellationToken);
```

The overloads also accept `AnalyticsEvent` and `CrashEvent` records from `AMDevIT.Analytics.Abstractions`. Use event names and parameter types accepted by the destination provider. On Android, an event's optional message is added as the `message` parameter unless that key already exists.

### Crashlytics: unobserved task exceptions

Use `IAnalyticsInstance.LogErrorAsync` instead of calling `FirebaseCrashlytics.Instance.RecordException` directly. Register a Crashlytics source with `AddFirebase()` or `AddFirebaseCrashlytics()` first. Without a crash source, the common API has nowhere to send the exception.

After Firebase and the analytics sources have been initialized at application startup, register this handler **once** for the application's lifetime. On iOS and Mac Catalyst, call `FirebaseApple.Initialize()` on the main thread before this code, as shown in [Apple integration](#apple-integration).

```csharp
using AMDevIT.Analytics.Core;
using Microsoft.Extensions.DependencyInjection;

IAnalyticsInstance analytics = serviceProvider.GetRequiredService<IAnalyticsInstance>();
await analytics.InitializeAsync();

EventHandler<UnobservedTaskExceptionEventArgs> onUnobservedTaskException = (sender, args) =>
{
    Exception exception = args.Exception;

    // Keep provider work off the thread raising this event.
    _ = Task.Run(() => ReportUnobservedAsync(analytics, exception));
};

TaskScheduler.UnobservedTaskException += onUnobservedTaskException;

static async Task ReportUnobservedAsync(IAnalyticsInstance analytics, Exception exception)
{
    try
    {
        await analytics.LogErrorAsync(exception,
                                      eventID: "unobserved_task_exception",
                                      message: "An unobserved task failed.").ConfigureAwait(false);
    }
    catch (Exception)
    {
        // Best effort: never create another unobserved failure or report recursively.
        // If needed, write to an independent local diagnostic sink here.
    }
}
```

The same handler works with the Android and Apple providers; it needs no direct Java exception conversion. The API-33 guard in an application's existing handler is an application-specific restriction, not a requirement introduced by this library. If your application still needs that restriction, keep its Android guard before scheduling the report.

This records a **non-fatal managed exception**. `UnobservedTaskException` is not a handler for every application crash: its timing depends on task collection, and termination may prevent the event or upload. Prefer catching and awaiting errors at their original call site. This example deliberately does not call `args.SetObserved()`; observation is an application policy. Unsubscribe using the same `onUnobservedTaskException` delegate before disposing the application's analytics services, and account for any reports already in flight. Do not send secrets or personal data in exception messages or parameters.

## Lifecycle and failures

- DI registration composes services synchronously; it does not initialize Firebase.
- `InitializeAsync` initializes all registered sources. Logging also initializes each target source lazily.
- Analytics events go to all analytics sources; exceptions go to all crash-reporting sources. With no matching sources, dispatch completes without sending anything.
- Failure in one provider does not stop dispatch to the others. Failures are returned together in an `AggregateException`, with an `AnalyticsSourceOperationException` identifying each failed provider and operation.
- Caller cancellation is propagated as cancellation when no provider failure takes precedence.

Handle failures at your application's chosen boundary. Direct calls to `IAnalyticsInstance` can throw; the optional logging bridge isolates asynchronous dispatch failures from `ILogger` callers.

### Manual construction

DI is optional. For Android, create and own the provider sources explicitly:

```csharp
using AMDevIT.Analytics.Core;
using AMDevIT.Analytics.Firebase.ManagedDroid;

using FirebaseAnalyticsLoggerSource firebaseAnalytics = new();
using FirebaseCrashEventLoggerSource firebaseCrashlytics = new();
AnalyticsInstance analytics = new(analyticsSources: [firebaseAnalytics],
                                  crashSources: [firebaseCrashlytics]);

await analytics.InitializeAsync();
await analytics.LogEventAsync("app_opened");
```

Keep the sources alive for the application's required lifetime. `AnalyticsInstance` does not dispose manually supplied sources.

## Microsoft.Extensions.Logging bridge

Use the host's normal logging registration and add the optional provider while composing analytics services:

```csharp
using AMDevIT.Analytics.Core.Extensions;
using AMDevIT.Analytics.Firebase.ManagedDroid.Extensions;
using AMDevIT.Analytics.Microsoft.Extensions.Logging.Extensions;
using Microsoft.Extensions.Logging;

services.AddAMDevITAnalytics()
        .AddFirebase()
        .AddMicrosoftLogging(options =>
        {
            options.MinimumLevel = LogLevel.Information;
            options.SendExceptionsToCrashReporting = true;
            options.SendRegularLogsToAnalytics = false;
            options.QueueCapacity = 256;
        });
```

By default, accepted entries with exceptions go to crash reporting. Regular entries opt into analytics through an `EventId.Name` beginning with `Analytics.`:

```csharp
logger.LogInformation(new EventId(1001, "Analytics.checkout_completed"),
                      "Checkout completed for product {ProductID}",
                      productID);
```

The provider removes the prefix to produce `checkout_completed`. It captures structured properties and metadata, with optional scope capture. Host logging filters and the provider's minimum level still apply.

`ILogger.Log` formats and copies the entry synchronously, then offers it to a bounded in-memory queue without waiting for provider dispatch or queue space. A worker sends accepted entries asynchronously. New entries are dropped when the queue is full. This is best-effort delivery, not durable storage.

| Option | Default |
| --- | --- |
| `MinimumLevel` | `Information` |
| `SendExceptionsToCrashReporting` | `true` |
| `SendRegularLogsToAnalytics` | `false` |
| `AnalyticsEventNamePrefix` | `Analytics.` |
| `QueueCapacity` | `256` |
| `FlushTimeout` | 2 seconds |
| `MaximumMessageLength` | `1024` |
| `MaximumParameterCount` | `20` |
| `IncludeScopes` | `false` |

`AnalyticsFilter` can select additional entries. Categories beginning with `AMDevIT.Analytics` are excluded by default to prevent recursion. Disposal attempts to drain the queue within `FlushTimeout`; provider failures are tracked without logging them back through `ILogger`.

## Adding another provider

Implement `IAnalyticsLoggerSource`, `ICrashEventLoggerSource`, or both. Their shared `IAnalyticsSource` contract exposes `InstanceID`, `IsInitialized`, and `InitializeAsync`.

Initialization must be idempotent and safe for concurrent callers. Register implementations under their source interfaces in the service collection, then call `AddAMDevITAnalytics`. Multiple registered implementations participate in dispatch.

Keep vendor configuration in provider-specific options or constructors. Supply mutable platform state through provider-specific accessors and asynchronous configuration through resolver services, rather than extending the Core initialization contract.

## Apple integration

The Swift wrapper exposes Firebase initialization, Analytics events, user properties, collection and consent controls, session information, and Crashlytics recording and report controls. The binding projects these native APIs into .NET, and `Firebase.ManagedApple` provides managed sources and dependency-injection extensions.

Configure the default Firebase app once on the main thread during application startup, before any source can initialize on a background thread:

```csharp
using AMDevIT.Analytics.Core.Extensions;
using AMDevIT.Analytics.Firebase.ManagedApple;
using AMDevIT.Analytics.Firebase.ManagedApple.Extensions;

FirebaseApple.Initialize();

services.AddAMDevITAnalytics()
        .AddFirebase();
```

`FirebaseApple.Initialize()` reads `GoogleService-Info.plist` through the native Firebase SDK. If the host or another integration has already configured the default app, call `FirebaseApple.Initialize(useExistingApp: true)` instead. The managed binding cannot discover that external state, so choosing the correct mode remains the host's responsibility.

`FirebaseAnalyticsLoggerSource` accepts strings, finite numbers, booleans, and Firebase's `items` collection. `FirebaseCrashEventLoggerSource` converts managed stack traces to the native exception model and exposes collection and pending-report controls. Crash custom keys are persistent Firebase context and can therefore affect later reports.

Add `AMDevIT.Analytics.Firebase.ManagedApple` to the iOS and Mac Catalyst targets of the host. Include the host's `GoogleService-Info.plist` as a bundle resource. Firebase remains compiled into the native wrapper through the Xcode project's Swift Package dependencies; there is no separate optional Firebase runtime NuGet package. Do not assume compatibility with another independently embedded Firebase copy.

See the [Apple build guide](src/apple/AmDEVFirebaseAnalytics/BUILDING.md) and [privacy/resource audit](src/apple/AmDEVFirebaseAnalytics/PRIVACY.md). The checked-in XCFramework has been regenerated for iOS, iOS Simulator, and Mac Catalyst; every slice contains the wrapper privacy manifest, 17 pinned dependency resource bundles, and its dSYM. The Xcode resource phase and archive checks reject missing manifests. Final host-app, privacy-report, signing, and symbolication validation is still required before release.

Signing, host privacy declarations and consent, initialization ownership, Crashlytics symbol uploads, and device/Release validation remain host/release responsibilities. Successful binding compilation alone does not establish a working Firebase app integration.

## Tests

The repository includes Swift tests for the native wrapper, MSTest tests for Core and the logging bridge, and .NET test apps for Foundation and Android runtime behavior. The unit suites do not require Firebase credentials or send telemetry. See [TESTING.md](TESTING.md) for coverage, commands, and the separate real-Firebase integration checklist. Test source and project fixes are committed, but no durable test-result artifact is stored in the repository; record a fresh complete run for the release candidate.

## Repository layout

```text
assets/icons/                         Shared package icon and resolution variants
src/dotnet/AMDevIT.Analytics/           .NET solution and package projects
src/apple/AmDEVFirebaseAnalytics/       Swift wrapper, Xcode project, and build scripts
LICENSE                               Apache-2.0 license
```

The Android implementation currently lives in the `Firebase.ManagedDroid` .NET project; there is no separate native Android source tree in this checkout.

## NuGet assets and release preparation

All package projects share the root README and `assets/icons/nuget_icon_128.png` through `Directory.Build.props`. Both files are included at the package root, matching `PackageReadmeFile` and `PackageIcon`. The 256- and 512-pixel PNG variants are available for other presentation sizes; the original generated artwork is also retained under `assets/icons`.

NuGet supports PNG/JPEG package icons up to 1 MB and recommends 128 × 128 pixels. See the [NuGet icon and README reference](https://learn.microsoft.com/en-us/nuget/reference/nuspec#icon). Asset details and generation provenance are documented in `assets/icons/README.md`.

Before publishing, restore and build the intended projects with their platform workloads, inspect the resulting `.nupkg` for the README, icon, binding, and native dependencies, and validate the host application on a device. Updating these assets does not validate or publish a package.

Use [RELEASING.md](https://github.com/AMDevIT/AMDevITAnalytics/blob/Task-Apple-Library/RELEASING.md) for the six-package release checklist, native resources, symbols, and platform validation.

## License

Licensed under [Apache-2.0](https://github.com/AMDevIT/AMDevITAnalytics/blob/Task-Apple-Library/LICENSE). Firebase and other third-party dependencies retain their own licenses and terms. This project is an independent integration, not an official Google, Firebase, or Microsoft SDK.
