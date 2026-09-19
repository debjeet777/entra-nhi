namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class DomainValueTests
{
    [TestMethod]
    public void PresentPreservesSuppliedValue()
    {
        DomainValue<string> value = DomainValue<string>.Present("synthetic-value");

        Assert.IsTrue(value.IsPresent);
        Assert.AreEqual("synthetic-value", value.Value);
    }

    [TestMethod]
    public void PresentIsDistinguishableFromNotPresentlyKnown()
    {
        DomainValue<string> present = DomainValue<string>.Present("synthetic-value");
        DomainValue<string> unknown = DomainValue<string>.NotPresentlyKnown();

        Assert.IsTrue(present.IsPresent);
        Assert.IsFalse(unknown.IsPresent);
    }

    [TestMethod]
    public void NotPresentlyKnownDoesNotExposeFabricatedValue()
    {
        DomainValue<string> unknown = DomainValue<string>.NotPresentlyKnown();

        Assert.ThrowsExactly<InvalidOperationException>(() =>
        {
            _ = unknown.Value;
        });
    }

    [TestMethod]
    public void PresentRejectsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            DomainValue<string>.Present(null!));
    }

    [TestMethod]
    public void DomainValueIsAReferenceType()
    {
        Assert.IsFalse(typeof(DomainValue<string>).IsValueType);
    }

    [TestMethod]
    public void ArbitraryConstructionIsNotPubliclyAvailable()
    {
        Assert.HasCount(0, typeof(DomainValue<string>).GetConstructors());
    }

    [TestMethod]
    public void NullableValueTypeCanBePresentWhenItContainsAValue()
    {
        DomainValue<int?> value = DomainValue<int?>.Present(42);

        Assert.IsTrue(value.IsPresent);
        Assert.AreEqual(42, value.Value);
    }

    [TestMethod]
    public void NullableValueTypeRejectsPresentNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            DomainValue<int?>.Present(null));
    }

    [TestMethod]
    public void DoesNotIntroduceAnotherAssessmentState()
    {
        Assert.HasCount(5, Enum.GetValues<AssessmentState>());
    }
}
