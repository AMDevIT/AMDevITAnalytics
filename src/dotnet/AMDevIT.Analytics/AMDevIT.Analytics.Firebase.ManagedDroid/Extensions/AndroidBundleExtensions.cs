namespace AMDevIT.Analytics.Firebase.ManagedDroid.Extensions;

internal static class AndroidBundleExtensions
{
    #region Methods

    public static Bundle? ToBundle<V>(this IReadOnlyDictionary<string, V> readonlyDictionary)
    {
        Bundle? bundle = null;

        if (readonlyDictionary.Count > 0)
        {
            bundle = new ();

            foreach (var currentKeyPair in readonlyDictionary)
            {
                if (currentKeyPair.Value is int intValue)
                {                    
                    bundle.PutInt(currentKeyPair.Key, intValue);
                    continue;
                }
            }
        }

        return bundle;
    }


    public static Bundle? ToBundle<V>(this Dictionary<string, V?> dictionary)        
    {
        Bundle? bundle;
        IReadOnlyDictionary<string, V?> currentDictionary = dictionary;

        bundle = currentDictionary.ToBundle();
        return bundle;
    }

    #endregion
}
