namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class AssessmentStateTests
{
    [TestMethod]
    public void DefinesExactlyTheFiveCanonicalAssessmentStates()
    {
        string[] expected =
        [
            nameof(AssessmentState.PASS),
            nameof(AssessmentState.FAIL),
            nameof(AssessmentState.NOT_EVALUATED),
            nameof(AssessmentState.NOT_APPLICABLE),
            nameof(AssessmentState.ERROR)
        ];

        CollectionAssert.AreEqual(expected, Enum.GetNames<AssessmentState>());
    }

    [TestMethod]
    public void DefinesExactlyFiveAssessmentStates()
    {
        Assert.HasCount(5, Enum.GetValues<AssessmentState>());
    }
}
