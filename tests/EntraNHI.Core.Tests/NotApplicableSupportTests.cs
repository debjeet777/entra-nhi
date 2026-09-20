using System.Reflection;

namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class NotApplicableSupportTests
{
    private static NormalizedIdentityReference CreateSubject(string tenant = "synthetic-tenant")
    {
        return new NormalizedIdentityReference(
            new TenantContext(tenant),
            new NormalizedIdentityKey("synthetic-identity-key"));
    }

    private static NotApplicableSupport CreateSupport(
        TenantContext? tenantContext = null,
        NormalizedIdentityReference? subject = null,
        SupportText? assessmentContext = null,
        SupportText? applicabilityCriterion = null)
    {
        return new NotApplicableSupport(
            tenantContext ?? new TenantContext("synthetic-tenant"),
            subject ?? CreateSubject(),
            assessmentContext ?? new SupportText("synthetic-assessment-context"),
            applicabilityCriterion ?? new SupportText("synthetic-applicability-criterion"));
    }

    [TestMethod]
    public void ExposesTenantContextSubjectAssessmentContextAndCriterion()
    {
        var tenantContext = new TenantContext("synthetic-tenant");
        NormalizedIdentityReference subject = CreateSubject();
        var assessmentContext = new SupportText("synthetic-assessment-context");
        var applicabilityCriterion = new SupportText("synthetic-applicability-criterion");

        var support = new NotApplicableSupport(
            tenantContext,
            subject,
            assessmentContext,
            applicabilityCriterion);

        Assert.AreSame(tenantContext, support.TenantContext);
        Assert.AreEqual(subject, support.Subject);
        Assert.AreEqual(assessmentContext, support.AssessmentContext);
        Assert.AreEqual(applicabilityCriterion, support.ApplicabilityCriterion);
    }

    [TestMethod]
    public void RetainsTenantContextExplicitly()
    {
        var tenantContext = new TenantContext("synthetic-tenant");

        NotApplicableSupport support = CreateSupport(tenantContext: tenantContext);

        Assert.AreSame(tenantContext, support.TenantContext);
    }

    [TestMethod]
    public void RejectsNullTenantContext()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new NotApplicableSupport(
                null!,
                CreateSubject(),
                new SupportText("synthetic-assessment-context"),
                new SupportText("synthetic-applicability-criterion")));
    }

    [TestMethod]
    public void RejectsNullSubject()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new NotApplicableSupport(
                new TenantContext("synthetic-tenant"),
                null!,
                new SupportText("synthetic-assessment-context"),
                new SupportText("synthetic-applicability-criterion")));
    }

    [TestMethod]
    public void RejectsNullAssessmentContext()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new NotApplicableSupport(
                new TenantContext("synthetic-tenant"),
                CreateSubject(),
                null!,
                new SupportText("synthetic-applicability-criterion")));
    }

    [TestMethod]
    public void RejectsNullApplicabilityCriterion()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new NotApplicableSupport(
                new TenantContext("synthetic-tenant"),
                CreateSubject(),
                new SupportText("synthetic-assessment-context"),
                null!));
    }

    [TestMethod]
    public void EqualValuesProduceEqualSupport()
    {
        NotApplicableSupport left = CreateSupport();
        NotApplicableSupport right = CreateSupport();

        Assert.AreEqual(left, right);
    }

    [TestMethod]
    public void DoesNotContainAssessmentState()
    {
        Assert.IsFalse(
            typeof(NotApplicableSupport).GetProperties()
                .Any(property => property.PropertyType == typeof(AssessmentState)));

        Assert.IsFalse(
            typeof(NotApplicableSupport).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field => field.FieldType == typeof(AssessmentState)));

        Assert.IsFalse(
            typeof(NotApplicableSupport).GetMethods()
                .Where(method => method.DeclaringType == typeof(NotApplicableSupport))
                .Any(method =>
                    method.ReturnType == typeof(AssessmentState) ||
                    method.GetParameters().Any(parameter =>
                        parameter.ParameterType == typeof(AssessmentState))));
    }

    [TestMethod]
    public void DoesNotContainVerdictSupportOrEvidenceReference()
    {
        Assert.IsFalse(
            typeof(NotApplicableSupport).GetProperties()
                .Any(property =>
                    property.PropertyType == typeof(VerdictSupport) ||
                    property.PropertyType == typeof(EvidenceReference)));

        Assert.IsFalse(
            typeof(NotApplicableSupport).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field =>
                    field.FieldType == typeof(VerdictSupport) ||
                    field.FieldType == typeof(EvidenceReference)));

        Assert.IsFalse(
            typeof(NotApplicableSupport).GetMethods()
                .Where(method => method.DeclaringType == typeof(NotApplicableSupport))
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
            typeof(NotApplicableSupport).GetProperties()
                .Any(property => property.PropertyType == typeof(PassPreconditionDisposition)));

        Assert.IsFalse(
            typeof(NotApplicableSupport).GetMethods()
                .Where(method => method.DeclaringType == typeof(NotApplicableSupport))
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
