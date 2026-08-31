namespace AMDevIT.Analytics.Firebase.ManagedApple;

/// <summary>Specifies how an Analytics consent category should be updated.</summary>
public enum FirebaseAnalyticsConsentStatus
{
    /// <summary>Preserves the category's existing value.</summary>
    Unchanged = 0,

    /// <summary>Grants consent for the category.</summary>
    Granted = 1,

    /// <summary>Denies consent for the category.</summary>
    Denied = 2
}
