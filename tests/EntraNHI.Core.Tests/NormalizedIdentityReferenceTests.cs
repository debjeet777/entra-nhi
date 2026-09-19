namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class NormalizedIdentityReferenceTests
{
    [TestMethod]
    public void ExposesTenantContextAndNormalizedIdentityKey()
    {
        var tenant = new TenantContext("synthetic-tenant");
        var key = new NormalizedIdentityKey("synthetic-key");

        var reference = new NormalizedIdentityReference(tenant, key);

        Assert.AreEqual(tenant, reference.TenantContext);
        Assert.AreEqual(key, reference.Key);
    }

    [TestMethod]
    public void EqualTenantAndKeyProduceEqualReferences()
    {
        var left = new NormalizedIdentityReference(
            new TenantContext("synthetic-tenant"),
            new NormalizedIdentityKey("synthetic-key"));

        var right = new NormalizedIdentityReference(
            new TenantContext("synthetic-tenant"),
            new NormalizedIdentityKey("synthetic-key"));

        Assert.AreEqual(left, right);
    }

    [TestMethod]
    public void SameKeyUnderDifferentTenantsProducesDifferentReferences()
    {
        var left = new NormalizedIdentityReference(
            new TenantContext("synthetic-tenant-a"),
            new NormalizedIdentityKey("synthetic-key"));

        var right = new NormalizedIdentityReference(
            new TenantContext("synthetic-tenant-b"),
            new NormalizedIdentityKey("synthetic-key"));

        Assert.AreNotEqual(left, right);
    }

    [TestMethod]
    public void DifferentKeysUnderSameTenantProduceDifferentReferences()
    {
        var left = new NormalizedIdentityReference(
            new TenantContext("synthetic-tenant"),
            new NormalizedIdentityKey("synthetic-key-a"));

        var right = new NormalizedIdentityReference(
            new TenantContext("synthetic-tenant"),
            new NormalizedIdentityKey("synthetic-key-b"));

        Assert.AreNotEqual(left, right);
    }

    [TestMethod]
    public void RejectsNullTenantContext()
    {
        var key = new NormalizedIdentityKey("synthetic-key");

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new NormalizedIdentityReference(null!, key));
    }

    [TestMethod]
    public void RejectsNullNormalizedIdentityKey()
    {
        var tenant = new TenantContext("synthetic-tenant");

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new NormalizedIdentityReference(tenant, null!));
    }

    [TestMethod]
    public void NormalizedIdentityReferenceIsAReferenceType()
    {
        Assert.IsFalse(typeof(NormalizedIdentityReference).IsValueType);
    }

    [TestMethod]
    public void DoesNotIntroduceAnotherAssessmentState()
    {
        Assert.HasCount(5, Enum.GetValues<AssessmentState>());
    }
}
