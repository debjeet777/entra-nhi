using EntraNHI.Core;

namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class OperationalFailureCategoryTests
{
    [TestMethod]
    public void DefinesExactlyTheFourteenCanonicalCategories()
    {
        string[] expected =
        [
            nameof(OperationalFailureCategory.AuthorizationFailure),
            nameof(OperationalFailureCategory.AuthenticationBoundaryFailure),
            nameof(OperationalFailureCategory.ProviderServiceUnavailable),
            nameof(OperationalFailureCategory.ThrottlingRateLimitation),
            nameof(OperationalFailureCategory.Timeout),
            nameof(OperationalFailureCategory.NetworkTransportFailure),
            nameof(OperationalFailureCategory.MalformedInvalidProviderResponse),
            nameof(OperationalFailureCategory.NormalizationFailure),
            nameof(OperationalFailureCategory.PartialCollectionFailure),
            nameof(OperationalFailureCategory.ConfigurationFailure),
            nameof(OperationalFailureCategory.UnsupportedCapabilityOperation),
            nameof(OperationalFailureCategory.SerializationOutputFailure),
            nameof(OperationalFailureCategory.Cancellation),
            nameof(OperationalFailureCategory.InternalUnclassifiedOperationalFailure)
        ];

        CollectionAssert.AreEqual(
            expected,
            Enum.GetNames<OperationalFailureCategory>());
    }

    [TestMethod]
    public void DoesNotIntroduceUnknownOrSuccessCategory()
    {
        string[] names = Enum.GetNames<OperationalFailureCategory>();

        CollectionAssert.DoesNotContain(names, "Unknown");
        CollectionAssert.DoesNotContain(names, "Other");
        CollectionAssert.DoesNotContain(names, "None");
        CollectionAssert.DoesNotContain(names, "Success");
    }

    [TestMethod]
    public void RemainsSeparateFromAssessmentCapabilityAndCompleteness()
    {
        Assert.AreNotEqual(
            typeof(AssessmentState),
            typeof(OperationalFailureCategory));

        Assert.AreNotEqual(
            typeof(CapabilityState),
            typeof(OperationalFailureCategory));

        Assert.AreNotEqual(
            typeof(ObservationCompleteness),
            typeof(OperationalFailureCategory));
    }
}
