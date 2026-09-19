namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class NormalizedIdentityKeyTests
{
    [TestMethod]
    public void PreservesValidOpaqueValueExactly()
    {
        const string opaqueValue = " synthetic:key/value-01 ";

        var key = new NormalizedIdentityKey(opaqueValue);

        Assert.AreEqual(opaqueValue, key.Value);
    }

    [TestMethod]
    public void RejectsNullValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new NormalizedIdentityKey(null!));
    }

    [TestMethod]
    public void RejectsEmptyValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new NormalizedIdentityKey(string.Empty));
    }

    [TestMethod]
    public void RejectsWhitespaceOnlyValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new NormalizedIdentityKey("   "));
    }

    [TestMethod]
    public void EqualOpaqueValuesProduceEqualKeys()
    {
        var left = new NormalizedIdentityKey("synthetic-key");
        var right = new NormalizedIdentityKey("synthetic-key");

        Assert.AreEqual(left, right);
    }

    [TestMethod]
    public void DifferentOpaqueValuesProduceDifferentKeys()
    {
        var left = new NormalizedIdentityKey("synthetic-key-a");
        var right = new NormalizedIdentityKey("synthetic-key-b");

        Assert.AreNotEqual(left, right);
    }

    [TestMethod]
    public void NormalizedIdentityKeyIsAReferenceType()
    {
        Assert.IsFalse(typeof(NormalizedIdentityKey).IsValueType);
    }

    [TestMethod]
    public void DoesNotIntroduceAnotherAssessmentState()
    {
        Assert.HasCount(5, Enum.GetValues<AssessmentState>());
    }
}
