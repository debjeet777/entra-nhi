namespace EntraNHI.Core;

/// <summary>
/// Represents the canonical category of an operational failure.
/// </summary>
/// <remarks>
/// Categories are provider-independent normalized concepts. They are separate
/// from assessment outcome, capability state, observation completeness,
/// diagnostics, and programmer or invariant violations.
/// </remarks>
public enum OperationalFailureCategory
{
    AuthorizationFailure,
    AuthenticationBoundaryFailure,
    ProviderServiceUnavailable,
    ThrottlingRateLimitation,
    Timeout,
    NetworkTransportFailure,
    MalformedInvalidProviderResponse,
    NormalizationFailure,
    PartialCollectionFailure,
    ConfigurationFailure,
    UnsupportedCapabilityOperation,
    SerializationOutputFailure,
    Cancellation,
    InternalUnclassifiedOperationalFailure
}
