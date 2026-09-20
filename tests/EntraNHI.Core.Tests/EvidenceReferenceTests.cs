namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class EvidenceReferenceTests
{
    [TestMethod]
    public void ExposesTenantContextEvidenceKeyAndProvenanceReference()
    {
        var tenant = new TenantContext("synthetic-tenant");
        var key = new EvidenceKey("synthetic-evidence-key");
        var provenance = new ProvenanceReference("synthetic-provenance");

        var reference = new EvidenceReference(tenant, key, provenance);

        Assert.AreEqual(tenant, reference.TenantContext);
        Assert.AreEqual(key, reference.EvidenceKey);
        Assert.AreEqual(provenance, reference.ProvenanceReference);
    }

    [TestMethod]
    public void EqualValuesProduceEqualReferences()
    {
        var left = new EvidenceReference(
            new TenantContext("synthetic-tenant"),
            new EvidenceKey("synthetic-evidence-key"),
            new ProvenanceReference("synthetic-provenance"));

        var right = new EvidenceReference(
            new TenantContext("synthetic-tenant"),
            new EvidenceKey("synthetic-evidence-key"),
            new ProvenanceReference("synthetic-provenance"));

        Assert.AreEqual(left, right);
    }

    [TestMethod]
    public void SameKeysUnderDifferentTenantsProduceDifferentReferences()
    {
        var left = new EvidenceReference(
            new TenantContext("synthetic-tenant-a"),
            new EvidenceKey("synthetic-evidence-key"),
            new ProvenanceReference("synthetic-provenance"));

        var right = new EvidenceReference(
            new TenantContext("synthetic-tenant-b"),
            new EvidenceKey("synthetic-evidence-key"),
            new ProvenanceReference("synthetic-provenance"));

        Assert.AreNotEqual(left, right);
    }

    [TestMethod]
    public void DifferentEvidenceKeysProduceDifferentReferences()
    {
        var left = new EvidenceReference(
            new TenantContext("synthetic-tenant"),
            new EvidenceKey("synthetic-evidence-key-a"),
            new ProvenanceReference("synthetic-provenance"));

        var right = new EvidenceReference(
            new TenantContext("synthetic-tenant"),
            new EvidenceKey("synthetic-evidence-key-b"),
            new ProvenanceReference("synthetic-provenance"));

        Assert.AreNotEqual(left, right);
    }

    [TestMethod]
    public void DifferentProvenanceReferencesProduceDifferentReferences()
    {
        var left = new EvidenceReference(
            new TenantContext("synthetic-tenant"),
            new EvidenceKey("synthetic-evidence-key"),
            new ProvenanceReference("synthetic-provenance-a"));

        var right = new EvidenceReference(
            new TenantContext("synthetic-tenant"),
            new EvidenceKey("synthetic-evidence-key"),
            new ProvenanceReference("synthetic-provenance-b"));

        Assert.AreNotEqual(left, right);
    }

    [TestMethod]
    public void RejectsNullTenantContext()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new EvidenceReference(
                null!,
                new EvidenceKey("synthetic-evidence-key"),
                new ProvenanceReference("synthetic-provenance")));
    }

    [TestMethod]
    public void RejectsNullEvidenceKey()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new EvidenceReference(
                new TenantContext("synthetic-tenant"),
                null!,
                new ProvenanceReference("synthetic-provenance")));
    }

    [TestMethod]
    public void RejectsNullProvenanceReference()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new EvidenceReference(
                new TenantContext("synthetic-tenant"),
                new EvidenceKey("synthetic-evidence-key"),
                null!));
    }

    [TestMethod]
    public void EvidenceReferenceIsAReferenceType()
    {
        Assert.IsFalse(typeof(EvidenceReference).IsValueType);
    }

    [TestMethod]
    public void DoesNotIntroduceAnotherAssessmentState()
    {
        Assert.HasCount(5, Enum.GetValues<AssessmentState>());
    }
}
