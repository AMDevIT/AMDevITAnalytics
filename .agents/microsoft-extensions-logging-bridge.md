# Microsoft.Extensions.Logging bridge

## Objective

Provide an optional `ILoggerProvider` that routes selected Microsoft.Extensions.Logging entries through
`IAnalyticsInstance` without blocking application logging calls.

## Routing defaults

- The minimum accepted level is `Information`.
- Exceptions are routed to crash-reporting sources.
- Regular logs are not routed automatically.
- An `EventId.Name` beginning with `Analytics.` explicitly opts a log into analytics routing.
- A code-based `AnalyticsFilter` can opt additional entries into analytics routing.
- Categories beginning with `AMDevIT.Analytics` are excluded to prevent recursion.

## Queue and lifecycle

`ILogger.Log` formats the message and copies structured state and configured scopes synchronously. The resulting
immutable entry is offered to a bounded `Channel<AnalyticsLogEntry>` using `TryWrite`. When full, the newest entry is
dropped rather than blocking the caller. A single asynchronous worker dispatches entries to `IAnalyticsInstance`.

Synchronous and asynchronous disposal stop new writes, complete the channel, and attempt to flush it within the
configured timeout. Provider failures are retained in diagnostic counters and are never logged through `ILogger`.

## Configuration

`AnalyticsLoggingOptions` controls levels, routing, category exclusions, queue capacity, flush timeout, message and
parameter limits, built-in metadata, scope capture, the explicit event prefix, and an optional analytics predicate.
