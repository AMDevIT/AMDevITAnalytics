using Microsoft.Extensions.Logging;

namespace AMDevIT.Analytics.Microsoft.Extensions.Logging;

public sealed record AnalyticsLogEntryContext(string Category,
                                              LogLevel LogLevel,
                                              EventId EventID,
                                              bool HasException);
