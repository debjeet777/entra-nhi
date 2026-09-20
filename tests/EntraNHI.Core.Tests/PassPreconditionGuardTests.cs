using System.Reflection;

namespace EntraNHI.Core.Tests;

[TestClass]
public sealed class PassPreconditionGuardTests
{
    [TestMethod]
    public void ObservedCompleteIsSatisfied()
    {
        Assert.AreEqual(
            PassPreconditionDisposition.Satisfied,
            PassPreconditionGuard.Evaluate(
                CapabilityState.Observed,
                ObservationCompleteness.Complete));
    }

    [TestMethod]
    public void ObservedPartialRequiresRuleSpecificSufficiency()
    {
        Assert.AreEqual(
            PassPreconditionDisposition.RequiresRuleSpecificSufficiency,
            PassPreconditionGuard.Evaluate(
                CapabilityState.Observed,
                ObservationCompleteness.Partial));
    }

    [TestMethod]
    public void ObservedUnknownIsInsufficient()
    {
        Assert.AreEqual(
            PassPreconditionDisposition.Insufficient,
            PassPreconditionGuard.Evaluate(
                CapabilityState.Observed,
                ObservationCompleteness.Unknown));
    }

    [TestMethod]
    public void EveryNonObservedCapabilityWithEveryCompletenessIsInsufficient()
    {
        CapabilityState[] nonObservedStates =
            Enum.GetValues<CapabilityState>()
                .Where(state => state != CapabilityState.Observed)
                .ToArray();

        foreach (CapabilityState capabilityState in nonObservedStates)
        {
            foreach (ObservationCompleteness completeness
                     in Enum.GetValues<ObservationCompleteness>())
            {
                Assert.AreEqual(
                    PassPreconditionDisposition.Insufficient,
                    PassPreconditionGuard.Evaluate(
                        capabilityState,
                        completeness));
            }
        }
    }

    [TestMethod]
    public void OnlyObservedCompleteIsSatisfied()
    {
        foreach (CapabilityState capabilityState
                 in Enum.GetValues<CapabilityState>())
        {
            foreach (ObservationCompleteness completeness
                     in Enum.GetValues<ObservationCompleteness>())
            {
                bool expected =
                    capabilityState == CapabilityState.Observed &&
                    completeness == ObservationCompleteness.Complete;

                Assert.AreEqual(
                    expected,
                    PassPreconditionGuard.Evaluate(
                        capabilityState,
                        completeness) ==
                    PassPreconditionDisposition.Satisfied);
            }
        }
    }

    [TestMethod]
    public void OnlyObservedPartialRequiresRuleSpecificSufficiency()
    {
        foreach (CapabilityState capabilityState
                 in Enum.GetValues<CapabilityState>())
        {
            foreach (ObservationCompleteness completeness
                     in Enum.GetValues<ObservationCompleteness>())
            {
                bool expected =
                    capabilityState == CapabilityState.Observed &&
                    completeness == ObservationCompleteness.Partial;

                Assert.AreEqual(
                    expected,
                    PassPreconditionGuard.Evaluate(
                        capabilityState,
                        completeness) ==
                    PassPreconditionDisposition.RequiresRuleSpecificSufficiency);
            }
        }
    }

    [TestMethod]
    public void EvaluateHasExactlyTheApprovedPublicContract()
    {
        MethodInfo? method = typeof(PassPreconditionGuard).GetMethod(
            nameof(PassPreconditionGuard.Evaluate),
            [
                typeof(CapabilityState),
                typeof(ObservationCompleteness)
            ]);

        Assert.IsNotNull(method);
        Assert.AreEqual(
            typeof(PassPreconditionDisposition),
            method.ReturnType);

        ParameterInfo[] parameters = method.GetParameters();

        Assert.HasCount(2, parameters);
        Assert.AreEqual(typeof(CapabilityState), parameters[0].ParameterType);
        Assert.AreEqual(
            typeof(ObservationCompleteness),
            parameters[1].ParameterType);
    }

    [TestMethod]
    public void GuardDoesNotAcceptOperationalFailureCategory()
    {
        MethodInfo[] methods =
            typeof(PassPreconditionGuard).GetMethods(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly);

        Assert.IsFalse(
            methods
                .SelectMany(method => method.GetParameters())
                .Any(parameter =>
                    parameter.ParameterType ==
                    typeof(OperationalFailureCategory)));
    }

    [TestMethod]
    public void InvalidCapabilityStateIsRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            PassPreconditionGuard.Evaluate(
                (CapabilityState)int.MaxValue,
                ObservationCompleteness.Complete));
    }

    [TestMethod]
    public void InvalidObservationCompletenessIsRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            PassPreconditionGuard.Evaluate(
                CapabilityState.Observed,
                (ObservationCompleteness)int.MaxValue));
    }}
