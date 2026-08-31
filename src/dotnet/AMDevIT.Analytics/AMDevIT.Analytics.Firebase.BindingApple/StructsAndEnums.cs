using ObjCRuntime;

namespace AMDevIT.Analytics.Firebase.BindingApple {
    /// <summary>
    /// Specifies the consent update for an Analytics data category.
    /// </summary>
    [Native]
    public enum AnalyticsConsentStatus : long {
        /// <summary>
        /// Preserves the category's previously configured consent value.
        /// </summary>
        Unchanged = 0,

        /// <summary>
        /// Grants consent for the category.
        /// </summary>
        Granted = 1,

        /// <summary>
        /// Denies consent for the category.
        /// </summary>
        Denied = 2
    }
}
