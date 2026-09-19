namespace EntraNHI.Core;

/// <summary>
/// Represents the canonical authoritative outcome of a rule evaluation.
/// </summary>
/// <remarks>
/// These are the only authoritative assessment states in V1.
/// Capability, data-availability, operational-failure, severity, and
/// presentation concepts are not assessment states.
/// </remarks>
public enum AssessmentState
{
    PASS,
    FAIL,
    NOT_EVALUATED,
    NOT_APPLICABLE,
    ERROR
}
