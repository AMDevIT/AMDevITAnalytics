using Microsoft.Extensions.Logging;

namespace AMDevIT.Analytics.Microsoft.Extensions.Logging;

internal sealed class AnalyticsLogger
    : ILogger
{
    #region Fields

    private readonly string category;
    private readonly AnalyticsLoggerProvider provider;

    #endregion

    #region .ctor

    /// <summary>Creates a logger for a category.</summary>
    /// <param name="category">Logger category.</param>
    /// <param name="provider">Owning analytics provider.</param>
    public AnalyticsLogger(string category,
                           AnalyticsLoggerProvider provider)
    {
        this.category = category;
        this.provider = provider;
    }

    #endregion

    #region Methods

    /// <summary>Begins a logging scope.</summary>
    /// <param name="state">Scope state.</param>
    /// <returns>A disposable scope.</returns>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return this.provider.ScopeProvider.Push(state);
    }

    /// <summary>Determines whether a level is enabled.</summary>
    /// <param name="logLevel">Level to test.</param>
    /// <returns><see langword="true"/> when enabled.</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return this.provider.IsEnabled(this.category, logLevel);
    }

    /// <summary>Formats and queues a log entry.</summary>
    /// <param name="logLevel">Entry level.</param>
    /// <param name="eventID">Event identifier.</param>
    /// <param name="state">Structured state.</param>
    /// <param name="exception">Optional exception.</param>
    /// <param name="formatter">Message formatter.</param>
    public void Log<TState>(LogLevel logLevel,
                            EventId eventID,
                            TState state,
                            Exception? exception,
                            Func<TState, Exception?, string> formatter)
    {
        string message;

        if (!this.IsEnabled(logLevel))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(formatter);

        try
        {
            message = formatter(state, exception);
            this.provider.Enqueue(this.category,
                                  logLevel,
                                  eventID,
                                  state,
                                  message,
                                  exception);
        }
        catch
        {
            // Logging must never break the calling application.
        }
    }

    #endregion
}
