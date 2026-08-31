namespace AMDevIT.Analytics.Core;

/// <summary>Identifies a failed provider operation and retains its original exception.</summary>
public sealed class AnalyticsSourceOperationException
    : Exception
{
    #region Properties

    /// <summary>Gets the identifier of the source instance whose operation failed.</summary>
    public Guid SourceInstanceID
    {
        get;
    }

    /// <summary>Gets the runtime type of the source whose operation failed.</summary>
    public Type SourceType
    {
        get;
    }

    /// <summary>Gets the name of the operation that failed.</summary>
    public string Operation
    {
        get;
    }

    #endregion

    #region .ctor

    /// <summary>Creates an exception describing a failed source operation.</summary>
    /// <param name="sourceInstanceID">Failed source instance identifier.</param>
    /// <param name="sourceType">Failed source type.</param>
    /// <param name="operation">Operation that failed.</param>
    /// <param name="innerException">Original provider exception.</param>
    internal AnalyticsSourceOperationException(Guid sourceInstanceID,
                                                Type sourceType,
                                                string operation,
                                                Exception innerException)
        : base($"Analytics source '{sourceType.FullName}' failed during '{operation}'.",
               innerException)
    {
        this.SourceInstanceID = sourceInstanceID;
        this.SourceType = sourceType;
        this.Operation = operation;
    }

    #endregion
}
