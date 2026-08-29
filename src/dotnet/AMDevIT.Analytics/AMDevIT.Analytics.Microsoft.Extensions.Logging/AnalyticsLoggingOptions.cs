using Microsoft.Extensions.Logging;

namespace AMDevIT.Analytics.Microsoft.Extensions.Logging;

public sealed class AnalyticsLoggingOptions
{
    #region Const

    public const string DefaultAnalyticsEventNamePrefix = "Analytics.";
    public const int DefaultMaximumParameterCount = 20;
    public const int DefaultMaximumMessageLength = 1024;
    public const int DefaultQueueCapacity = 256;

    #endregion

    #region Properties

    public string AnalyticsEventNamePrefix
    {
        get;
        set;
    } = DefaultAnalyticsEventNamePrefix;

    public Func<AnalyticsLogEntryContext, bool>? AnalyticsFilter
    {
        get;
        set;
    }

    public IList<string> ExcludedCategoryPrefixes
    {
        get;
    } = ["AMDevIT.Analytics"];

    public TimeSpan FlushTimeout
    {
        get;
        set;
    } = TimeSpan.FromSeconds(2);

    public bool IncludeCategory
    {
        get;
        set;
    } = true;

    public bool IncludeLogLevel
    {
        get;
        set;
    } = true;

    public bool IncludeMessageTemplate
    {
        get;
        set;
    } = true;

    public bool IncludeScopes
    {
        get;
        set;
    }

    public int MaximumMessageLength
    {
        get;
        set;
    } = DefaultMaximumMessageLength;

    public int MaximumParameterCount
    {
        get;
        set;
    } = DefaultMaximumParameterCount;

    public LogLevel MinimumLevel
    {
        get;
        set;
    } = LogLevel.Information;

    public int QueueCapacity
    {
        get;
        set;
    } = DefaultQueueCapacity;

    public bool SendExceptionsToCrashReporting
    {
        get;
        set;
    } = true;

    public bool SendRegularLogsToAnalytics
    {
        get;
        set;
    }

    #endregion
}
