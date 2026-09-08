using Foundation;
using System.Globalization;

namespace AMDevIT.Analytics.Firebase.ManagedApple;

internal static class FirebaseAppleParameters
{
    #region Methods

    public static NSDictionary? Create(IReadOnlyDictionary<string, object?>? parameters,
                                                           bool customValues = false,
                                                           bool nullRemovesValue = false,
                                                           bool allowItems = false)
    {
        NSMutableDictionary<NSString, NSObject> result;

        if (parameters == null)
        {
            return null;
        }

        result = [];

        try
        {
            foreach (KeyValuePair<string, object?> parameter in parameters)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(parameter.Key);

                if (parameter.Value == null && !customValues && !nullRemovesValue)
                {
                    continue;
                }

                using NSString key = new(parameter.Key);

                if (parameter.Value == null && nullRemovesValue)
                {
                    result[key] = NSNull.Null;
                    continue;
                }

                using NSObject value = allowItems && parameter.Key == "items" &&
                                       parameter.Value is IEnumerable<IReadOnlyDictionary<string, object?>> items
                    ? CreateItems(items)
                    : CreateValue(parameter.Value, customValues);
                result[key] = value;
            }

            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }

    public static NSObject CreateValue(object? value, bool customValues)
    {
        switch (value)
        {
            case null when customValues: return new NSString(string.Empty);
            case string text: return new NSString(text);
            case bool number: return customValues ? NSNumber.FromBoolean(number) : NSNumber.FromInt64(number ? 1 : 0);
            case byte number: return NSNumber.FromInt64(number);
            case sbyte number: return NSNumber.FromInt64(number);
            case short number: return NSNumber.FromInt64(number);
            case ushort number: return NSNumber.FromInt64(number);
            case int number: return NSNumber.FromInt64(number);
            case uint number: return NSNumber.FromInt64(number);
            case long number: return NSNumber.FromInt64(number);
            case ulong number when number <= long.MaxValue: return NSNumber.FromInt64((long)number);
            case float number when float.IsFinite(number): return NSNumber.FromDouble(number);
            case double number when double.IsFinite(number): return NSNumber.FromDouble(number);
            case decimal number: return NSNumber.FromDouble((double)number);
            default:
                if (customValues)
                {
                    return new NSString(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
                }

                throw new ArgumentException($"Unsupported Firebase Analytics parameter value of type '{value?.GetType().FullName ?? "null"}'. Use strings or finite numeric values.",
                                            nameof(value));
        }
    }

    private static NSArray CreateItems(IEnumerable<IReadOnlyDictionary<string, object?>> items)
    {
        List<NSObject> dictionaries = [];

        try
        {
            foreach (IReadOnlyDictionary<string, object?> item in items)
            {
                ArgumentNullException.ThrowIfNull(item);
                dictionaries.Add(Create(item)!);
            }

            return NSArray.FromNSObjects(dictionaries.ToArray());
        }
        finally
        {
            foreach (NSObject dictionary in dictionaries)
            {
                dictionary.Dispose();
            }
        }
    }

    #endregion
}
