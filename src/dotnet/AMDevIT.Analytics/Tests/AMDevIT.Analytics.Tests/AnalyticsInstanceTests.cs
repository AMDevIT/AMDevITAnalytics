using AMDevIT.Analytics.Abstractions;
using AMDevIT.Analytics.Core;
using AMDevIT.Analytics.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMDevIT.Analytics.Tests;

[TestClass]
public sealed class AnalyticsInstanceTests
{
    #region Methods

    [TestMethod]
    public async Task InitializationDeduplicatesByReferenceRatherThanIdentifier()
    {
        RecordingSource first = new();
        RecordingSource second = new() { InstanceID = first.InstanceID };
        AnalyticsInstance instance = new([first, second], [first]);

        await instance.InitializeAsync();

        Assert.AreEqual(1, first.InitializeCalls);
        Assert.AreEqual(1, second.InitializeCalls);
    }

    [TestMethod]
    public async Task RoutesRecordsToTheCorrectProvidersWithoutCopyingPayloads()
    {
        RecordingSource analytics = new();
        RecordingSource crashes = new();
        AnalyticsInstance instance = new([analytics], [crashes]);
        AnalyticsEvent analyticsEvent = new("checkout", "message", new Dictionary<string, object?> { ["count"] = 2 });
        CrashEvent crashEvent = new(new InvalidOperationException("failure"), "checkout_failed");

        await instance.LogEventAsync(analyticsEvent);
        await instance.LogErrorAsync(crashEvent);

        Assert.AreSame(analyticsEvent, analytics.Events.Single());
        Assert.AreSame(crashEvent, crashes.Errors.Single());
        Assert.IsEmpty(analytics.Errors);
        Assert.IsEmpty(crashes.Events);
    }

    [TestMethod]
    public async Task ConvenienceOverloadsPreserveAllArguments()
    {
        RecordingSource source = new();
        AnalyticsInstance instance = new([source], [source]);
        Exception error = new InvalidOperationException("failure");
        Dictionary<string, object?> parameters = new() { ["attempt"] = 3 };

        await instance.LogEventAsync("event", "message", parameters);
        await instance.LogErrorAsync(error, "error", "detail", parameters);

        Assert.AreEqual("event", source.Events.Single().EventID);
        Assert.AreEqual("message", source.Events.Single().Message);
        Assert.AreSame(parameters, source.Events.Single().Parameters);
        Assert.AreSame(error, source.Errors.Single().Exception);
        Assert.AreEqual("error", source.Errors.Single().EventID);
        Assert.AreEqual("detail", source.Errors.Single().Message);
        Assert.AreSame(parameters, source.Errors.Single().Parameters);
    }

    [TestMethod]
    public async Task EmptyProviderSetsAreNoOps()
    {
        AnalyticsInstance instance = new();
        await instance.InitializeAsync();
        await instance.LogEventAsync("event");
        await instance.LogErrorAsync(new Exception(), "error");
    }

    [TestMethod]
    public void RejectsInvalidRecordsBeforeDispatch()
    {
        AnalyticsInstance instance = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => instance.LogEventAsync((AnalyticsEvent)null!));
        Assert.ThrowsExactly<ArgumentException>(() => instance.LogEventAsync(" "));
        Assert.ThrowsExactly<ArgumentNullException>(() => instance.LogErrorAsync((CrashEvent)null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => instance.LogErrorAsync(null!, "error"));
        Assert.ThrowsExactly<ArgumentException>(() => instance.LogErrorAsync(new Exception(), ""));
    }

    [TestMethod]
    public async Task StartsEveryProviderBeforeAwaitingAnyProvider()
    {
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource secondStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        RecordingSource first = new() { OnEvent = _ => release.Task };
        RecordingSource second = new() { OnEvent = _ => { secondStarted.SetResult(); return Task.CompletedTask; } };
        AnalyticsInstance instance = new([first, second]);
        Task dispatch = instance.LogEventAsync("event");

        try
        {
            await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.IsFalse(dispatch.IsCompleted);
        }
        finally
        {
            release.TrySetResult();
            await dispatch;
        }
    }

