using AMDevIT.Analytics.Abstractions;
using AMDevIT.Analytics.Core;
using AMDevIT.Analytics.Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMDevIT.Analytics.Tests;

[TestClass]
public sealed class AnalyticsLoggerTests
{
    #region Methods

    [TestMethod]
    public async Task ExplicitEventAndExceptionRouteIndependently()
    {
        RecordingSource source = new();
        AnalyticsLoggerProvider provider = CreateProvider(source);
        ILogger logger = provider.CreateLogger("Host.Checkout");
        Exception error = new InvalidOperationException("failed");

        logger.LogInformation("ordinary log");
        logger.LogInformation(new EventId(7, "Analytics.checkout"), "Bought {Product}", "sku");
        logger.LogError(error, "Checkout failed");
        await provider.DisposeAsync();

        Assert.AreEqual("checkout", source.Events.Single().EventID);
        Assert.AreEqual("sku", source.Events.Single().Parameters!["Product"]);
        Assert.AreSame(error, source.Errors.Single().Exception);
        Assert.AreEqual("logger_event", source.Errors.Single().EventID);
        Assert.AreEqual(0L, provider.FailedEntryCount);
    }

    [TestMethod]
    public async Task MinimumLevelExcludedCategoriesAndNoneAreRespected()
    {
        RecordingSource source = new();
        AnalyticsLoggerProvider provider = CreateProvider(source, new() { SendRegularLogsToAnalytics = true });
        ILogger logger = provider.CreateLogger("Host");
        logger.LogDebug("ignored");
        logger.Log(LogLevel.None, "ignored");
        provider.CreateLogger("AMDevIT.Analytics.Internal").LogCritical(new Exception(), "ignored");
        logger.LogWarning("accepted");
        await provider.DisposeAsync();

        Assert.HasCount(1, source.Events);
        Assert.IsEmpty(source.Errors);
        Assert.IsFalse(logger.IsEnabled(LogLevel.Critical));
        Assert.AreSame(logger, provider.CreateLogger("Host"));
        await provider.DisposeAsync();
        provider.Dispose();
    }

    [TestMethod]
    public async Task StructuredStateAndScopeAreSnapshots()
    {
        RecordingSource source = new();
        AnalyticsLoggerProvider provider = CreateProvider(source, new() { IncludeScopes = true });
        ILogger logger = provider.CreateLogger("Host");
        Dictionary<string, object?> state = new() { ["value"] = 1, ["{OriginalFormat}"] = "Value {value}" };
        Dictionary<string, object?> scope = new() { ["session"] = "before" };

        using (logger.BeginScope(scope))
        {
            logger.Log(LogLevel.Information, new EventId(8, "Analytics.snapshot"), state, null, (_, _) => "snapshot");
            state["value"] = 2;
            scope["session"] = "after";
        }

        await provider.DisposeAsync();

        IReadOnlyDictionary<string, object?> parameters = source.Events.Single().Parameters!;
        Assert.AreEqual(1, parameters["value"]);
        Assert.AreEqual("before", parameters["logger_scope_0_session"]);
        Assert.AreEqual("Value {value}", parameters["logger_message_template"]);
        Assert.AreEqual("Host", parameters["logger_category"]);
        Assert.AreEqual("Information", parameters["logger_log_level"]);
    }

    [TestMethod]
    public async Task EnforcesMessageAndParameterLimits()
    {
        RecordingSource source = new();
        AnalyticsLoggerProvider provider = CreateProvider(source, new()
        {
            MaximumMessageLength = 4,
            MaximumParameterCount = 1,
            SendRegularLogsToAnalytics = true
        });
        ILogger logger = provider.CreateLogger("Host");
        logger.LogInformation("abcdef {Value}", 3);
        await provider.DisposeAsync();

        Assert.AreEqual("abcd", source.Events.Single().Message);
        Assert.AreEqual(1, source.Events.Single().Parameters!.Count);
        Assert.AreEqual(3, source.Events.Single().Parameters!["Value"]);
    }

