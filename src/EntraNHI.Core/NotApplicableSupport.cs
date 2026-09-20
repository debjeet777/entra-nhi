namespace EntraNHI.Core;

/// <summary>
/// Carries the structured context establishing deterministically that a rule
/// does not apply.
/// </summary>
/// <remarks>
/// This type preserves the subject context, the assessment context relevant
/// to applicability, and the applicability criterion. It introduces no
/// general subject hierarchy or assessment model, does not select an
/// assessment state, and must not be used as PASS or FAIL evidence.
/// </remarks>
public sealed record NotApplicableSupport
{
    public NotApplicableSupport(
        TenantContext tenantContext,
        NormalizedIdentityReference subject,
        SupportText assessmentContext,
        SupportText applicabilityCriterion)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(assessmentContext);
        ArgumentNullException.ThrowIfNull(applicabilityCriterion);

        TenantContext = tenantContext;
        Subject = subject;
        AssessmentContext = assessmentContext;
        ApplicabilityCriterion = applicabilityCriterion;
    }

    public TenantContext TenantContext { get; }

    public NormalizedIdentityReference Subject { get; }

    public SupportText AssessmentContext { get; }

    public SupportText ApplicabilityCriterion { get; }
}
