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
