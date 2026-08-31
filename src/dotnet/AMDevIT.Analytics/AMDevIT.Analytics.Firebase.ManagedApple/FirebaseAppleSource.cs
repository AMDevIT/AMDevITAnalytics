using Foundation;

namespace AMDevIT.Analytics.Firebase.ManagedApple;

/// <summary>Serializes manager access, lazy initialization, and disposal.</summary>
internal sealed class FirebaseAppleSource<TManager> : IDisposable
    where TManager : NSObject, new()
{
    #region Fields

    private TManager? manager;
    private bool disposed;

    #endregion

    #region Properties

    public bool Disposed
    {
        get
        {
            lock (FirebaseApple.SyncRoot)
            {
                return this.disposed;
            }
        }
    }

    public bool IsInitialized
    {
        get
        {
            lock (FirebaseApple.SyncRoot)
            {
                return this.manager != null && !this.disposed;
            }
        }
    }

    #endregion

    #region Methods

    public void Initialize(CancellationToken cancellationToken)
    {
        this.Execute(_ => { }, cancellationToken);
    }

    public void Execute(Action<TManager> operation, CancellationToken cancellationToken = default)
    {
        this.Read(manager =>
        {
            operation(manager);
            return true;
        }, cancellationToken);
    }

    public TResult Read<TResult>(Func<TManager, TResult> operation, CancellationToken cancellationToken = default)
    {
        lock (FirebaseApple.SyncRoot)
        {
            ObjectDisposedException.ThrowIf(this.disposed, this);
            cancellationToken.ThrowIfCancellationRequested();
            FirebaseApple.Initialize();
            this.manager ??= new TManager();
            cancellationToken.ThrowIfCancellationRequested();
            return operation(this.manager);
        }
    }

    public void Dispose()
    {
        lock (FirebaseApple.SyncRoot)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.manager?.Dispose();
            this.manager = null;
        }
    }

    #endregion
}
