namespace EntraNHI.Core;

/// <summary>
/// Represents the canonical availability state of a capability observation.
/// </summary>
/// <remarks>
/// Capability state is separate from assessment outcome, observation
/// completeness, operational failure, and provider-specific mechanics.
/// </remarks>
public enum CapabilityState
{
    Observed,
    NotRequested,
    UnavailableAuthorization,
    UnavailableLicensingService,
    Unsupported,
    Failed,
    Unknown
}