    [TestMethod]
    public async Task FilteringAndFallbackNamesWorkWithoutPrefix()
    {
        RecordingSource source = new();
        AnalyticsLoggerProvider provider = CreateProvider(source, new() { AnalyticsFilter = _ => true });
        ILogger logger = provider.CreateLogger("Host");
        logger.LogInformation(new EventId(9, "Analytics."), "fallback");
        logger.LogInformation(new EventId(0), "default");
        logger.LogInformation(new EventId(1, "custom"), "named");
        await provider.DisposeAsync();

        CollectionAssert.AreEqual(new[] { "logger_9", "logger_event", "custom" }, source.Events.Select(entry => entry.EventID).ToArray());
    }

    [TestMethod]
    public async Task CrashFailureDoesNotSuppressAnalyticsOrLaterEntries()
    {
        RecordingSource source = new() { OnError = _ => throw new IOException("offline") };
        AnalyticsLoggerProvider provider = CreateProvider(source, new() { SendRegularLogsToAnalytics = true });
        ILogger logger = provider.CreateLogger("Host");
        logger.LogError(new Exception(), "both");
        logger.LogInformation("later");
        await provider.DisposeAsync();

        Assert.HasCount(2, source.Events);
        Assert.AreEqual(1L, provider.FailedEntryCount);
        Assert.IsInstanceOfType<AggregateException>(provider.LastException);
    }

    [TestMethod]
    public async Task FullQueueDropsNewEntryWithoutBlockingCaller()
    {
        TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        RecordingSource source = new()
        {
            OnEvent = _ => { started.TrySetResult(); return release.Task; }
        };
        AnalyticsLoggerProvider provider = CreateProvider(source, new() { QueueCapacity = 1, SendRegularLogsToAnalytics = true });
        ILogger logger = provider.CreateLogger("Host");

        try
        {
            logger.LogInformation("processing");
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            logger.LogInformation("queued");
            logger.LogInformation("dropped");
            Assert.AreEqual(1L, provider.DroppedEntryCount);
        }
        finally
        {
            release.TrySetResult();
            await provider.DisposeAsync();
        }

        CollectionAssert.AreEqual(new[] { "processing", "queued" }, source.Events.Select(entry => entry.Message).ToArray());
    }

    [TestMethod]
    public async Task FlushTimeoutCancelsCooperativeProvider()
    {
        TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        RecordingSource source = new()
        {
            OnEvent = token => { started.TrySetResult(); return Task.Delay(Timeout.InfiniteTimeSpan, token); }
        };
        AnalyticsLoggerProvider provider = CreateProvider(source, new()
        {
            SendRegularLogsToAnalytics = true,
            FlushTimeout = TimeSpan.FromMilliseconds(50)
        });
        provider.CreateLogger("Host").LogInformation("pending");
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await provider.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
        Assert.AreEqual(0L, provider.FailedEntryCount);
    }

    [TestMethod]
    public void RejectsInvalidOptionsBeforeStartingWorker()
    {
        RecordingSource source = new();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => CreateProvider(source, new() { QueueCapacity = 0 }));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => CreateProvider(source, new() { MaximumMessageLength = 0 }));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => CreateProvider(source, new() { MaximumParameterCount = 0 }));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => CreateProvider(source, new() { FlushTimeout = TimeSpan.Zero }));
    }

    [TestMethod]
    public async Task FlushTimeoutDoesNotHangWhenProviderIgnoresCancellation()
    {
        TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        RecordingSource source = new() { OnEvent = _ => { started.TrySetResult(); return release.Task; } };
        AnalyticsLoggerProvider provider = CreateProvider(source, new()
        {
            SendRegularLogsToAnalytics = true,
            FlushTimeout = TimeSpan.FromMilliseconds(50)
        });
        Task disposal = Task.CompletedTask;

        try
        {
            provider.CreateLogger("Host").LogInformation("pending");
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            disposal = provider.DisposeAsync().AsTask();
            await disposal.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            release.TrySetResult();
            await disposal;
            await provider.DisposeAsync();
        }
    }

    private static AnalyticsLoggerProvider CreateProvider(RecordingSource source, AnalyticsLoggingOptions? options = null)
    {
        return new AnalyticsLoggerProvider(new AnalyticsInstance([source], [source]),
                                           Options.Create(options ?? new AnalyticsLoggingOptions()));
    }

    #endregion
}
