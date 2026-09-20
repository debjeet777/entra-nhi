namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class ProvenanceReferenceTests
{
    [TestMethod]
    public void PreservesValidOpaqueValueExactly()
    {
        const string opaqueValue = " synthetic:provenance/ref-01 ";

        var reference = new ProvenanceReference(opaqueValue);

        Assert.AreEqual(opaqueValue, reference.Value);
    }

    [TestMethod]
    public void RejectsNullValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new ProvenanceReference(null!));
    }

    [TestMethod]
    public void RejectsEmptyValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new ProvenanceReference(string.Empty));
    }

    [TestMethod]
    public void RejectsWhitespaceOnlyValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new ProvenanceReference("   "));
    }

    [TestMethod]
    public void EqualOpaqueValuesProduceEqualReferences()
    {
        var left = new ProvenanceReference("synthetic-provenance");
        var right = new ProvenanceReference("synthetic-provenance");

        Assert.AreEqual(left, right);
    }

    [TestMethod]
    public void DifferentOpaqueValuesProduceDifferentReferences()
    {
        var left = new ProvenanceReference("synthetic-provenance-a");
        var right = new ProvenanceReference("synthetic-provenance-b");

        Assert.AreNotEqual(left, right);
    }

    [TestMethod]
    public void ProvenanceReferenceIsAReferenceType()
    {
        Assert.IsFalse(typeof(ProvenanceReference).IsValueType);
    }

    [TestMethod]
    public void ProvenanceReferenceCannotBeDefaultConstructed()
    {
        Assert.IsNull(typeof(ProvenanceReference).GetConstructor(Type.EmptyTypes));
    }

    [TestMethod]
    public void DoesNotIntroduceAnotherAssessmentState()
    {
        Assert.HasCount(5, Enum.GetValues<AssessmentState>());
    }
}
