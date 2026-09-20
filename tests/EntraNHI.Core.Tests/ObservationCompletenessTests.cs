namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class ObservationCompletenessTests
{
    [TestMethod]
    public void DefinesExactlyTheThreeCanonicalCompletenessStates()
    {
        string[] expected =
        [
            nameof(ObservationCompleteness.Complete),
            nameof(ObservationCompleteness.Partial),
            nameof(ObservationCompleteness.Unknown)
        ];

        CollectionAssert.AreEqual(
            expected,
            Enum.GetNames<ObservationCompleteness>());
    }

    [TestMethod]
    public void DefinesExactlyThreeCompletenessStates()
    {
        Assert.HasCount(
            3,
            Enum.GetValues<ObservationCompleteness>());
    }


    [TestMethod]
    public void CompletenessIsSeparateFromCapabilityState()
    {
        Assert.AreNotEqual(
            typeof(CapabilityState),
            typeof(ObservationCompleteness));
    }

    [TestMethod]
    public void CompletenessDoesNotDefineConfirmedEmptyState()
    {
        CollectionAssert.DoesNotContain(
            Enum.GetNames<ObservationCompleteness>(),
            "ConfirmedEmpty");
    }

    [TestMethod]
    public void CompletenessDoesNotIntroduceAnotherAssessmentState()
    {
        Assert.HasCount(5, Enum.GetValues<AssessmentState>());
    }
}