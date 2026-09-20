namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class CapabilityObservationContextTests
{
    [TestMethod]
    public void ExposesCapabilityStateAndCompleteness()
    {
        var context = new CapabilityObservationContext(
            CapabilityState.Observed,
            ObservationCompleteness.Complete);

        Assert.AreEqual(CapabilityState.Observed, context.CapabilityState);
        Assert.AreEqual(ObservationCompleteness.Complete, context.Completeness);
    }

    [TestMethod]
    public void EqualValuesProduceEqualContexts()
    {
        var left = new CapabilityObservationContext(
            CapabilityState.Observed,
            ObservationCompleteness.Complete);

        var right = new CapabilityObservationContext(
            CapabilityState.Observed,
            ObservationCompleteness.Complete);

        Assert.AreEqual(left, right);
    }

    [TestMethod]
    public void DifferentCapabilityStatesProduceDifferentContexts()
    {
        var left = new CapabilityObservationContext(
            CapabilityState.Observed,
            ObservationCompleteness.Complete);

        var right = new CapabilityObservationContext(
            CapabilityState.Failed,
            ObservationCompleteness.Complete);

        Assert.AreNotEqual(left, right);
    }

    [TestMethod]
    public void DifferentCompletenessValuesProduceDifferentContexts()
    {
        var left = new CapabilityObservationContext(
            CapabilityState.Observed,
            ObservationCompleteness.Complete);

        var right = new CapabilityObservationContext(
            CapabilityState.Observed,
            ObservationCompleteness.Partial);

        Assert.AreNotEqual(left, right);
    }

    [TestMethod]
    public void AcceptsEveryCanonicalCapabilityState()
    {
        foreach (CapabilityState state in Enum.GetValues<CapabilityState>())
        {
            var context = new CapabilityObservationContext(
                state,
                ObservationCompleteness.Complete);

            Assert.AreEqual(state, context.CapabilityState);
        }
    }

    [TestMethod]
    public void AcceptsEveryCanonicalCompletenessValue()
    {
        foreach (ObservationCompleteness completeness in Enum.GetValues<ObservationCompleteness>())
        {
            var context = new CapabilityObservationContext(
                CapabilityState.Observed,
                completeness);

            Assert.AreEqual(completeness, context.Completeness);
        }
    }

    [TestMethod]
    public void AcceptsUnknownCapabilityAndUnknownCompleteness()
    {
        var context = new CapabilityObservationContext(
            CapabilityState.Unknown,
            ObservationCompleteness.Unknown);

        Assert.AreEqual(CapabilityState.Unknown, context.CapabilityState);
        Assert.AreEqual(ObservationCompleteness.Unknown, context.Completeness);
    }

    [TestMethod]
    public void RejectsUndefinedCapabilityState()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new CapabilityObservationContext(
                (CapabilityState)int.MaxValue,
                ObservationCompleteness.Complete));
    }

    [TestMethod]
    public void RejectsUndefinedCompleteness()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new CapabilityObservationContext(
                CapabilityState.Observed,
                (ObservationCompleteness)int.MaxValue));
    }

    [TestMethod]
    public void CapabilityObservationContextIsAReferenceType()
    {
        Assert.IsFalse(typeof(CapabilityObservationContext).IsValueType);
    }

    [TestMethod]
    public void DoesNotSelectAssessmentState()
    {
        Assert.IsFalse(
            typeof(CapabilityObservationContext).GetProperties()
                .Any(property => property.PropertyType == typeof(AssessmentState)));

        Assert.IsFalse(
            typeof(CapabilityObservationContext).GetMethods()
                .Where(method => method.DeclaringType == typeof(CapabilityObservationContext))
                .Any(method =>
                    method.ReturnType == typeof(AssessmentState) ||
                    method.ReturnType == typeof(PassPreconditionDisposition)));
    }

    [TestMethod]
    public void DoesNotIntroduceAnotherAssessmentState()
    {
        Assert.HasCount(5, Enum.GetValues<AssessmentState>());
    }
}
