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

    public AnalyticsLogger(string category,
                           AnalyticsLoggerProvider provider)
    {
        this.category = category;
        this.provider = provider;
    }

    #endregion

    #region Methods

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return this.provider.ScopeProvider.Push(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return this.provider.IsEnabled(this.category, logLevel);
    }

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
