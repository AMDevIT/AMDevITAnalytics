namespace AMDevIT.Analytics.Firebase.ManagedApple;

/// <summary>Cancels the completion itself so a late native error cannot fault an abandoned task.</summary>
internal static class FirebaseAppleCallback
{
    #region Methods

    internal static async Task<T> WaitAsync<T>(TaskCompletionSource<T> completion, CancellationToken cancellationToken)
    {
        using CancellationTokenRegistration registration = cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));
        return await completion.Task.ConfigureAwait(false);
    }

    #endregion
}
