namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class EvidenceKeyTests
{
    [TestMethod]
    public void PreservesValidOpaqueValueExactly()
    {
        const string opaqueValue = " synthetic:evidence/key-01 ";

        var key = new EvidenceKey(opaqueValue);

        Assert.AreEqual(opaqueValue, key.Value);
    }

    [TestMethod]
    public void RejectsNullValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new EvidenceKey(null!));
    }

    [TestMethod]
    public void RejectsEmptyValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new EvidenceKey(string.Empty));
    }

    [TestMethod]
    public void RejectsWhitespaceOnlyValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new EvidenceKey("   "));
    }

    [TestMethod]
    public void EqualOpaqueValuesProduceEqualKeys()
    {
        var left = new EvidenceKey("synthetic-evidence-key");
        var right = new EvidenceKey("synthetic-evidence-key");

        Assert.AreEqual(left, right);
    }

    [TestMethod]
    public void DifferentOpaqueValuesProduceDifferentKeys()
    {
        var left = new EvidenceKey("synthetic-evidence-key-a");
        var right = new EvidenceKey("synthetic-evidence-key-b");

        Assert.AreNotEqual(left, right);
    }

    [TestMethod]
    public void EvidenceKeyIsAReferenceType()
    {
        Assert.IsFalse(typeof(EvidenceKey).IsValueType);
    }

    [TestMethod]
    public void EvidenceKeyCannotBeDefaultConstructed()
    {
        Assert.IsNull(typeof(EvidenceKey).GetConstructor(Type.EmptyTypes));
    }

    [TestMethod]
    public void DoesNotIntroduceAnotherAssessmentState()
    {
        Assert.HasCount(5, Enum.GetValues<AssessmentState>());
    }
}
