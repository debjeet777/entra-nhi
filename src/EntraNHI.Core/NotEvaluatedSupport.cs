namespace EntraNHI.Core;

/// <summary>
/// Carries the structured context explaining why an otherwise applicable or
/// potentially applicable rule could not be evaluated.
/// </summary>
/// <remarks>
/// This type preserves the required capability/data identity, the
/// capability/completeness context, and the affected evaluation scope. It
/// does not select an assessment state, does not map capability state to an
/// assessment state, and must not be used as PASS or FAIL evidence.
/// </remarks>
public sealed record NotEvaluatedSupport
{
    public NotEvaluatedSupport(
        TenantContext tenantContext,
        SupportText requiredCapability,
        CapabilityObservationContext observationContext,
        SupportText evaluationScope)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);
        ArgumentNullException.ThrowIfNull(requiredCapability);
        ArgumentNullException.ThrowIfNull(observationContext);
        ArgumentNullException.ThrowIfNull(evaluationScope);

        TenantContext = tenantContext;
        RequiredCapability = requiredCapability;
        ObservationContext = observationContext;
        EvaluationScope = evaluationScope;
    }

    public TenantContext TenantContext { get; }

    public SupportText RequiredCapability { get; }

    public CapabilityObservationContext ObservationContext { get; }

    public SupportText EvaluationScope { get; }
}
