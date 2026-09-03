using AMDevIT.Analytics.Abstractions;
using AMDevIT.Analytics.Firebase.BindingApple;
using Foundation;

namespace AMDevIT.Analytics.Firebase.ManagedApple;

/// <summary>Records analytics events and exposes Apple Firebase Analytics controls.</summary>
/// <remarks>Call <see cref="FirebaseApple.Initialize"/> at application startup before background use.</remarks>
public sealed class FirebaseAnalyticsLoggerSource : IAnalyticsLoggerSource, IDisposable
{
    #region Const

    private const string MessageParameter = "message";

    #endregion

    #region Fields

    private readonly FirebaseAppleSource<IFirebaseAnalyticsManager> source;

    #endregion

    #region Properties

    /// <summary>Gets whether this source has been disposed.</summary>
    public bool Disposed => this.source.Disposed;

    /// <inheritdoc />
    public Guid InstanceID { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public bool IsInitialized => this.source.IsInitialized;

    /// <summary>Gets the Analytics app instance identifier, or null when unavailable.</summary>
    public string? AppInstanceID => this.source.Read(manager => manager.AppInstanceID);

    #endregion

    #region .ctor

    /// <summary>Creates a lazily initialized Apple Analytics source.</summary>
    public FirebaseAnalyticsLoggerSource() : this(new FirebaseAppleSource<IFirebaseAnalyticsManager>(() => new FirebaseAnalyticsManager(), () => FirebaseApple.Initialize()))
    {
    }

    internal FirebaseAnalyticsLoggerSource(FirebaseAppleSource<IFirebaseAnalyticsManager> source)
    {
        this.source = source;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        this.source.Initialize(cancellationToken);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <remarks>Supports strings, finite numbers, booleans, and an items collection of dictionaries. Null values are omitted.</remarks>
    public Task LogEventAsync(AnalyticsEvent analyticsEvent, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object?> parameters;

        ArgumentNullException.ThrowIfNull(analyticsEvent);
        ArgumentException.ThrowIfNullOrWhiteSpace(analyticsEvent.EventID);
        parameters = analyticsEvent.Parameters == null ? [] : new(analyticsEvent.Parameters);

        if (!string.IsNullOrWhiteSpace(analyticsEvent.Message) && !parameters.ContainsKey(MessageParameter))
        {
            parameters.Add(MessageParameter, analyticsEvent.Message);
        }

        using NSDictionary? nativeParameters = FirebaseAppleParameters.Create(parameters, allowItems: true);
        this.source.Execute(manager => manager.LogEventWithName(analyticsEvent.EventID, nativeParameters), cancellationToken);
        return Task.CompletedTask;
    }

    /// <summary>Sets or clears the Analytics user identifier.</summary>
    /// <param name="userID">The identifier, or null to clear it.</param>
    public void SetUserID(string? userID) => this.source.Execute(manager => manager.SetUserID(userID));

    /// <summary>Sets or clears a named Analytics user property.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The property value, or null to clear it.</param>
    public void SetUserProperty(string name, string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        this.source.Execute(manager => manager.SetUserProperty(value, name));
    }

    /// <summary>Sets Firebase's persisted Analytics collection override.</summary>
    /// <param name="enabled">Whether collection is enabled.</param>
    public void SetAnalyticsCollectionEnabled(bool enabled) => this.source.Execute(manager => manager.SetAnalyticsCollectionEnabled(enabled));

    /// <summary>Updates only the specified consent categories; no consent is granted implicitly.</summary>
    /// <param name="analyticsStorage">Analytics storage consent.</param>
    /// <param name="adStorage">Advertising storage consent.</param>
    /// <param name="adUserData">Advertising user data consent.</param>
    /// <param name="adPersonalization">Advertising personalization consent.</param>
    public void SetConsent(FirebaseAnalyticsConsentStatus analyticsStorage = FirebaseAnalyticsConsentStatus.Unchanged,
                           FirebaseAnalyticsConsentStatus adStorage = FirebaseAnalyticsConsentStatus.Unchanged,
                           FirebaseAnalyticsConsentStatus adUserData = FirebaseAnalyticsConsentStatus.Unchanged,
                           FirebaseAnalyticsConsentStatus adPersonalization = FirebaseAnalyticsConsentStatus.Unchanged)
    {
        ValidateConsent(analyticsStorage);
        ValidateConsent(adStorage);
        ValidateConsent(adUserData);
        ValidateConsent(adPersonalization);
        this.source.Execute(manager => manager.SetConsentWithAnalyticsStorage((AnalyticsConsentStatus)analyticsStorage,
                                                                               (AnalyticsConsentStatus)adStorage,
                                                                               (AnalyticsConsentStatus)adUserData,
                                                                               (AnalyticsConsentStatus)adPersonalization));
    }

    /// <summary>Merges default scalar parameters for subsequent events. Null clears all defaults; null values remove keys.</summary>
    /// <param name="parameters">The parameters to merge, or null to clear all defaults.</param>
    public void SetDefaultEventParameters(IReadOnlyDictionary<string, object?>? parameters)
    {
        using NSDictionary? nativeParameters = FirebaseAppleParameters.Create(parameters, nullRemovesValue: true);
        this.source.Execute(manager => manager.SetDefaultEventParameters(nativeParameters));
    }

    /// <summary>Clears local Analytics data and resets the app instance identifier.</summary>
    public void ResetAnalyticsData() => this.source.Execute(manager => manager.ResetAnalyticsData());

    /// <summary>Sets the positive inactivity interval used to expire Analytics sessions.</summary>
    /// <param name="interval">The inactivity interval.</param>
    public void SetSessionTimeoutInterval(TimeSpan interval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(interval, TimeSpan.Zero);
        this.source.Execute(manager => manager.SetSessionTimeoutInterval(interval.TotalSeconds));
    }

    /// <summary>Retrieves the current session identifier. Cancellation only cancels the managed wait.</summary>
    /// <param name="cancellationToken">Token used to cancel the managed wait.</param>
    /// <returns>The session identifier, or null when unavailable.</returns>
    /// <exception cref="NSErrorException">Firebase reported an error.</exception>
    public Task<long?> GetSessionIDAsync(CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<long?> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        this.source.Execute(manager => manager.SessionIDWithCompletion((sessionID, error) =>
        {
            if (error != null)
            {
                completion.TrySetException(new NSErrorException(error));
            }
            else
            {
                completion.TrySetResult(sessionID?.Int64Value);
            }
        }), cancellationToken);

        return FirebaseAppleCallback.WaitAsync(completion, cancellationToken);
    }

    /// <summary>Releases this native manager without shutting down the shared Firebase app.</summary>
    public void Dispose() => this.source.Dispose();

    private static void ValidateConsent(FirebaseAnalyticsConsentStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown Analytics consent status.");
        }
    }

    #endregion
}
