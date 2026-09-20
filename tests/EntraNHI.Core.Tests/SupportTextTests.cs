namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class SupportTextTests
{
    [TestMethod]
    public void PreservesValidOpaqueValueExactly()
    {
        const string opaqueValue = " synthetic:support-text-01 ";

        var text = new SupportText(opaqueValue);

        Assert.AreEqual(opaqueValue, text.Value);
    }

    [TestMethod]
    public void RejectsNullValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new SupportText(null!));
    }

    [TestMethod]
    public void RejectsEmptyValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new SupportText(string.Empty));
    }

    [TestMethod]
    public void RejectsWhitespaceOnlyValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new SupportText("   "));
    }

    [TestMethod]
    public void EqualOpaqueValuesProduceEqualTexts()
    {
        var left = new SupportText("synthetic-support-text");
        var right = new SupportText("synthetic-support-text");

        Assert.AreEqual(left, right);
    }

    [TestMethod]
    public void DifferentOpaqueValuesProduceDifferentTexts()
    {
        var left = new SupportText("synthetic-support-text-a");
        var right = new SupportText("synthetic-support-text-b");

        Assert.AreNotEqual(left, right);
    }

    [TestMethod]
    public void SupportTextIsAReferenceType()
    {
        Assert.IsFalse(typeof(SupportText).IsValueType);
    }

    [TestMethod]
    public void SupportTextCannotBeDefaultConstructed()
    {
        Assert.IsNull(typeof(SupportText).GetConstructor(Type.EmptyTypes));
    }

    [TestMethod]
    public void DoesNotContainAssessmentState()
    {
        Assert.IsFalse(
            typeof(SupportText).GetProperties()
                .Any(property => property.PropertyType == typeof(AssessmentState)));

        Assert.IsFalse(
            typeof(SupportText).GetMethods()
                .Where(method => method.DeclaringType == typeof(SupportText))
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
