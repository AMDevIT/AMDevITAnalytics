using AMDevIT.Analytics.Firebase.BindingApple;
using Foundation;

namespace AMDevIT.Analytics.Firebase.ManagedApple;

/// <summary>Coordinates default Firebase app initialization across all managed Apple sources.</summary>
public static class FirebaseApple
{
    #region Fields

    internal static readonly object SyncRoot = new();
    private static bool initialized;

    #endregion

    #region Methods

    /// <summary>Configures Firebase once, or adopts the default app already configured by the host.</summary>
    /// <param name="useExistingApp">True only when the host has already configured the default Firebase app.</param>
    /// <remarks>
    /// Call at application startup on the main thread, before any source is used. Supply GoogleService-Info.plist
    /// when configuring here. If another Firebase integration configures the app, call with true before using
    /// these sources; the binding cannot detect an externally configured app. Repeated calls are ignored.
    /// Sources also call this method lazily, but first-time configuration still requires the main thread.
    /// </remarks>
    public static void Initialize(bool useExistingApp = false)
    {
        lock (SyncRoot)
        {
            if (initialized)
            {
                return;
            }

            if (!useExistingApp)
            {
                if (!NSThread.IsMain)
                {
                    throw new InvalidOperationException("Call FirebaseApple.Initialize() on the main thread at application startup before using Firebase sources from a background thread.");
                }

                FirebaseCoreManager.InitializeFirebase();
            }

            initialized = true;
        }
    }

    #endregion
}
