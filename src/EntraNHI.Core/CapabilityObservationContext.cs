namespace EntraNHI.Core;

/// <summary>
/// Preserves the capability and completeness context grounding an observation.
/// </summary>
/// <remarks>
/// This type carries capability/completeness information only. It does not
/// calculate a pass-precondition disposition or select an assessment state.
/// </remarks>
public sealed record CapabilityObservationContext
{
    public CapabilityObservationContext(
        CapabilityState capabilityState,
        ObservationCompleteness completeness)
    {
        if (!Enum.IsDefined(capabilityState))
        {
            throw new ArgumentOutOfRangeException(
                nameof(capabilityState),
                capabilityState,
                "Capability state must be a defined canonical value.");
        }

        if (!Enum.IsDefined(completeness))
        {
            throw new ArgumentOutOfRangeException(
                nameof(completeness),
                completeness,
                "Observation completeness must be a defined canonical value.");
        }

        CapabilityState = capabilityState;
        Completeness = completeness;
    }

    public CapabilityState CapabilityState { get; }

    public ObservationCompleteness Completeness { get; }
}