    [TestMethod]
    public async Task AggregatesAllFailuresAndStillCallsHealthyProviders()
    {
        Exception firstError = new InvalidOperationException("first");
        Exception secondError = new IOException("second");
        RecordingSource first = new() { OnEvent = _ => throw firstError };
        RecordingSource second = new() { OnEvent = _ => Task.FromException(secondError) };
        RecordingSource healthy = new();
        AnalyticsInstance instance = new([first, second, healthy]);
        AggregateException failure = await Assert.ThrowsExactlyAsync<AggregateException>(() => instance.LogEventAsync("event"));
        AnalyticsSourceOperationException[] details = failure.InnerExceptions.Cast<AnalyticsSourceOperationException>().ToArray();

        Assert.AreEqual(2, details.Length);
        Assert.AreEqual(first.InstanceID, details[0].SourceInstanceID);
        Assert.AreEqual(typeof(RecordingSource), details[0].SourceType);
        Assert.AreEqual("log event", details[0].Operation);
        Assert.AreSame(firstError, details[0].InnerException);
        Assert.AreSame(secondError, details[1].InnerException);
        Assert.HasCount(1, healthy.Events);
    }

    [TestMethod]
    public async Task InitializationFailurePreventsOnlyThatSourcesDispatch()
    {
        RecordingSource failed = new() { OnInitialize = _ => throw new InvalidOperationException() };
        RecordingSource healthy = new();
        AnalyticsInstance instance = new(crashSources: [failed, healthy]);
        AggregateException failure = await Assert.ThrowsExactlyAsync<AggregateException>(() => instance.LogErrorAsync(new Exception(), "error"));

        Assert.IsEmpty(failed.Errors);
        Assert.HasCount(1, healthy.Errors);
        Assert.AreEqual("log error", ((AnalyticsSourceOperationException)failure.InnerExceptions.Single()).Operation);
        failure = await Assert.ThrowsExactlyAsync<AggregateException>(() => instance.InitializeAsync());
        Assert.AreEqual("initialize", ((AnalyticsSourceOperationException)failure.InnerExceptions.Single()).Operation);
    }

    [TestMethod]
    public async Task CallerCancellationIsPropagatedWithItsToken()
    {
        using CancellationTokenSource cancellation = new();
        RecordingSource source = new() { OnEvent = token => Task.FromCanceled(token) };
        AnalyticsInstance instance = new([source]);
        cancellation.Cancel();
        OperationCanceledException failure = await Assert.ThrowsAsync<OperationCanceledException>(() => instance.LogEventAsync("event", cancellationToken: cancellation.Token));
        Assert.AreEqual(cancellation.Token, failure.CancellationToken);
    }

    [TestMethod]
    public async Task ForeignCancellationIsAProviderFailure()
    {
        RecordingSource source = new() { OnEvent = _ => throw new OperationCanceledException() };
        AnalyticsInstance instance = new([source]);
        AggregateException failure = await Assert.ThrowsExactlyAsync<AggregateException>(() => instance.LogEventAsync("event"));
        Assert.IsInstanceOfType<OperationCanceledException>(failure.InnerExceptions.Single().InnerException);
    }

    [TestMethod]
    public async Task ProviderFaultsTakePrecedenceOverCallerCancellation()
    {
        using CancellationTokenSource cancellation = new();
        RecordingSource canceled = new() { OnEvent = token => Task.FromCanceled(token) };
        RecordingSource failed = new() { OnEvent = _ => throw new IOException("failure") };
        AnalyticsInstance instance = new([canceled, failed]);
        cancellation.Cancel();
        AggregateException failure = await Assert.ThrowsExactlyAsync<AggregateException>(() => instance.LogEventAsync("event", cancellationToken: cancellation.Token));
        Assert.AreEqual(failed.InstanceID, ((AnalyticsSourceOperationException)failure.InnerExceptions.Single()).SourceInstanceID);
    }

    [TestMethod]
    public void RepeatedCoreRegistrationPreservesHostOverride()
    {
        ServiceCollection services = new();
        AnalyticsInstance custom = new();
        services.AddSingleton<IAnalyticsInstance>(custom);
        services.AddAMDevITAnalytics();
        services.AddAMDevITAnalytics();
        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.AreSame(custom, provider.GetRequiredService<IAnalyticsInstance>());
        Assert.HasCount(1, provider.GetServices<IAnalyticsInstance>());
    }

    #endregion
}
