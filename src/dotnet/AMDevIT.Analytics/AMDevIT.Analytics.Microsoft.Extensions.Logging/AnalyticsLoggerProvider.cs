using AMDevIT.Analytics.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Channels;

namespace AMDevIT.Analytics.Microsoft.Extensions.Logging;

public sealed class AnalyticsLoggerProvider
    : ILoggerProvider, ISupportExternalScope, IAsyncDisposable
{
    #region Const

    private const string CategoryParameter = "logger_category";
    private const string EventIDParameter = "logger_event_id";
    private const string LogLevelParameter = "logger_log_level";
    private const string MessageTemplateParameter = "logger_message_template";
    private const string ScopeParameterPrefix = "logger_scope_";

    #endregion

    #region Fields

    private readonly CancellationTokenSource processingCancellation = new();
    private readonly IAnalyticsInstance analyticsInstance;
    private readonly ConcurrentDictionary<string, AnalyticsLogger> loggers = new(StringComparer.Ordinal);
    private readonly AnalyticsLoggingOptions options;
    private readonly Task processingTask;
    private readonly Channel<AnalyticsLogEntry> queue;
    private Exception? lastException;
    private IExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();
    private long droppedEntryCount;
    private long failedEntryCount;
    private int disposedValue;

    #endregion

    #region Properties

    public long DroppedEntryCount => Interlocked.Read(ref this.droppedEntryCount);

    public long FailedEntryCount => Interlocked.Read(ref this.failedEntryCount);

    public Exception? LastException => this.lastException;

    internal IExternalScopeProvider ScopeProvider => this.scopeProvider;

    #endregion

    #region .ctor

    /// <summary>Creates an analytics logger provider.</summary>
    /// <param name="analyticsInstance">Analytics instance receiving entries.</param>
    /// <param name="optionsAccessor">Provider configuration.</param>
    public AnalyticsLoggerProvider(IAnalyticsInstance analyticsInstance,
                                   IOptions<AnalyticsLoggingOptions> optionsAccessor)
    {
        BoundedChannelOptions channelOptions;

        ArgumentNullException.ThrowIfNull(analyticsInstance);
        ArgumentNullException.ThrowIfNull(optionsAccessor);

        this.analyticsInstance = analyticsInstance;
        this.options = optionsAccessor.Value;
        ValidateOptions(this.options);

        channelOptions = new BoundedChannelOptions(this.options.QueueCapacity)
        {
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        this.queue = Channel.CreateBounded<AnalyticsLogEntry>(channelOptions);
        this.processingTask = this.ProcessQueueAsync();
    }

    #endregion

    #region Methods

    /// <summary>Creates or retrieves a category logger.</summary>
    /// <param name="categoryName">Logger category.</param>
    /// <returns>The category logger.</returns>
    public ILogger CreateLogger(string categoryName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryName);

        return this.loggers.GetOrAdd(categoryName,
                                     category => new AnalyticsLogger(category, this));
    }

    /// <summary>Stops processing and flushes queued entries.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref this.disposedValue, 1) != 0)
        {
            return;
        }

        this.queue.Writer.TryComplete();

        try
        {
            if (!this.processingTask.Wait(this.options.FlushTimeout))
            {
                this.processingCancellation.Cancel();
                this.processingTask.Wait(TimeSpan.FromMilliseconds(250));
            }
        }
        catch
        {
            this.processingCancellation.Cancel();
        }
        finally
        {
            this.processingCancellation.Dispose();
        }
    }

    /// <summary>Asynchronously stops processing and flushes queued entries.</summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref this.disposedValue, 1) != 0)
        {
            return;
        }

        this.queue.Writer.TryComplete();

        try
        {
            await this.processingTask.WaitAsync(this.options.FlushTimeout);
        }
        catch (TimeoutException)
        {
            this.processingCancellation.Cancel();

            try
            {
                await this.processingTask;
            }
            catch (OperationCanceledException)
            {
            }
        }
        finally
        {
            this.processingCancellation.Dispose();
        }
    }

    /// <summary>Sets the external scope provider.</summary>
    /// <param name="scopeProvider">Scope provider to use.</param>
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        ArgumentNullException.ThrowIfNull(scopeProvider);
        this.scopeProvider = scopeProvider;
    }

    internal void Enqueue<TState>(string category,
                                  LogLevel logLevel,
                                  EventId eventID,
                                  TState state,
                                  string message,
                                  Exception? exception)
    {
        AnalyticsLogEntryContext context;
        AnalyticsLogEntry entry;
        Dictionary<string, object?> parameters;
        bool isExplicitAnalyticsEvent;
        bool sendToAnalytics;
        bool sendToCrashReporting;
        string analyticsEventID;
        string? normalizedMessage;

        if (Volatile.Read(ref this.disposedValue) != 0)
        {
            return;
        }

        isExplicitAnalyticsEvent = IsExplicitAnalyticsEvent(eventID,
                                                            this.options.AnalyticsEventNamePrefix);
        context = new AnalyticsLogEntryContext(category,
                                               logLevel,
                                               eventID,
                                               exception != null);
        sendToAnalytics = isExplicitAnalyticsEvent ||
                          this.options.SendRegularLogsToAnalytics ||
                          this.options.AnalyticsFilter?.Invoke(context) == true;
        sendToCrashReporting = exception != null &&
                               this.options.SendExceptionsToCrashReporting;

        if (!sendToAnalytics && !sendToCrashReporting)
        {
            return;
        }

        analyticsEventID = GetAnalyticsEventID(eventID,
                                               isExplicitAnalyticsEvent,
                                               this.options.AnalyticsEventNamePrefix);
        normalizedMessage = NormalizeMessage(message, this.options.MaximumMessageLength);
        parameters = this.CreateParameters(category,
                                           logLevel,
                                           eventID,
                                           state);
        entry = new AnalyticsLogEntry(analyticsEventID,
                                      normalizedMessage,
                                      parameters,
                                      exception,
                                      sendToAnalytics,
                                      sendToCrashReporting);

        if (!this.queue.Writer.TryWrite(entry))
        {
            Interlocked.Increment(ref this.droppedEntryCount);
        }
    }

    /// <summary>Determines whether a category and level are enabled.</summary>
    internal bool IsEnabled(string category,
                            LogLevel logLevel)
    {
        bool excluded;

        if (logLevel == LogLevel.None ||
            logLevel < this.options.MinimumLevel ||
            Volatile.Read(ref this.disposedValue) != 0)
        {
            return false;
        }

        excluded = this.options.ExcludedCategoryPrefixes.Any(prefix =>
            !string.IsNullOrWhiteSpace(prefix) &&
            category.StartsWith(prefix, StringComparison.Ordinal));

        return !excluded;
    }

    /// <summary>Builds the analytics event identifier for a log event.</summary>
    private static string GetAnalyticsEventID(EventId eventID,
                                              bool isExplicitAnalyticsEvent,
                                              string eventNamePrefix)
    {
        string? eventName = eventID.Name;

        if (!string.IsNullOrWhiteSpace(eventName))
        {
            if (isExplicitAnalyticsEvent)
            {
                eventName = eventName[eventNamePrefix.Length..];
            }

            if (!string.IsNullOrWhiteSpace(eventName))
            {
                return eventName;
            }
        }

        return eventID.Id == 0
            ? "logger_event"
            : $"logger_{eventID.Id}";
    }

    /// <summary>Determines whether an event explicitly targets analytics.</summary>
    private static bool IsExplicitAnalyticsEvent(EventId eventID,
                                                 string eventNamePrefix)
    {
        return !string.IsNullOrWhiteSpace(eventNamePrefix) &&
               eventID.Name?.StartsWith(eventNamePrefix,
                                        StringComparison.Ordinal) == true;
    }

    /// <summary>Truncates a log message to the configured limit.</summary>
    private static string? NormalizeMessage(string message,
                                            int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        return message.Length <= maximumLength
            ? message
            : message[..maximumLength];
    }

    /// <summary>Converts a parameter value to a supported scalar representation.</summary>
    private static object? NormalizeParameterValue(object? value)
    {
        return value switch
        {
            null => null,
            string or bool or byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal => value,
            char charValue => charValue.ToString(),
            Enum enumValue => enumValue.ToString(),
            IFormattable formattableValue => formattableValue.ToString(null,
                                                                       CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    /// <summary>Validates provider options.</summary>
    private static void ValidateOptions(AnalyticsLoggingOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.QueueCapacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.MaximumMessageLength);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.MaximumParameterCount);

        if (options.FlushTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options.FlushTimeout),
                                                  options.FlushTimeout,
                                                  "Flush timeout must be greater than zero.");
        }
    }

    /// <summary>Creates parameters from state, metadata and scopes.</summary>
    private Dictionary<string, object?> CreateParameters<TState>(string category,
                                                                  LogLevel logLevel,
                                                                  EventId eventID,
                                                                  TState state)
    {
        Dictionary<string, object?> parameters = [];
        List<object?> scopes;

        this.CopyStateParameters(parameters, state, prefix: null);

        if (this.options.IncludeCategory)
        {
            this.TryAddParameter(parameters, CategoryParameter, category);
        }

        if (this.options.IncludeLogLevel)
        {
            this.TryAddParameter(parameters, LogLevelParameter, logLevel.ToString());
        }

        if (eventID.Id != 0)
        {
            this.TryAddParameter(parameters, EventIDParameter, eventID.Id);
        }

        if (this.options.IncludeScopes)
        {
            scopes = [];
            this.scopeProvider.ForEachScope((scope, capturedScopes) => capturedScopes.Add(scope),
                                            scopes);

            for (int index = 0; index < scopes.Count; index++)
            {
                this.CopyStateParameters(parameters,
                                         scopes[index],
                                         $"{ScopeParameterPrefix}{index}_");
            }
        }

        return parameters;
    }

    /// <summary>Copies structured or scalar state into a parameter dictionary.</summary>
    private void CopyStateParameters(Dictionary<string, object?> parameters,
                                     object? state,
                                     string? prefix)
    {
        IEnumerable<KeyValuePair<string, object?>> structuredState;
        int scalarIndex;

        if (state is IEnumerable<KeyValuePair<string, object?>> currentStructuredState)
        {
            structuredState = currentStructuredState;

            foreach (KeyValuePair<string, object?> currentValue in structuredState)
            {
                if (currentValue.Key == "{OriginalFormat}")
                {
                    if (this.options.IncludeMessageTemplate)
                    {
                        this.TryAddParameter(parameters,
                                             prefix == null
                                                 ? MessageTemplateParameter
                                                 : $"{prefix}message_template",
                                             currentValue.Value);
                    }

                    continue;
                }

                this.TryAddParameter(parameters,
                                     $"{prefix}{currentValue.Key}",
                                     currentValue.Value);
            }

            return;
        }

        if (state == null || prefix == null)
        {
            return;
        }

        scalarIndex = parameters.Count;
        this.TryAddParameter(parameters,
                             $"{prefix}{scalarIndex}",
                             state);
    }

    /// <summary>Dispatches one queued entry to configured destinations.</summary>
    private async Task ProcessEntryAsync(AnalyticsLogEntry entry,
                                         CancellationToken cancellationToken)
    {
        if (entry.SendToCrashReporting && entry.Exception != null)
        {
            try
            {
                await this.analyticsInstance.LogErrorAsync(entry.Exception,
                                                           entry.EventID,
                                                           entry.Message,
                                                           entry.Parameters,
                                                           cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                this.RecordFailure(exception);
            }
        }

        if (entry.SendToAnalytics)
        {
            try
            {
                await this.analyticsInstance.LogEventAsync(entry.EventID,
                                                           entry.Message,
                                                           entry.Parameters,
                                                           cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                this.RecordFailure(exception);
            }
        }
    }

    /// <summary>Consumes queued entries until disposal or cancellation.</summary>
    private async Task ProcessQueueAsync()
    {
        CancellationToken cancellationToken = this.processingCancellation.Token;

        try
        {
            await foreach (AnalyticsLogEntry entry in this.queue.Reader.ReadAllAsync(cancellationToken))
            {
                await this.ProcessEntryAsync(entry, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            this.RecordFailure(exception);
        }
    }

    /// <summary>Records a provider processing failure.</summary>
    private void RecordFailure(Exception exception)
    {
        this.lastException = exception;
        Interlocked.Increment(ref this.failedEntryCount);
    }

    /// <summary>Adds a normalized parameter within configured limits.</summary>
    private void TryAddParameter(Dictionary<string, object?> parameters,
                                 string key,
                                 object? value)
    {
        if (parameters.Count >= this.options.MaximumParameterCount ||
            string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        parameters.TryAdd(key, NormalizeParameterValue(value));
    }

    #endregion
}
