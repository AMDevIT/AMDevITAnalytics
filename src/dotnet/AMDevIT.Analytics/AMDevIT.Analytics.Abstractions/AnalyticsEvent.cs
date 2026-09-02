namespace AMDevIT.Analytics.Abstractions;

/// <summary>Describes an analytics event and its optional metadata.</summary>
/// <param name="EventID">Identifier of the event.</param>
/// <param name="Message">Optional message associated with the event.</param>
/// <param name="Parameters">Optional named event parameters.</param>
public sealed record AnalyticsEvent(string EventID,
                                    string? Message = null,
                                    IReadOnlyDictionary<string, object?>? Parameters = null);
