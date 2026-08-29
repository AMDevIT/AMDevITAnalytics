namespace AMDevIT.Analytics.Abstractions;

public sealed record CrashEvent(Exception Exception,
                                string EventID,
                                string? Message = null,
                                IReadOnlyDictionary<string, object?>? Parameters = null);
