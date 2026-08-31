using System;
using Foundation;
using ObjCRuntime;

namespace AMDevIT.Analytics.Firebase.BindingApple {
    /// <summary>
    /// Exposes Firebase Analytics through the native Apple wrapper.
    /// </summary>
    /// <remarks>Configure Firebase before using this manager.</remarks>
    [BaseType (typeof (NSObject), Name = "_TtC22AmDEVFirebaseAnalytics16AnalyticsManager")]
    interface AnalyticsManager {
        /// <summary>
        /// Records an Analytics event.
        /// </summary>
        /// <param name="name">The event name, following Firebase naming restrictions.</param>
        /// <param name="parameters">Event parameters, or null when no parameters are needed.</param>
        [Export ("logEventWithName:parameters:")]
        void LogEventWithName (string name, [NullAllowed] NSDictionary<NSString, NSObject> parameters);

        /// <summary>
        /// Associates Analytics events with a user identifier.
        /// </summary>
        /// <param name="userID">The user identifier, or null to clear the current identifier.</param>
        [Export ("setUserID:")]
        void SetUserID ([NullAllowed] string userID);

        /// <summary>
        /// Sets or clears an Analytics user property.
        /// </summary>
        /// <param name="value">The property value, or null to clear it.</param>
        /// <param name="name">The user property name.</param>
        [Export ("setUserProperty:forName:")]
        void SetUserProperty ([NullAllowed] string value, string name);

        /// <summary>
        /// Sets whether Analytics collection is enabled.
        /// </summary>
        /// <param name="enabled">True to enable collection; false to disable it.</param>
        /// <remarks>Firebase persists this setting.</remarks>
        [Export ("setAnalyticsCollectionEnabled:")]
        void SetAnalyticsCollectionEnabled (bool enabled);

        /// <summary>
        /// Updates the specified Analytics consent categories.
        /// </summary>
        /// <param name="analyticsStorage">The consent status for Analytics storage.</param>
        /// <param name="adStorage">The consent status for advertising storage.</param>
        /// <param name="adUserData">The consent status for advertising user data.</param>
        /// <param name="adPersonalization">The consent status for personalized advertising.</param>
        /// <remarks>Use AnalyticsConsentStatus.Unchanged to preserve a category's existing value.</remarks>
        [Export ("setConsentWithAnalyticsStorage:adStorage:adUserData:adPersonalization:")]
        void SetConsentWithAnalyticsStorage (AnalyticsConsentStatus analyticsStorage, AnalyticsConsentStatus adStorage, AnalyticsConsentStatus adUserData, AnalyticsConsentStatus adPersonalization);

        /// <summary>
        /// Merges default parameters into subsequent Analytics events.
        /// </summary>
        /// <param name="parameters">Default parameters to merge, or null to clear all defaults. Use NSNull values to remove individual parameters.</param>
        [Export ("setDefaultEventParameters:")]
        void SetDefaultEventParameters ([NullAllowed] NSDictionary<NSString, NSObject> parameters);

        /// <summary>
        /// Clears local Analytics data and resets the app instance identifier.
        /// </summary>
        [Export ("resetAnalyticsData")]
        void ResetAnalyticsData ();

        /// <summary>
        /// Sets the inactivity timeout for Analytics sessions.
        /// </summary>
        /// <param name="interval">The session timeout interval, in seconds.</param>
        [Export ("setSessionTimeoutInterval:")]
        void SetSessionTimeoutInterval (double interval);

        /// <summary>
        /// Retrieves the current Analytics session identifier asynchronously.
        /// </summary>
        /// <param name="completion">The callback receiving the session identifier and error. On failure, the identifier is null; on success, the error is null.</param>
        /// <remarks>Firebase controls the callback queue and timing.</remarks>
        [Export ("sessionIDWithCompletion:")]
        void SessionIDWithCompletion (Action<NSNumber, NSError> completion);

        /// <summary>
        /// Gets the Analytics app instance identifier.
        /// </summary>
        /// <value>The app instance identifier, or null when unavailable.</value>
        [NullAllowed, Export ("appInstanceID")]
        string AppInstanceID { get; }
    }

    /// <summary>
    /// Exposes Firebase Crashlytics through the native Apple wrapper.
    /// </summary>
    /// <remarks>Configure Firebase before using this manager.</remarks>
    [BaseType (typeof (NSObject), Name = "_TtC22AmDEVFirebaseAnalytics18CrashlyticsManager")]
    interface CrashlyticsManager {
        /// <summary>
        /// Adds a diagnostic message to Crashlytics reports.
        /// </summary>
        /// <param name="message">The diagnostic message to log.</param>
        /// <remarks>Logging a message does not itself record an error.</remarks>
        [Export ("logWithMessage:")]
        void LogWithMessage (string message);

        /// <summary>
        /// Records a non-fatal native error.
        /// </summary>
        /// <param name="error">The native error to record.</param>
        [Export ("recordWithError:")]
        void RecordWithError (NSError error);

        /// <summary>
        /// Records a non-fatal native error with event-specific metadata.
        /// </summary>
        /// <param name="error">The native error to record.</param>
        /// <param name="userInfo">Additional error metadata, or null when no metadata is needed.</param>
        [Export ("recordWithError:userInfo:")]
        void RecordWithError (NSError error, [NullAllowed] NSDictionary<NSString, NSObject> userInfo);

