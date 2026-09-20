namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class VerdictSupportTests
{
    private static EvidenceReference CreateEvidenceReference(
        string tenant = "synthetic-tenant",
        string key = "synthetic-evidence-key",
        string provenance = "synthetic-provenance")
    {
        return new EvidenceReference(
            new TenantContext(tenant),
            new EvidenceKey(key),
            new ProvenanceReference(provenance));
    }

    [TestMethod]
    public void ExposesEvidenceReferencesInOrder()
    {
        EvidenceReference first = CreateEvidenceReference(key: "synthetic-evidence-key-a");
        EvidenceReference second = CreateEvidenceReference(key: "synthetic-evidence-key-b");

        var support = new VerdictSupport([first, second]);

        Assert.HasCount(2, [.. support.Evidence]);
        Assert.AreEqual(first, support.Evidence[0]);
        Assert.AreEqual(second, support.Evidence[1]);
    }

    [TestMethod]
    public void AcceptsSingleEvidenceReference()
    {
        EvidenceReference reference = CreateEvidenceReference();

        var support = new VerdictSupport([reference]);

        Assert.HasCount(1, [.. support.Evidence]);
        Assert.AreEqual(reference, support.Evidence[0]);
    }

    [TestMethod]
    public void RejectsNullCollection()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new VerdictSupport(null!));
    }

    [TestMethod]
    public void RejectsEmptyCollection()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new VerdictSupport([]));
    }

    [TestMethod]
    public void RejectsEmptyEnumerable()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new VerdictSupport(Enumerable.Empty<EvidenceReference>()));
    }

    [TestMethod]
    public void RejectsNullEvidenceElement()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new VerdictSupport([CreateEvidenceReference(), null!]));
    }

    [TestMethod]
    public void RejectsOnlyNullEvidenceElement()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new VerdictSupport([null!]));
    }

    [TestMethod]
    public void CallerMutationOfSourceCollectionCannotChangeConstructedSupport()
    {
        var source = new List<EvidenceReference>
        {
            CreateEvidenceReference(key: "synthetic-evidence-key-a")
        };

        var support = new VerdictSupport(source);

        source.Add(CreateEvidenceReference(key: "synthetic-evidence-key-b"));
        source.Clear();

        Assert.HasCount(1, [.. support.Evidence]);
        Assert.AreEqual(
            new EvidenceKey("synthetic-evidence-key-a"),
            support.Evidence[0].EvidenceKey);
    }

    [TestMethod]
    public void CallerMutationOfSourceArrayCannotChangeConstructedSupport()
    {
        EvidenceReference[] source = [CreateEvidenceReference(key: "synthetic-evidence-key-a")];

        var support = new VerdictSupport(source);

        source[0] = CreateEvidenceReference(key: "synthetic-evidence-key-b");

        Assert.HasCount(1, [.. support.Evidence]);
        Assert.AreEqual(
            new EvidenceKey("synthetic-evidence-key-a"),
            support.Evidence[0].EvidenceKey);
    }

    [TestMethod]
    public void ExposedEvidenceIsReadOnly()
    {
        var support = new VerdictSupport([CreateEvidenceReference()]);

        Assert.IsTrue(((IList<EvidenceReference>)support.Evidence).IsReadOnly);
    }

    [TestMethod]
    public void DoesNotContainAssessmentState()
    {
        Assert.IsFalse(
            typeof(VerdictSupport).GetProperties()
                .Any(property => property.PropertyType == typeof(AssessmentState)));

        Assert.IsFalse(
            typeof(VerdictSupport).GetMethods()
                .Where(method => method.DeclaringType == typeof(VerdictSupport))
                .Any(method =>
                    method.ReturnType == typeof(AssessmentState) ||
                    method.GetParameters().Any(parameter =>
                        parameter.ParameterType == typeof(AssessmentState))));
    }

    [TestMethod]
    public void DoesNotIntroduceAnotherAssessmentState()
    {
        Assert.HasCount(5, Enum.GetValues<AssessmentState>());
    }
}
