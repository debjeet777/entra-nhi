namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class TenantContextTests
{
    [TestMethod]
    public void EqualOpaqueValuesProduceEqualTenantContexts()
    {
        TenantContext first = new("synthetic-tenant-a");
        TenantContext second = new("synthetic-tenant-a");

        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public void DifferentOpaqueValuesProduceDifferentTenantContexts()
    {
        TenantContext first = new("synthetic-tenant-a");
        TenantContext second = new("synthetic-tenant-b");

        Assert.AreNotEqual(first, second);
    }

    [TestMethod]
    public void EmptyOpaqueValueIsRejected()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new TenantContext(""));
    }

    [TestMethod]
    public void WhitespaceOpaqueValueIsRejected()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new TenantContext("   "));
    }

    [TestMethod]
    public void OpaqueValueIsPreservedWithoutProviderInterpretation()
    {
        const string opaqueValue = "synthetic-context::not-a-provider-id";

        TenantContext context = new(opaqueValue);

        Assert.AreEqual(opaqueValue, context.Value);
    }

    [TestMethod]
    public void TenantContextIsAReferenceType()
    {
        Assert.IsFalse(typeof(TenantContext).IsValueType);
    }

    [TestMethod]
    public void TenantContextCannotBeDefaultConstructed()
    {
        Assert.IsNull(typeof(TenantContext).GetConstructor(Type.EmptyTypes));
    }
}
