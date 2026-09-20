namespace EntraNHI.Core;

/// <summary>
/// Represents how completely an applicable observation scope is known.
/// </summary>
/// <remarks>
/// Complete is affirmative knowledge and must not be inferred merely from
/// collection success, an empty result, a non-null value, or absence of error.
/// Content presence and capability state remain separate concepts.
/// </remarks>
public enum ObservationCompleteness
{
    Complete,
    Partial,
    Unknown
}