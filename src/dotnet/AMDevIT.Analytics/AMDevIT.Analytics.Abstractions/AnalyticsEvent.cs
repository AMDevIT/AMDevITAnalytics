namespace AMDevIT.Analytics.Abstractions;

public sealed record AnalyticsEvent(string EventID,
                                    string? Message = null,
                                    IReadOnlyDictionary<string, object?>? Parameters = null);
