using AMDevIT.Analytics.Abstractions;

namespace AMDevIT.Analytics.Core;

/// <summary>Initializes registered analytics and crash sources and dispatches events to them.</summary>
/// <remarks>Source failures are collected and reported in an <see cref="AggregateException"/>.</remarks>
public sealed class AnalyticsInstance
    : IAnalyticsInstance
{
    #region Const

    private const string InitializeOperation = "initialize";
    private const string LogErrorOperation = "log error";
    private const string LogEventOperation = "log event";

    #endregion

    #region Fields

    private readonly IReadOnlyCollection<IAnalyticsLoggerSource> analyticsSources;
    private readonly IReadOnlyCollection<ICrashEventLoggerSource> crashSources;

    #endregion

    #region .ctor

    /// <summary>Creates an analytics instance for the supplied sources.</summary>
    /// <param name="analyticsSources">Analytics event sources.</param>
    /// <param name="crashSources">Crash event sources.</param>
    public AnalyticsInstance(IEnumerable<IAnalyticsLoggerSource>? analyticsSources = null,
                             IEnumerable<ICrashEventLoggerSource>? crashSources = null)
    {
        this.analyticsSources = analyticsSources?.ToArray() ?? [];
        this.crashSources = crashSources?.ToArray() ?? [];
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {   
        IReadOnlyCollection<IAnalyticsSource> sources;
        // List<IAnalyticsSource> concatenatedInstances = [];

        //foreach (IAnalyticsSource currentSource in this.analyticsSources)
        //{
        //    concatenatedInstances.Add(currentSource);
        //}

        //foreach (IAnalyticsSource currentSource in this.crashSources)
        //{
        //    concatenatedInstances.Add(currentSource);
        //}

        // sources = concatenatedInstances.Distinct<IAnalyticsSource>(ReferenceEqualityComparer.Instance).ToArray();

      sources = [.. this.analyticsSources.Cast<IAnalyticsSource>()
                                         .Concat(this.crashSources)
                                         .Distinct<IAnalyticsSource>(ReferenceEqualityComparer.Instance)];

        return ExecuteForAllSourcesAsync(sources,
                                         source => source.InitializeAsync(cancellationToken),
                                         InitializeOperation,
                                         cancellationToken);
    }

    /// <inheritdoc />
    public Task LogEventAsync(AnalyticsEvent analyticsEvent,
                              CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(analyticsEvent);
        ArgumentException.ThrowIfNullOrWhiteSpace(analyticsEvent.EventID);

        return ExecuteForAllSourcesAsync(this.analyticsSources,
                                         async source =>
                                         {
                                             await source.InitializeAsync(cancellationToken);
                                             await source.LogEventAsync(analyticsEvent,
                                                                        cancellationToken);
                                         },
                                         LogEventOperation,
                                         cancellationToken);
    }

    /// <inheritdoc />
    public Task LogEventAsync(string eventID,
                              string? message = null,
                              IReadOnlyDictionary<string, object?>? parameters = null,
                              CancellationToken cancellationToken = default)
    {
        AnalyticsEvent analyticsEvent = new(eventID,
                                            message,
                                            parameters);

        return this.LogEventAsync(analyticsEvent, cancellationToken);
    }

    /// <inheritdoc />
    public Task LogErrorAsync(CrashEvent crashEvent,
                              CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(crashEvent);
        ArgumentNullException.ThrowIfNull(crashEvent.Exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(crashEvent.EventID);

        return ExecuteForAllSourcesAsync(this.crashSources,
                                         async source =>
                                         {
                                             await source.InitializeAsync(cancellationToken);
                                             await source.LogErrorAsync(crashEvent,
                                                                        cancellationToken);
                                         },
                                         LogErrorOperation,
                                         cancellationToken);
    }

    /// <inheritdoc />
    public Task LogErrorAsync(Exception exception,
                              string eventID,
                              string? message = null,
                              IReadOnlyDictionary<string, object?>? parameters = null,
                              CancellationToken cancellationToken = default)
    {
        CrashEvent crashEvent = new(exception,
                                    eventID,
                                    message,
                                    parameters);

        return this.LogErrorAsync(crashEvent, cancellationToken);
    }

    /// <summary>Executes an operation for every source and aggregates failures.</summary>
    private static async Task ExecuteForAllSourcesAsync<TSource>(IReadOnlyCollection<TSource> sources,
                                                                 Func<TSource, Task> operation,
                                                                 string operationName,
                                                                 CancellationToken cancellationToken)
        where TSource : IAnalyticsSource
    {
        Task[] operations;
        List<Exception> exceptions;

        if (sources.Count == 0)
        {
            return;
        }

        operations = sources.Select(source => ExecuteForSourceAsync(source,
                                                                     operation,
                                                                     operationName,
                                                                     cancellationToken))
                            .ToArray();

        try
        {
            await Task.WhenAll(operations);
        }
        catch
        {
            exceptions = operations.Where(currentOperation => currentOperation.IsFaulted)
                                   .SelectMany(currentOperation => currentOperation.Exception!.InnerExceptions)
                                   .ToList();

            if (exceptions.Count > 0)
            {
                throw new AggregateException($"One or more analytics sources failed during '{operationName}'.",
                                             exceptions);
            }

            cancellationToken.ThrowIfCancellationRequested();
            throw;
        }
    }

    /// <summary>Executes an operation for one source and enriches provider failures.</summary>
    private static async Task ExecuteForSourceAsync<TSource>(TSource source,
                                                              Func<TSource, Task> operation,
                                                              string operationName,
                                                              CancellationToken cancellationToken)
        where TSource : IAnalyticsSource
    {
        try
        {
            await operation(source);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new AnalyticsSourceOperationException(source.InstanceID,
                                                        source.GetType(),
                                                        operationName,
                                                        exception);
        }
    }

    #endregion
}
