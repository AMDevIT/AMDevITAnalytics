using AMDevIT.Analytics.Abstractions;
using System.Collections.Concurrent;

namespace AMDevIT.Analytics.Tests;

internal sealed class RecordingSource : IAnalyticsLoggerSource, ICrashEventLoggerSource
{
    #region Properties

    public Guid InstanceID { get; init; } = Guid.NewGuid();
    public bool IsInitialized { get; private set; }
    public int InitializeCalls { get; private set; }
    public ConcurrentQueue<AnalyticsEvent> Events { get; } = new();
    public ConcurrentQueue<CrashEvent> Errors { get; } = new();
    public Func<CancellationToken, Task> OnInitialize { get; set; } = _ => Task.CompletedTask;
    public Func<CancellationToken, Task> OnEvent { get; set; } = _ => Task.CompletedTask;
    public Func<CancellationToken, Task> OnError { get; set; } = _ => Task.CompletedTask;

    #endregion

    #region Methods

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        this.InitializeCalls++;
        await this.OnInitialize(cancellationToken);
        this.IsInitialized = true;
    }

    public async Task LogEventAsync(AnalyticsEvent analyticsEvent, CancellationToken cancellationToken = default)
    {
        if (!this.IsInitialized) throw new InvalidOperationException("Initialize must precede dispatch.");
        this.Events.Enqueue(analyticsEvent);
        await this.OnEvent(cancellationToken);
    }

    public async Task LogErrorAsync(CrashEvent crashEvent, CancellationToken cancellationToken = default)
    {
        if (!this.IsInitialized) throw new InvalidOperationException("Initialize must precede dispatch.");
        this.Errors.Enqueue(crashEvent);
        await this.OnError(cancellationToken);
    }

    #endregion
}
