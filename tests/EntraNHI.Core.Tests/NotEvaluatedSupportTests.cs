using System.Reflection;

namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class NotEvaluatedSupportTests
{
    private static NotEvaluatedSupport CreateSupport(
        TenantContext? tenantContext = null,
        SupportText? requiredCapability = null,
        CapabilityObservationContext? observationContext = null,
        SupportText? evaluationScope = null)
    {
        return new NotEvaluatedSupport(
            tenantContext ?? new TenantContext("synthetic-tenant"),
            requiredCapability ?? new SupportText("synthetic-required-capability"),
            observationContext ?? new CapabilityObservationContext(
                CapabilityState.Observed,
                ObservationCompleteness.Complete),
            evaluationScope ?? new SupportText("synthetic-evaluation-scope"));
    }

    [TestMethod]
    public void ExposesTenantContextRequiredCapabilityObservationContextAndScope()
    {
        var tenantContext = new TenantContext("synthetic-tenant");
        var requiredCapability = new SupportText("synthetic-required-capability");
        var observationContext = new CapabilityObservationContext(
            CapabilityState.UnavailableAuthorization,
            ObservationCompleteness.Partial);
        var evaluationScope = new SupportText("synthetic-evaluation-scope");

        var support = new NotEvaluatedSupport(
            tenantContext,
            requiredCapability,
            observationContext,
            evaluationScope);

        Assert.AreSame(tenantContext, support.TenantContext);
        Assert.AreEqual(requiredCapability, support.RequiredCapability);
        Assert.AreEqual(observationContext, support.ObservationContext);
        Assert.AreEqual(evaluationScope, support.EvaluationScope);
    }

    [TestMethod]
    public void RetainsTenantContextExplicitly()
    {
        var tenantContext = new TenantContext("synthetic-tenant");

        NotEvaluatedSupport support = CreateSupport(tenantContext: tenantContext);

        Assert.AreSame(tenantContext, support.TenantContext);
    }

    [TestMethod]
    public void AcceptsUnknownCapabilityAndUnknownCompletenessAsDomainContext()
    {
        var observationContext = new CapabilityObservationContext(
            CapabilityState.Unknown,
            ObservationCompleteness.Unknown);

        NotEvaluatedSupport support = CreateSupport(observationContext: observationContext);

        Assert.AreEqual(CapabilityState.Unknown, support.ObservationContext.CapabilityState);
        Assert.AreEqual(ObservationCompleteness.Unknown, support.ObservationContext.Completeness);
    }

    [TestMethod]
    public void RejectsNullTenantContext()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new NotEvaluatedSupport(
                null!,
                new SupportText("synthetic-required-capability"),
                new CapabilityObservationContext(
                    CapabilityState.Observed,
                    ObservationCompleteness.Complete),
                new SupportText("synthetic-evaluation-scope")));
    }

    [TestMethod]
    public void RejectsNullRequiredCapability()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new NotEvaluatedSupport(
                new TenantContext("synthetic-tenant"),
                null!,
                new CapabilityObservationContext(
                    CapabilityState.Observed,
                    ObservationCompleteness.Complete),
                new SupportText("synthetic-evaluation-scope")));
    }

    [TestMethod]
    public void RejectsNullObservationContext()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new NotEvaluatedSupport(
                new TenantContext("synthetic-tenant"),
                new SupportText("synthetic-required-capability"),
                null!,
                new SupportText("synthetic-evaluation-scope")));
    }

    [TestMethod]
    public void RejectsNullEvaluationScope()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new NotEvaluatedSupport(
                new TenantContext("synthetic-tenant"),
                new SupportText("synthetic-required-capability"),
                new CapabilityObservationContext(
                    CapabilityState.Observed,
                    ObservationCompleteness.Complete),
                null!));
    }

    [TestMethod]
    public void EqualValuesProduceEqualSupport()
    {
        NotEvaluatedSupport left = CreateSupport();
        NotEvaluatedSupport right = CreateSupport();

        Assert.AreEqual(left, right);
    }

    [TestMethod]
    public void DoesNotContainAssessmentState()
    {
        Assert.IsFalse(
            typeof(NotEvaluatedSupport).GetProperties()
                .Any(property => property.PropertyType == typeof(AssessmentState)));

        Assert.IsFalse(
            typeof(NotEvaluatedSupport).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field => field.FieldType == typeof(AssessmentState)));

        Assert.IsFalse(
            typeof(NotEvaluatedSupport).GetMethods()
                .Where(method => method.DeclaringType == typeof(NotEvaluatedSupport))
                .Any(method =>
                    method.ReturnType == typeof(AssessmentState) ||
                    method.GetParameters().Any(parameter =>
                        parameter.ParameterType == typeof(AssessmentState))));
    }

    [TestMethod]
    public void DoesNotContainVerdictSupportOrEvidenceReference()
    {
        Assert.IsFalse(
            typeof(NotEvaluatedSupport).GetProperties()
                .Any(property =>
                    property.PropertyType == typeof(VerdictSupport) ||
                    property.PropertyType == typeof(EvidenceReference)));

        Assert.IsFalse(
            typeof(NotEvaluatedSupport).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field =>
                    field.FieldType == typeof(VerdictSupport) ||
                    field.FieldType == typeof(EvidenceReference)));

        Assert.IsFalse(
            typeof(NotEvaluatedSupport).GetMethods()
                .Where(method => method.DeclaringType == typeof(NotEvaluatedSupport))
                .Any(method =>
                    method.ReturnType == typeof(VerdictSupport) ||
                    method.ReturnType == typeof(EvidenceReference) ||
                    method.GetParameters().Any(parameter =>
                        parameter.ParameterType == typeof(VerdictSupport) ||
                        parameter.ParameterType == typeof(EvidenceReference))));
    }

    [TestMethod]
    public void DoesNotCalculatePassPreconditionDisposition()
    {
        Assert.IsFalse(
            typeof(NotEvaluatedSupport).GetProperties()
                .Any(property => property.PropertyType == typeof(PassPreconditionDisposition)));

        Assert.IsFalse(
            typeof(NotEvaluatedSupport).GetMethods()
                .Where(method => method.DeclaringType == typeof(NotEvaluatedSupport))
                .Any(method =>
                    method.ReturnType == typeof(PassPreconditionDisposition) ||
                    method.GetParameters().Any(parameter =>
                        parameter.ParameterType == typeof(PassPreconditionDisposition))));
    }

    [TestMethod]
    public void DoesNotIntroduceAnotherAssessmentState()
    {
        Assert.HasCount(5, Enum.GetValues<AssessmentState>());
    }
}
