# AMDevIT.Analytics

Provider-neutral analytics and crash reporting for .NET, with Firebase integrations and an optional `Microsoft.Extensions.Logging` bridge.

Register multiple providers behind a single `IAnalyticsInstance`, send structured events and exceptions, and keep vendor-specific initialization out of application code.

## Packages and implementation status

The repository currently declares version **0.0.6**. This is the source version, not a guarantee that every package is available on NuGet.org. The library is under development; platform support differs between packages.

| Package / project | Target frameworks | Purpose and current status |
| --- | --- | --- |
| `AMDevIT.Analytics.Abstractions` | `net10.0` | Event records and provider lifecycle, analytics, and crash-reporting contracts. |
| `AMDevIT.Analytics.Core` | `net10.0`, `net10.0-ios`, `net10.0-android` | Provider orchestration, dependency injection, and aggregated failures. |
| `AMDevIT.Analytics.Firebase.ManagedDroid` | `net10.0-android` | Implemented Firebase Analytics and Crashlytics sources, with combined or separate DI registrations. |
| `AMDevIT.Analytics.Firebase.BindingApple` | `net10.0-ios` | Low-level binding to the bundled native Apple wrapper. Binding compilation has been checked; native linking and app runtime integration still need validation. |
| `AMDevIT.Analytics.Firebase.ManagedApple` | `net10.0-ios` | **Incomplete.** Source methods still throw `NotImplementedException`; this is not a usable managed Firebase provider yet. |
| `AMDevIT.Analytics.Microsoft.Extensions.Logging` | `net10.0` | Optional queued `ILoggerProvider` that forwards selected logs and exceptions. |

The native Apple build scripts support iOS device, iOS Simulator, and Mac Catalyst archives. The .NET Apple projects currently target **iOS only**, not `net10.0-maccatalyst`. Do not infer managed Mac Catalyst support from the native wrapper.

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

These Firebase registration extensions currently belong to the **Android** provider. In a multitargeted host, keep their imports and registrations in Android-specific code.

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

The Swift wrapper exposes Firebase initialization, Analytics events, user properties, collection and consent controls, session information, and Crashlytics recording and report controls. The binding projects these native APIs into .NET; the managed adapter remains unfinished.

See the [Apple build guide](https://github.com/AMDevIT/AMDevITAnalytics/blob/Task-Apple-Library/src/apple/AmDEVFirebaseAnalytics/BUILDING.md) for XCFramework generation, Objective Sharpie extraction, prerequisites, and integration caveats. The checked-in Xcode project currently uses an iOS 26.5 deployment target; verify it against your supported devices before release.

Native dependency and resource packaging, privacy manifests, signing, initialization lifecycle, host symbol uploads, and device/Release validation remain release work. Successful binding compilation alone does not establish a working Firebase app integration.

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

Before publishing, restore and build the intended projects with their platform workloads, inspect the resulting `.nupkg` for the README, icon, and dependencies, and validate the host application. Do not publish `ManagedApple` as a functional provider while its implementation is incomplete. Updating these assets does not validate or publish a package.

## License

Licensed under [Apache-2.0](https://github.com/AMDevIT/AMDevITAnalytics/blob/Task-Apple-Library/LICENSE). Firebase and other third-party dependencies retain their own licenses and terms. This project is an independent integration, not an official Google, Firebase, or Microsoft SDK.
