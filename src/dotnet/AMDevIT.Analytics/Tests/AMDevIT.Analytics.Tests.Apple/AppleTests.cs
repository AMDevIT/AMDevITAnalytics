using AMDevIT.Analytics.Abstractions;
using AMDevIT.Analytics.Core.Extensions;
using AMDevIT.Analytics.Firebase.BindingApple;
using AMDevIT.Analytics.Firebase.ManagedApple;
using AMDevIT.Analytics.Firebase.ManagedApple.Extensions;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace AMDevIT.Analytics.Tests;

[TestClass]
public sealed class AppleTests
{
    #region Methods

    [TestMethod]
    public async Task StartupConfiguresOnlyOnceAcrossConcurrentCallers()
    {
        FirebaseAppleInitialization initialization = new(new object());
        int configurations = 0;
        Action configure = () => Interlocked.Increment(ref configurations);
        await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() => initialization.Initialize(false, true, configure))));
        Assert.AreEqual(1, configurations);
        initialization.Initialize(false, false, () => Assert.Fail("Already initialized."));
    }

    [TestMethod]
    public void StartupRequiresMainThreadAndRetriesAfterFailure()
    {
        FirebaseAppleInitialization initialization = new(new object());
        int configurations = 0;
        Assert.ThrowsExactly<InvalidOperationException>(() => initialization.Initialize(false, false, () => configurations++));
        Assert.AreEqual(0, configurations);
        Assert.ThrowsExactly<IOException>(() => initialization.Initialize(false, true, () => throw new IOException("configuration")));
        initialization.Initialize(false, true, () => configurations++);
        Assert.AreEqual(1, configurations);
    }

    [TestMethod]
    public void ExistingAppAdoptionNeverInvokesConfiguration()
    {
        FirebaseAppleInitialization initialization = new(new object());
        initialization.Initialize(true, false, () => Assert.Fail("Do not configure an existing app."));
        initialization.Initialize(false, true, () => Assert.Fail("Do not configure an adopted app."));
    }

    [TestMethod]
    public void ConvertsScalarTypesWithoutLosingInt64Precision()
    {
        object[] integers = [(byte)1, (sbyte)-1, (short)-2, (ushort)2, -3, uint.MaxValue, long.MinValue, (ulong)long.MaxValue];
        foreach (object number in integers)
        {
            using NSObject result = FirebaseAppleParameters.CreateValue(number, false);
            Assert.AreEqual(Convert.ToInt64(number, CultureInfo.InvariantCulture), ((NSNumber)result).Int64Value);
        }

        foreach (object number in new object[] { 1.5f, 2.5, 3.5m })
        {
            using NSObject result = FirebaseAppleParameters.CreateValue(number, false);
            Assert.AreEqual(Convert.ToDouble(number, CultureInfo.InvariantCulture), ((NSNumber)result).DoubleValue);
        }

        using NSObject text = FirebaseAppleParameters.CreateValue("text", false);
        using NSObject yes = FirebaseAppleParameters.CreateValue(true, false);
        using NSObject no = FirebaseAppleParameters.CreateValue(false, false);
        Assert.AreEqual("text", text.ToString());
        Assert.AreEqual(1L, ((NSNumber)yes).Int64Value);
        Assert.AreEqual(0L, ((NSNumber)no).Int64Value);
    }

    [TestMethod]
    public void RejectsInvalidAnalyticsValuesButFormatsCustomValuesInvariantly()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        foreach (object value in new object[] { double.NaN, double.PositiveInfinity, float.NegativeInfinity, ulong.MaxValue, new object(), new[] { 1 } })
        {
            Assert.ThrowsExactly<ArgumentException>(() => FirebaseAppleParameters.CreateValue(value, false));
        }

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");
            using NSObject custom = FirebaseAppleParameters.CreateValue(TimeSpan.FromSeconds(1.5), true);
            using NSObject empty = FirebaseAppleParameters.CreateValue(null, true);
            Assert.AreEqual("00:00:01.5000000", custom.ToString());
            Assert.AreEqual("", empty.ToString());
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [TestMethod]
    public void DistinguishesOmittedNullRemovedValueAndCustomEmptyString()
    {
        Dictionary<string, object?> input = new() { ["key"] = null };
        using NSString key = new("key");
        using NSDictionary omitted = FirebaseAppleParameters.Create(input)!;
        using NSDictionary removed = FirebaseAppleParameters.Create(input, nullRemovesValue: true)!;
        using NSDictionary custom = FirebaseAppleParameters.Create(input, customValues: true)!;
        Assert.AreEqual((nuint)0, omitted.Count);
        Assert.IsInstanceOfType<NSNull>(removed[key]);
        Assert.AreEqual("", custom[key].ToString());
        Assert.IsNull(FirebaseAppleParameters.Create(null));
        Assert.ThrowsExactly<ArgumentException>(() => FirebaseAppleParameters.Create(new Dictionary<string, object?> { [" "] = 1 }));
    }

    [TestMethod]
    public void ItemsRetainTheirDictionariesAndRejectNestedCollections()
    {
        Dictionary<string, object?> item = new() { ["item_id"] = "sku", ["quantity"] = 2 };
        Dictionary<string, object?> input = new() { ["items"] = new[] { item } };
        using NSDictionary result = FirebaseAppleParameters.Create(input, allowItems: true)!;
        using NSString key = new("items");
        using NSString itemKey = new("item_id");
        NSArray items = (NSArray)result[key];
        NSDictionary<NSString, NSObject> nativeItem = items.GetItem<NSDictionary<NSString, NSObject>>(0);
        Assert.AreEqual((nuint)1, items.Count);
        Assert.AreEqual("sku", nativeItem[itemKey].ToString());
        Assert.ThrowsExactly<ArgumentException>(() => FirebaseAppleParameters.Create(input));
        item["items"] = new[] { new Dictionary<string, object?>() };
        Assert.ThrowsExactly<ArgumentException>(() => FirebaseAppleParameters.Create(input, allowItems: true));
    }

    [TestMethod]
    public async Task SourceCreatesOneManagerAcrossConcurrentInitializationsAndDisposesOnce()
    {
        int created = 0;
        AnalyticsManagerSpy manager = new();
        using FirebaseAppleSource<IFirebaseAnalyticsManager> source = new(() => { Interlocked.Increment(ref created); return manager; }, () => { });
        Assert.IsFalse(source.IsInitialized);
        await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() => source.Initialize(default))));
        Assert.AreEqual(1, created);
        Assert.IsTrue(source.IsInitialized);
        source.Dispose();
        source.Dispose();
        Assert.AreEqual(1, manager.DisposeCalls);
        Assert.IsTrue(source.Disposed);
        Assert.IsFalse(source.IsInitialized);
        Assert.ThrowsExactly<ObjectDisposedException>(() => source.Read(_ => true));
    }

    [TestMethod]
    public void CancellationAndFailedStartupDoNotCreateAManager()
    {
        int created = 0;
        using CancellationTokenSource cancellation = new();
        using FirebaseAppleSource<NSObject> source = new(() => { created++; return new NSObject(); }, () => throw new InvalidOperationException("startup"));
        cancellation.Cancel();
        Assert.ThrowsExactly<OperationCanceledException>(() => source.Initialize(cancellation.Token));
        Assert.AreEqual(0, created);
        Assert.ThrowsExactly<InvalidOperationException>(() => source.Initialize(default));
        Assert.AreEqual(0, created);
        Assert.IsFalse(source.IsInitialized);
    }

    [TestMethod]
    public async Task AnalyticsAddsMessageWithoutMutatingOrOverwritingInput()
    {
        AnalyticsManagerSpy manager = new();
        using FirebaseAnalyticsLoggerSource source = CreateAnalytics(manager);
        Dictionary<string, object?> input = new() { ["quantity"] = 2 };
        await source.LogEventAsync(new AnalyticsEvent("purchase", "detail", input));
        Assert.AreEqual("purchase", manager.EventName);
        Assert.AreEqual("detail", manager.Parameters!["message"]);
        Assert.IsFalse(input.ContainsKey("message"));
        input["message"] = "explicit";
        await source.LogEventAsync(new AnalyticsEvent("purchase", "ignored", input));
        Assert.AreEqual("explicit", manager.Parameters!["message"]);
    }

    [TestMethod]
    public void AnalyticsControlsValidateAndForwardValues()
    {
        AnalyticsManagerSpy manager = new();
        using FirebaseAnalyticsLoggerSource source = CreateAnalytics(manager);
        source.SetUserID(null);
        source.SetUserProperty("tier", "gold");
        source.SetAnalyticsCollectionEnabled(false);
        source.SetConsent(FirebaseAnalyticsConsentStatus.Granted, FirebaseAnalyticsConsentStatus.Denied);
        source.SetDefaultEventParameters(null);
        source.ResetAnalyticsData();
        source.SetSessionTimeoutInterval(TimeSpan.FromMilliseconds(1250));
        Assert.AreEqual("instance", source.AppInstanceID);
        CollectionAssert.AreEqual(new[] { "user:", "property:tier:gold", "collection:False", "defaults:clear", "reset" }, manager.Calls);
        CollectionAssert.AreEqual(new[] { AnalyticsConsentStatus.Granted, AnalyticsConsentStatus.Denied, AnalyticsConsentStatus.Unchanged, AnalyticsConsentStatus.Unchanged }, manager.Consent);
        Assert.AreEqual(1.25, manager.Timeout);
        Assert.ThrowsExactly<ArgumentException>(() => source.SetUserProperty("", null));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.SetSessionTimeoutInterval(TimeSpan.Zero));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.SetConsent(adUserData: (FirebaseAnalyticsConsentStatus)99));
    }

    [TestMethod]
    public async Task SessionCallbackSupportsSuccessNilAndError()
    {
        AnalyticsManagerSpy manager = new();
        using FirebaseAnalyticsLoggerSource source = CreateAnalytics(manager);
        Task<long?> success = source.GetSessionIDAsync();
        using NSNumber number = NSNumber.FromInt64(long.MaxValue);
        manager.SessionCompletion!(number, null!);
        Assert.AreEqual(long.MaxValue, await success);
        Task<long?> unavailable = source.GetSessionIDAsync();
        manager.SessionCompletion!(null!, null!);
        Assert.IsNull(await unavailable);
        Task<long?> failed = source.GetSessionIDAsync();
        using NSError error = new(new NSString("test"), 7);
        manager.SessionCompletion!(null!, error);
        NSErrorException failure = await Assert.ThrowsExactlyAsync<NSErrorException>(() => failed);
        Assert.AreEqual((nint)7, failure.Error.Code);
    }

    [TestMethod]
    public async Task CancelingCallbackIgnoresLateErrorAndDuplicateCompletion()
    {
        using CancellationTokenSource cancellation = new();
        TaskCompletionSource<long?> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<long?> pending = FirebaseAppleCallback.WaitAsync(completion, cancellation.Token);
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => pending);
        Assert.IsTrue(completion.Task.IsCanceled);
        Assert.IsFalse(completion.TrySetException(new InvalidOperationException("late")));
        Assert.IsFalse(completion.TrySetResult(12));
    }

    [TestMethod]
    public async Task SessionCancellationAndDisposeDoNotRequireNativeCallback()
    {
        AnalyticsManagerSpy manager = new();
        using FirebaseAnalyticsLoggerSource source = CreateAnalytics(manager);
        using CancellationTokenSource cancellation = new();
        Task<long?> pending = source.GetSessionIDAsync(cancellation.Token);
        Action<NSNumber, NSError> callback = manager.SessionCompletion!;
        source.Dispose();
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => pending);
        using NSError error = new(new NSString("late"), 8);
        callback(null!, error);
        Assert.IsTrue(pending.IsCanceled);
        Assert.ThrowsExactly<ObjectDisposedException>(() => source.GetSessionIDAsync());
    }

    [TestMethod]
    public async Task CrashEventPreservesManagedExceptionAndContextOrder()
    {
        CrashlyticsManagerSpy manager = new();
        using FirebaseCrashEventLoggerSource source = CreateCrashlytics(manager);
        Exception exception = CaptureException();
        await source.LogErrorAsync(new CrashEvent(exception, "failed", "detail", new Dictionary<string, object?> { ["attempt"] = 2 }));
        Assert.AreEqual(typeof(InvalidOperationException).FullName, manager.ExceptionName);
        Assert.AreEqual("managed failure", manager.Reason);
        Assert.IsTrue(manager.Symbols.Any(symbol => symbol.Contains(nameof(CaptureException), StringComparison.Ordinal)));
        CollectionAssert.AreEqual(new[] { "log:failed", "log:detail", "keys:1", "exception" }, manager.Calls);
        source.RecordException(new InvalidOperationException(""));
        Assert.AreEqual(typeof(InvalidOperationException).FullName, manager.Reason);
        Assert.IsEmpty(manager.Symbols);
    }

    [TestMethod]
    public void CrashControlsAndNativeErrorOverloadsAreForwarded()
    {
        CrashlyticsManagerSpy manager = new();
        using FirebaseCrashEventLoggerSource source = CreateCrashlytics(manager);
        using NSError error = new(new NSString("test"), 5);
        source.RecordError(error);
        Assert.AreSame(error, manager.Error);
        source.RecordError(error, new Dictionary<string, object?> { ["detail"] = "value" });
        source.Log("breadcrumb");
        source.SetCustomValue("key", null);
        source.SetCustomKeysAndValues(new Dictionary<string, object?> { ["one"] = 1 });
        source.SetUserID(null);
        source.SetCrashlyticsCollectionEnabled(false);
        source.SendUnsentReports();
        source.DeleteUnsentReports();
        Assert.IsFalse(source.IsCrashlyticsCollectionEnabled);
        Assert.IsTrue(source.DidCrashDuringPreviousExecution);
        CollectionAssert.AreEqual(new[] { "error", "error:1", "log:breadcrumb", "value:key:", "keys:1", "user:", "collection:False", "send", "delete" }, manager.Calls);
    }

    [TestMethod]
    public async Task PendingReportsSupportBothResultsCancellationAndLateCallbacks()
    {
        CrashlyticsManagerSpy manager = new();
        using FirebaseCrashEventLoggerSource source = CreateCrashlytics(manager);
        foreach (bool expected in new[] { false, true })
        {
            Task<bool> pending = source.CheckForUnsentReportsAsync();
            manager.ReportsCompletion!(expected);
            manager.ReportsCompletion!(!expected);
            Assert.AreEqual(expected, await pending);
        }

        using CancellationTokenSource cancellation = new();
        Task<bool> canceled = source.CheckForUnsentReportsAsync(cancellation.Token);
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => canceled);
        manager.ReportsCompletion!(true);
        Assert.IsTrue(canceled.IsCanceled);
    }

    [TestMethod]
    public void RepeatedRegistrationsAreLazyAndDoNotDuplicateSources()
    {
        ServiceCollection services = new();
        services.AddAMDevITAnalytics().AddFirebase().AddFirebaseAnalytics().AddFirebaseCrashlytics();
        Assert.AreEqual(1, services.Count(item => item.ServiceType == typeof(IAnalyticsLoggerSource)));
        Assert.AreEqual(1, services.Count(item => item.ServiceType == typeof(ICrashEventLoggerSource)));
        using FirebaseAnalyticsLoggerSource first = new();
        using FirebaseCrashEventLoggerSource second = new();
        Assert.IsFalse(first.IsInitialized);
        Assert.IsFalse(second.IsInitialized);
        Assert.AreNotEqual(first.InstanceID, second.InstanceID);
    }

    private static FirebaseAnalyticsLoggerSource CreateAnalytics(AnalyticsManagerSpy manager)
    {
        return new FirebaseAnalyticsLoggerSource(new FirebaseAppleSource<IFirebaseAnalyticsManager>(() => manager, () => { }));
    }

    private static FirebaseCrashEventLoggerSource CreateCrashlytics(CrashlyticsManagerSpy manager)
    {
        return new FirebaseCrashEventLoggerSource(new FirebaseAppleSource<IFirebaseCrashlyticsManager>(() => manager, () => { }));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Exception CaptureException()
    {
        try
        {
            throw new InvalidOperationException("managed failure");
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    #endregion
}
