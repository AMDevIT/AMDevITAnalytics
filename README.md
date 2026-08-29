# AMDevITAnalytics

Provider-neutral analytics and crash reporting for .NET.

## Dependency injection

Register the core service and any number of provider sources:

```csharp
services.AddAMDevITAnalytics()
        .AddFirebase();
```

Providers can expose separate registrations when only part of their functionality is required:

```csharp
services.AddAMDevITAnalytics()
        .AddFirebaseAnalytics()
        .AddFirebaseCrashlytics();
```

Initialize all registered sources explicitly when the application lifecycle is ready:

```csharp
await analyticsInstance.InitializeAsync(cancellationToken);
```

Logging also initializes each source lazily, so explicit initialization is recommended but not required.
Failures from one source do not prevent the other registered sources from running; failures are reported together
as an `AggregateException` containing an `AnalyticsSourceOperationException` for each failed source.

## Manual construction

Sources can also be composed without dependency injection:

```csharp
FirebaseAnalyticsLoggerSource firebaseAnalytics = new();
FirebaseCrashEventLoggerSource firebaseCrashlytics = new();
AnalyticsInstance analyticsInstance = new(analyticsSources: [firebaseAnalytics],
                                           crashSources: [firebaseCrashlytics]);
```

Provider-specific configuration belongs to the provider registration or source constructor. Runtime dependencies,
such as a current platform activity, should be supplied through provider-specific accessor services rather than the
core initialization API.

## Microsoft.Extensions.Logging bridge

The optional logging provider forwards selected `ILogger` entries to the registered analytics sources:

```csharp
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

By default, exceptions are forwarded to crash-reporting sources. Regular log entries are forwarded to analytics only
when their `EventId.Name` starts with `Analytics.`:

```csharp
logger.LogInformation(new EventId(1001, "Analytics.checkout_completed"),
                      "Checkout completed for product {ProductID}",
                      productID);
```

The prefix is removed before dispatch, producing the analytics event name `checkout_completed`. Structured logging
properties, category, log level, numeric event ID, and optionally scopes are materialized as event parameters.

`ILogger.Log` remains synchronous and non-blocking. Entries are copied into a bounded in-memory queue and processed
asynchronously. New entries are dropped when the queue is full, while provider failures are isolated from the calling
application. Categories beginning with `AMDevIT.Analytics` are excluded by default to prevent logging recursion.
