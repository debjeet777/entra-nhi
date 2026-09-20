namespace EntraNHI.Core;

/// <summary>
/// Carries the structured context for a failure that prevented trustworthy
/// evaluation.
/// </summary>
/// <remarks>
/// This type preserves a defined operational failure category, the affected
/// evaluation scope, and opaque diagnostic context. The diagnostic context
/// is opaque presence-validated text only; it carries no exception object,
/// provider type, raw payload, authentication material, or secret-bearing
/// value, and construction does NOT perform secret detection or
/// sanitization. This type does not select an assessment state and must not
/// be used as PASS or FAIL evidence.
/// </remarks>
public sealed record ErrorSupport
{
    public ErrorSupport(
        TenantContext tenantContext,
        OperationalFailureCategory failureCategory,
        SupportText evaluationScope,
        SupportText diagnosticContext)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);
        ArgumentNullException.ThrowIfNull(evaluationScope);
        ArgumentNullException.ThrowIfNull(diagnosticContext);

        if (!Enum.IsDefined(failureCategory))
        {
            throw new ArgumentOutOfRangeException(
                nameof(failureCategory),
                failureCategory,
                "Failure category must be a defined canonical value.");
        }

        TenantContext = tenantContext;
        FailureCategory = failureCategory;
        EvaluationScope = evaluationScope;
        DiagnosticContext = diagnosticContext;
    }

    public TenantContext TenantContext { get; }

    public OperationalFailureCategory FailureCategory { get; }

    public SupportText EvaluationScope { get; }

    public SupportText DiagnosticContext { get; }
}
