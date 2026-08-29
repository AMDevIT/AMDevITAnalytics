using Android.OS;

namespace AMDevIT.Analytics.Firebase.ManagedDroid.Extensions;

internal static class AndroidBundleExtensions
{
    #region Methods

    public static Bundle? ToBundle(this IReadOnlyDictionary<string, object?> dictionary)
    {
        Bundle? bundle;

        ArgumentNullException.ThrowIfNull(dictionary);

        if (dictionary.Count == 0)
        {
            return null;
        }

        bundle = new Bundle();

        foreach (KeyValuePair<string, object?> currentParameter in dictionary)
        {
            PutValue(bundle,
                     currentParameter.Key,
                     currentParameter.Value);
        }

        return bundle;
    }

    private static void PutValue(Bundle bundle,
                                 string key,
                                 object? value)
    {
        switch (value)
        {
            case null:
                return;
            case string stringValue:
                bundle.PutString(key, stringValue);
                return;
            case bool boolValue:
                bundle.PutLong(key, boolValue ? 1L : 0L);
                return;
            case byte byteValue:
                bundle.PutLong(key, byteValue);
                return;
            case sbyte sbyteValue:
                bundle.PutLong(key, sbyteValue);
                return;
            case short shortValue:
                bundle.PutLong(key, shortValue);
                return;
            case ushort ushortValue:
                bundle.PutLong(key, ushortValue);
                return;
            case int intValue:
                bundle.PutLong(key, intValue);
                return;
            case uint uintValue:
                bundle.PutLong(key, uintValue);
                return;
            case long longValue:
                bundle.PutLong(key, longValue);
                return;
            case ulong ulongValue when ulongValue <= long.MaxValue:
                bundle.PutLong(key, (long)ulongValue);
                return;
            case float floatValue:
                bundle.PutDouble(key, floatValue);
                return;
            case double doubleValue:
                bundle.PutDouble(key, doubleValue);
                return;
            case decimal decimalValue:
                bundle.PutDouble(key, (double)decimalValue);
                return;
            default:
                throw new ArgumentException($"Analytics parameter '{key}' has unsupported type '{value.GetType().FullName}'.",
                                            nameof(value));
        }
    }

    #endregion
}
