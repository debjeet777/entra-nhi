using System.Reflection;

namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class ErrorSupportTests
{
    private static ErrorSupport CreateSupport(
        TenantContext? tenantContext = null,
        OperationalFailureCategory failureCategory = OperationalFailureCategory.Timeout,
        SupportText? evaluationScope = null,
        SupportText? diagnosticContext = null)
    {
        return new ErrorSupport(
            tenantContext ?? new TenantContext("synthetic-tenant"),
            failureCategory,
            evaluationScope ?? new SupportText("synthetic-evaluation-scope"),
            diagnosticContext ?? new SupportText("synthetic-diagnostic-context"));
    }

    [TestMethod]
    public void ExposesTenantContextFailureCategoryScopeAndDiagnosticContext()
    {
        var tenantContext = new TenantContext("synthetic-tenant");
        var evaluationScope = new SupportText("synthetic-evaluation-scope");
        var diagnosticContext = new SupportText("synthetic-diagnostic-context");

        var support = new ErrorSupport(
            tenantContext,
            OperationalFailureCategory.Timeout,
            evaluationScope,
            diagnosticContext);

        Assert.AreSame(tenantContext, support.TenantContext);
        Assert.AreEqual(OperationalFailureCategory.Timeout, support.FailureCategory);
        Assert.AreEqual(evaluationScope, support.EvaluationScope);
        Assert.AreEqual(diagnosticContext, support.DiagnosticContext);
    }

    [TestMethod]
    public void RetainsTenantContextExplicitly()
    {
        var tenantContext = new TenantContext("synthetic-tenant");

        ErrorSupport support = CreateSupport(tenantContext: tenantContext);

        Assert.AreSame(tenantContext, support.TenantContext);
    }

    [TestMethod]
    public void AcceptsEveryCanonicalFailureCategory()
    {
        foreach (OperationalFailureCategory category in Enum.GetValues<OperationalFailureCategory>())
        {
            ErrorSupport support = CreateSupport(failureCategory: category);

            Assert.AreEqual(category, support.FailureCategory);
        }
    }

    [TestMethod]
    public void RejectsUndefinedFailureCategory()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            CreateSupport(failureCategory: (OperationalFailureCategory)int.MaxValue));
    }

    [TestMethod]
    public void RejectsNegativeFailureCategory()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            CreateSupport(failureCategory: (OperationalFailureCategory)(-1)));
    }

    [TestMethod]
    public void RejectsNullTenantContext()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new ErrorSupport(
                null!,
                OperationalFailureCategory.Timeout,
                new SupportText("synthetic-evaluation-scope"),
                new SupportText("synthetic-diagnostic-context")));
    }

    [TestMethod]
    public void RejectsNullEvaluationScope()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new ErrorSupport(
                new TenantContext("synthetic-tenant"),
                OperationalFailureCategory.Timeout,
                null!,
                new SupportText("synthetic-diagnostic-context")));
    }

    [TestMethod]
    public void RejectsNullDiagnosticContext()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new ErrorSupport(
                new TenantContext("synthetic-tenant"),
                OperationalFailureCategory.Timeout,
                new SupportText("synthetic-evaluation-scope"),
                null!));
    }

    [TestMethod]
    public void EqualValuesProduceEqualSupport()
    {
        ErrorSupport left = CreateSupport();
        ErrorSupport right = CreateSupport();

        Assert.AreEqual(left, right);
    }

    [TestMethod]
    public void DoesNotContainAssessmentState()
    {
        Assert.IsFalse(
            typeof(ErrorSupport).GetProperties()
                .Any(property => property.PropertyType == typeof(AssessmentState)));

        Assert.IsFalse(
            typeof(ErrorSupport).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field => field.FieldType == typeof(AssessmentState)));

        Assert.IsFalse(
            typeof(ErrorSupport).GetMethods()
                .Where(method => method.DeclaringType == typeof(ErrorSupport))
                .Any(method =>
                    method.ReturnType == typeof(AssessmentState) ||
                    method.GetParameters().Any(parameter =>
                        parameter.ParameterType == typeof(AssessmentState))));
    }

    [TestMethod]
    public void DoesNotContainVerdictSupportOrEvidenceReference()
    {
        Assert.IsFalse(
            typeof(ErrorSupport).GetProperties()
                .Any(property =>
                    property.PropertyType == typeof(VerdictSupport) ||
                    property.PropertyType == typeof(EvidenceReference)));

        Assert.IsFalse(
            typeof(ErrorSupport).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field =>
                    field.FieldType == typeof(VerdictSupport) ||
                    field.FieldType == typeof(EvidenceReference)));

        Assert.IsFalse(
            typeof(ErrorSupport).GetMethods()
                .Where(method => method.DeclaringType == typeof(ErrorSupport))
                .Any(method =>
                    method.ReturnType == typeof(VerdictSupport) ||
                    method.ReturnType == typeof(EvidenceReference) ||
                    method.GetParameters().Any(parameter =>
                        parameter.ParameterType == typeof(VerdictSupport) ||
                        parameter.ParameterType == typeof(EvidenceReference))));
    }

    [TestMethod]
    public void DoesNotExposeExceptionContract()
    {
        Assert.IsFalse(
            typeof(ErrorSupport).GetProperties()
                .Any(property => typeof(Exception).IsAssignableFrom(property.PropertyType)));

        Assert.IsFalse(
            typeof(ErrorSupport).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field => typeof(Exception).IsAssignableFrom(field.FieldType)));

        Assert.IsFalse(
            typeof(ErrorSupport).GetMethods()
                .Where(method => method.DeclaringType == typeof(ErrorSupport))
                .Any(method =>
                    typeof(Exception).IsAssignableFrom(method.ReturnType) ||
                    method.GetParameters().Any(parameter =>
                        typeof(Exception).IsAssignableFrom(parameter.ParameterType))));

        Assert.IsFalse(
            typeof(ErrorSupport).GetConstructors()
                .SelectMany(constructor => constructor.GetParameters())
                .Any(parameter => typeof(Exception).IsAssignableFrom(parameter.ParameterType)));
    }

    [TestMethod]
    public void DoesNotCalculatePassPreconditionDisposition()
    {
        Assert.IsFalse(
            typeof(ErrorSupport).GetProperties()
                .Any(property => property.PropertyType == typeof(PassPreconditionDisposition)));

        Assert.IsFalse(
            typeof(ErrorSupport).GetMethods()
                .Where(method => method.DeclaringType == typeof(ErrorSupport))
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
