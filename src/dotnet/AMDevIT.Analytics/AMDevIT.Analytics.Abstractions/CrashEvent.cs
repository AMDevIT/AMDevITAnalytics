namespace AMDevIT.Analytics.Abstractions;

/// <summary>Describes an exception to report with an event identifier and optional metadata.</summary>
/// <param name="Exception">Exception to report.</param>
/// <param name="EventID">Identifier of the crash event.</param>
/// <param name="Message">Optional message associated with the exception.</param>
/// <param name="Parameters">Optional named event parameters.</param>
public sealed record CrashEvent(Exception Exception,
                                string EventID,
                                string? Message = null,
                                IReadOnlyDictionary<string, object?>? Parameters = null);