        /// <summary>
        /// Records an exception supplied by another runtime, such as .NET.
        /// </summary>
        /// <param name="name">The exception name or type.</param>
        /// <param name="reason">The exception message or reason.</param>
        /// <param name="stackTrace">The symbolicated stack frames, ordered from the throw site to the outermost caller.</param>
        [Export ("recordExceptionWithName:reason:stackTrace:")]
        void RecordExceptionWithName (string name, string reason, CrashlyticsStackFrame [] stackTrace);

        /// <summary>
        /// Sets a custom value shared by subsequent Crashlytics reports.
        /// </summary>
        /// <param name="value">The custom value passed to Firebase, which may be null.</param>
        /// <param name="key">The custom key identifying the value.</param>
        [Export ("setCustomValue:forKey:")]
        void SetCustomValue ([NullAllowed] NSObject value, string key);

        /// <summary>
        /// Merges custom keys and values into subsequent Crashlytics reports.
        /// </summary>
        /// <param name="values">The custom keys and values to merge.</param>
        [Export ("setCustomKeysAndValues:")]
        void SetCustomKeysAndValues (NSDictionary<NSString, NSObject> values);

        /// <summary>
        /// Associates Crashlytics reports with a user identifier.
        /// </summary>
        /// <param name="userID">The user identifier, or null to clear the current identifier.</param>
        [Export ("setUserID:")]
        void SetUserID ([NullAllowed] string userID);

        /// <summary>
        /// Sets Crashlytics automatic report collection.
        /// </summary>
        /// <param name="enabled">True to enable automatic collection; false to disable it.</param>
        /// <remarks>Firebase persists this override. Configure Info.plist to disable automatic collection from the first app launch.</remarks>
        [Export ("setCrashlyticsCollectionEnabled:")]
        void SetCrashlyticsCollectionEnabled (bool enabled);

        /// <summary>
        /// Gets whether Crashlytics automatic report collection is enabled.
        /// </summary>
        /// <value>True when automatic collection is enabled; otherwise, false.</value>
        [Export ("isCrashlyticsCollectionEnabled")]
        bool IsCrashlyticsCollectionEnabled { get; }

        /// <summary>
        /// Gets whether the previous application execution ended in a crash.
        /// </summary>
        /// <value>True when Crashlytics detected a crash during the previous execution; otherwise, false.</value>
        [Export ("didCrashDuringPreviousExecution")]
        bool DidCrashDuringPreviousExecution { get; }

        /// <summary>
        /// Checks asynchronously for pending Crashlytics reports.
        /// </summary>
        /// <param name="completion">The callback receiving true when unsent reports are available; otherwise, false.</param>
        /// <remarks>Use when automatic collection is disabled, and call once per launch. Firebase controls the callback queue and timing.</remarks>
        [Export ("checkForUnsentReportsWithCompletion:")]
        void CheckForUnsentReportsWithCompletion (Action<bool> completion);

        /// <summary>
        /// Requests upload of pending Crashlytics reports.
        /// </summary>
        /// <remarks>Use when automatic report collection is disabled.</remarks>
        [Export ("sendUnsentReports")]
        void SendUnsentReports ();

        /// <summary>
        /// Deletes pending local Crashlytics reports.
        /// </summary>
        /// <remarks>Use when automatic report collection is disabled.</remarks>
        [Export ("deleteUnsentReports")]
        void DeleteUnsentReports ();
    }

    /// <summary>
    /// Describes a symbolicated stack frame supplied by the calling runtime.
    /// </summary>
    [BaseType (typeof (NSObject), Name = "_TtC22AmDEVFirebaseAnalytics21CrashlyticsStackFrame")]
    [DisableDefaultCtor]
    interface CrashlyticsStackFrame {
        /// <summary>
        /// Gets the symbol associated with this stack frame.
        /// </summary>
        /// <value>The fully qualified method or function name.</value>
        [Export ("symbol")]
        string Symbol { get; }

        /// <summary>
        /// Gets the source file associated with this stack frame.
        /// </summary>
        /// <value>The source file name, or an empty string when unavailable.</value>
        [Export ("file")]
        string File { get; }

        /// <summary>
        /// Gets the source line associated with this stack frame.
        /// </summary>
        /// <value>The source line number, or zero when unavailable.</value>
        [Export ("line")]
        nint Line { get; }

        /// <summary>
        /// Initializes a stack frame from a symbol and source location.
        /// </summary>
        /// <param name="symbol">The fully qualified method or function name.</param>
        /// <param name="file">The source file name, or an empty string when unavailable.</param>
        /// <param name="line">The source line number, or zero when unavailable.</param>
        [Export ("initWithSymbol:file:line:")]
        [DesignatedInitializer]
        NativeHandle Constructor (string symbol, string file, nint line);
    }

    /// <summary>
    /// Provides initialization for the native Firebase application.
    /// </summary>
    [BaseType (typeof (NSObject), Name = "_TtC22AmDEVFirebaseAnalytics19FirebaseCoreManager")]
    interface FirebaseCoreManager {
        /// <summary>
        /// Initializes the default Firebase application.
        /// </summary>
        /// <remarks>Call once before using the Analytics or Crashlytics managers. The host application must supply its Firebase configuration.</remarks>
        [Static]
        [Export ("initializeFirebase")]
        void InitializeFirebase ();
    }
}
