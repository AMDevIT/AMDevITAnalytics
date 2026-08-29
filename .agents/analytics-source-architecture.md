# Analytics source architecture

## Decision

`AnalyticsInstance` is the provider-neutral orchestrator. It receives all analytics and crash sources either from
dependency injection or through its public constructor and fans operations out to every registered source.

The previous `IAnalyticsEventLogger`, `ICrashEventLogger`, and separate source initializer abstractions were removed.
Initialization is now part of `IAnalyticsSource`, which is inherited by both source contracts.

## Lifecycle

- DI registration is synchronous and only composes services.
- `IAnalyticsInstance.InitializeAsync` initializes all sources explicitly.
- Logging initializes each target source lazily as a fallback.
- Source initialization must be idempotent and thread-safe.
- Provider-specific static parameters belong in typed provider options or constructors.
- Mutable platform state belongs behind provider-specific accessor services.
- Asynchronously resolved configuration belongs behind provider-specific resolver services.

## Failure behavior

All registered sources are invoked even when one fails. Each provider failure is wrapped in an
`AnalyticsSourceOperationException`, and `AnalyticsInstance` reports all failures using an `AggregateException`.
Caller-requested cancellation remains an `OperationCanceledException` when no source has faulted.

## Firebase Android

Firebase Analytics initializes from the Android application context and does not retain an activity. Firebase
Crashlytics uses its process singleton. Both source registrations are available separately and through the combined
`AddFirebase` fluent registration.
