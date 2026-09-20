namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class PassPreconditionDispositionTests
{
    [TestMethod]
    public void DefinesExactlyTheThreeApprovedDispositions()
    {
        string[] expected =
        [
            nameof(PassPreconditionDisposition.Satisfied),
            nameof(PassPreconditionDisposition.RequiresRuleSpecificSufficiency),
            nameof(PassPreconditionDisposition.Insufficient)
        ];

        CollectionAssert.AreEqual(
            expected,
            Enum.GetNames<PassPreconditionDisposition>());
    }

    [TestMethod]
    public void DefinesExactlyThreeDispositions()
    {
        Assert.HasCount(
            3,
            Enum.GetValues<PassPreconditionDisposition>());
    }

    [TestMethod]
    public void DispositionIsDistinctFromAssessmentState()
    {
        Assert.AreNotEqual(
            typeof(PassPreconditionDisposition),
            typeof(AssessmentState));
    }

    [TestMethod]
    public void DoesNotIntroduceAssessmentStateAliasesOrConfirmedEmpty()
    {
        string[] prohibited =
        [
            "Pass",
            "PASS",
            "Fail",
            "FAIL",
            "Error",
            "ERROR",
            "Unknown",
            "NotApplicable",
            "NOT_APPLICABLE",
            "NotEvaluated",
            "NOT_EVALUATED",
            "Success",
            "ConfirmedEmpty"
        ];

        string[] actual = Enum.GetNames<PassPreconditionDisposition>();

        foreach (string name in prohibited)
        {
            CollectionAssert.DoesNotContain(actual, name);
        }
    }
}
