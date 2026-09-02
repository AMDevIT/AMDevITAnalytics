namespace AMDevIT.Analytics.Firebase.ManagedApple;

/// <summary>Testable startup gate; a failed configuration remains retryable.</summary>
internal sealed class FirebaseAppleInitialization
{
    #region Fields

    private readonly object syncRoot;
    private bool initialized;

    #endregion

    #region .ctor

    internal FirebaseAppleInitialization(object syncRoot)
    {
        this.syncRoot = syncRoot;
    }

    #endregion

    #region Methods

    internal void Initialize(bool useExistingApp, bool isMainThread, Action configure)
    {
        lock (this.syncRoot)
        {
            if (this.initialized) return;
            if (!useExistingApp)
            {
                if (!isMainThread)
                {
                    throw new InvalidOperationException("Call FirebaseApple.Initialize() on the main thread at application startup before using Firebase sources from a background thread.");
                }

                configure();
            }

            this.initialized = true;
        }
    }

    #endregion
}
