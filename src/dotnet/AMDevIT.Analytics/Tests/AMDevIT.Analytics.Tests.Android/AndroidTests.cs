using AMDevIT.Analytics.Abstractions;
using AMDevIT.Analytics.Core.Extensions;
using AMDevIT.Analytics.Firebase.ManagedDroid.Extensions;
using Android.OS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMDevIT.Analytics.Tests;

[TestClass]
public sealed class AndroidTests
{
    #region Methods

    [TestMethod]
    public void ConvertsAllSupportedScalarTypes()
    {
        Dictionary<string, object?> input = new()
        {
            ["string"] = "text", ["true"] = true, ["false"] = false, ["byte"] = (byte)1,
            ["sbyte"] = (sbyte)-1, ["short"] = (short)-2, ["ushort"] = (ushort)2,
            ["int"] = -3, ["uint"] = uint.MaxValue, ["long"] = long.MinValue,
            ["ulong"] = (ulong)long.MaxValue, ["float"] = 1.5f, ["double"] = 2.5, ["decimal"] = 3.5m,
            ["null"] = null
        };
        using Bundle bundle = input.ToBundle()!;
        Assert.AreEqual("text", bundle.GetString("string"));
        Assert.AreEqual(1L, bundle.GetLong("true"));
        Assert.AreEqual(0L, bundle.GetLong("false"));
        Assert.AreEqual(1L, bundle.GetLong("byte"));
        Assert.AreEqual(-1L, bundle.GetLong("sbyte"));
        Assert.AreEqual(-2L, bundle.GetLong("short"));
        Assert.AreEqual(2L, bundle.GetLong("ushort"));
        Assert.AreEqual(-3L, bundle.GetLong("int"));
        Assert.AreEqual((long)uint.MaxValue, bundle.GetLong("uint"));
        Assert.AreEqual(long.MinValue, bundle.GetLong("long"));
        Assert.AreEqual(long.MaxValue, bundle.GetLong("ulong"));
        Assert.AreEqual(1.5, bundle.GetDouble("float"));
        Assert.AreEqual(2.5, bundle.GetDouble("double"));
        Assert.AreEqual(3.5, bundle.GetDouble("decimal"));
        Assert.IsFalse(bundle.ContainsKey("null"));
    }

    [TestMethod]
    public void HandlesEmptyNullAndUnsupportedParameters()
    {
        Assert.IsNull(new Dictionary<string, object?>().ToBundle());
        Assert.ThrowsExactly<ArgumentNullException>(() => AndroidBundleExtensions.ToBundle(null!));
        Assert.ThrowsExactly<ArgumentException>(() => new Dictionary<string, object?> { ["overflow"] = ulong.MaxValue }.ToBundle());
        Assert.ThrowsExactly<ArgumentException>(() => new Dictionary<string, object?> { ["object"] = new object() }.ToBundle());
        Assert.ThrowsExactly<ArgumentException>(() => new Dictionary<string, object?> { ["array"] = new[] { 1 } }.ToBundle());
    }

    [TestMethod]
    public void RepeatedRegistrationsDoNotCreateDuplicateProviders()
    {
        ServiceCollection services = new();
        services.AddAMDevITAnalytics().AddFirebase().AddFirebaseAnalytics().AddFirebaseCrashlytics();
        Assert.AreEqual(1, services.Count(item => item.ServiceType == typeof(IAnalyticsLoggerSource)));
        Assert.AreEqual(1, services.Count(item => item.ServiceType == typeof(ICrashEventLoggerSource)));
    }

    #endregion
}
