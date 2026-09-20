namespace EntraNHI.Core;

/// <summary>
/// Classifies the generic capability/completeness precondition for later PASS
/// consideration.
/// </summary>
/// <remarks>
/// This guard does not evaluate a rule, prove rule-specific sufficiency, or
/// select an assessment state.
/// </remarks>
public static class PassPreconditionGuard
{
    public static PassPreconditionDisposition Evaluate(
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

        if (capabilityState != CapabilityState.Observed)
        {
            return PassPreconditionDisposition.Insufficient;
        }

        return completeness switch
        {
            ObservationCompleteness.Complete =>
                PassPreconditionDisposition.Satisfied,

            ObservationCompleteness.Partial =>
                PassPreconditionDisposition.RequiresRuleSpecificSufficiency,

            ObservationCompleteness.Unknown =>
                PassPreconditionDisposition.Insufficient,

            _ => throw new System.Diagnostics.UnreachableException()
        };
    }
}
