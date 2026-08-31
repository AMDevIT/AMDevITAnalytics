using Microsoft.Extensions.Logging;

namespace AMDevIT.Analytics.Microsoft.Extensions.Logging;

/// <summary>Describes a log entry for the optional analytics routing filter.</summary>
/// <param name="Category">Category of the logger that produced the entry.</param>
/// <param name="LogLevel">Severity of the entry.</param>
/// <param name="EventID">Identifier and optional name of the log event.</param>
/// <param name="HasException">Whether the entry includes an exception.</param>
public sealed record AnalyticsLogEntryContext(string Category,
                                              LogLevel LogLevel,
                                              EventId EventID,
                                              bool HasException);
