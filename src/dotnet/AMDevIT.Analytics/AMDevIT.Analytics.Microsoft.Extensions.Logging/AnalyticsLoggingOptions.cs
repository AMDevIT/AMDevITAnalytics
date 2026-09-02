using Microsoft.Extensions.Logging;

namespace AMDevIT.Analytics.Microsoft.Extensions.Logging;

/// <summary>Configures log routing, captured metadata, and background queue limits.</summary>
public sealed class AnalyticsLoggingOptions
{
    #region Const

    /// <summary>Default event-name prefix that explicitly opts a log entry into analytics.</summary>
    public const string DefaultAnalyticsEventNamePrefix = "Analytics.";
    /// <summary>Default maximum number of parameters attached to an entry.</summary>
    public const int DefaultMaximumParameterCount = 20;
    /// <summary>Default maximum message length in UTF-16 code units.</summary>
    public const int DefaultMaximumMessageLength = 1024;
    /// <summary>Default maximum number of entries waiting in the queue.</summary>
    public const int DefaultQueueCapacity = 256;

    #endregion

    #region Properties

    /// <summary>Gets or sets the event-name prefix that opts entries into analytics routing.</summary>
    /// <remarks>The default is <c>Analytics.</c>. The prefix is removed from the reported event identifier.</remarks>
    public string AnalyticsEventNamePrefix
    {
        get;
        set;
    } = DefaultAnalyticsEventNamePrefix;

    /// <summary>Gets or sets an optional predicate that opts additional entries into analytics routing.</summary>
    /// <remarks>A false result does not exclude entries selected by the event prefix or regular-log option.</remarks>
    public Func<AnalyticsLogEntryContext, bool>? AnalyticsFilter
    {
        get;
        set;
    }

    /// <summary>Gets the case-sensitive category prefixes excluded from all routing.</summary>
    /// <remarks>Contains <c>AMDevIT.Analytics</c> by default to prevent recursive logging.</remarks>
    public IList<string> ExcludedCategoryPrefixes
    {
        get;
    } = ["AMDevIT.Analytics"];

    /// <summary>Gets or sets how long disposal waits for queued entries before requesting cancellation.</summary>
    /// <remarks>Must be positive. The default is two seconds.</remarks>
    public TimeSpan FlushTimeout
    {
        get;
        set;
    } = TimeSpan.FromSeconds(2);

    /// <summary>Gets or sets whether the logger category is included in entry parameters. Defaults to true.</summary>
    public bool IncludeCategory
    {
        get;
        set;
    } = true;

    /// <summary>Gets or sets whether the log level is included in entry parameters. Defaults to true.</summary>
    public bool IncludeLogLevel
    {
        get;
        set;
    } = true;

    /// <summary>Gets or sets whether structured log and scope message templates are captured. Defaults to true.</summary>
    public bool IncludeMessageTemplate
    {
        get;
        set;
    } = true;

    /// <summary>Gets or sets whether active logging scopes are captured in entry parameters. Defaults to false.</summary>
    public bool IncludeScopes
    {
        get;
        set;
    }

    /// <summary>Gets or sets the maximum message length in UTF-16 code units before truncation.</summary>
    /// <remarks>Must be positive. The default is 1024.</remarks>
    public int MaximumMessageLength
    {
        get;
        set;
    } = DefaultMaximumMessageLength;

    /// <summary>Gets or sets the maximum combined number of state, metadata, and scope parameters per entry.</summary>
    /// <remarks>Must be positive. Additional parameters are omitted. The default is 20.</remarks>
    public int MaximumParameterCount
    {
        get;
        set;
    } = DefaultMaximumParameterCount;

    /// <summary>Gets or sets the minimum accepted log level. Defaults to <see cref="LogLevel.Information"/>.</summary>
    public LogLevel MinimumLevel
    {
        get;
        set;
    } = LogLevel.Information;

    /// <summary>Gets or sets the maximum number of entries waiting for background processing.</summary>
    /// <remarks>Must be positive. New entries are dropped when the queue is full. The default is 256.</remarks>
    public int QueueCapacity
    {
        get;
        set;
    } = DefaultQueueCapacity;

    /// <summary>Gets or sets whether accepted entries with exceptions are sent to crash reporting. Defaults to true.</summary>
    public bool SendExceptionsToCrashReporting
    {
        get;
        set;
    } = true;

    /// <summary>Gets or sets whether all accepted entries are sent to analytics without an explicit opt-in. Defaults to false.</summary>
    public bool SendRegularLogsToAnalytics
    {
        get;
        set;
    }

    #endregion
}
