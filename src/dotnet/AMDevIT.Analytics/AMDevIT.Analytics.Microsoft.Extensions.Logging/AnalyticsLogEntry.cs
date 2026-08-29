namespace AMDevIT.Analytics.Microsoft.Extensions.Logging;

internal sealed record AnalyticsLogEntry(string EventID,
                                         string? Message,
                                         IReadOnlyDictionary<string, object?> Parameters,
                                         Exception? Exception,
                                         bool SendToAnalytics,
                                         bool SendToCrashReporting);
