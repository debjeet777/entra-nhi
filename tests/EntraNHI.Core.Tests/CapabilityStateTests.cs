namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class CapabilityStateTests
{
    [TestMethod]
    public void DefinesExactlyTheSevenCanonicalCapabilityStates()
    {
        string[] expected =
        [
            nameof(CapabilityState.Observed),
            nameof(CapabilityState.NotRequested),
            nameof(CapabilityState.UnavailableAuthorization),
            nameof(CapabilityState.UnavailableLicensingService),
            nameof(CapabilityState.Unsupported),
            nameof(CapabilityState.Failed),
            nameof(CapabilityState.Unknown)
        ];

        CollectionAssert.AreEqual(expected, Enum.GetNames<CapabilityState>());
    }

    [TestMethod]
    public void DefinesExactlySevenCapabilityStates()
    {
        Assert.HasCount(7, Enum.GetValues<CapabilityState>());
    }


    [TestMethod]
    public void NonObservedConditionsRemainDistinct()
    {
        CapabilityState[] states =
        [
            CapabilityState.NotRequested,
            CapabilityState.UnavailableAuthorization,
            CapabilityState.UnavailableLicensingService,
            CapabilityState.Unsupported,
            CapabilityState.Failed,
            CapabilityState.Unknown
        ];

        Assert.HasCount(
            states.Length,
            states.Distinct().ToArray());
    }

    [TestMethod]
    public void CapabilityStateDoesNotIntroduceAnotherAssessmentState()
    {
        Assert.HasCount(5, Enum.GetValues<AssessmentState>());
    }
}