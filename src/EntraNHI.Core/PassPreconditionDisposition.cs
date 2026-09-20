namespace EntraNHI.Core;

/// <summary>
/// Represents the generic capability/completeness precondition for later PASS
/// consideration.
/// </summary>
/// <remarks>
/// A disposition is not an assessment state and does not authorize PASS or
/// select any other assessment outcome.
/// </remarks>
public enum PassPreconditionDisposition
{
    Satisfied,
    RequiresRuleSpecificSufficiency,
    Insufficient
}
