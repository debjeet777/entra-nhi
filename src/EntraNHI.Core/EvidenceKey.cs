namespace EntraNHI.Core;

/// <summary>
/// Identifies normalized assessment evidence referenced by verdict support.
/// </summary>
/// <remarks>
/// The value is intentionally opaque and provider-independent.
/// This type does not define identifier generation, provider semantics,
/// serialization, hashing, persistence, or presentation semantics.
/// </remarks>
public sealed record EvidenceKey
{
    public EvidenceKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Evidence key must contain a non-empty opaque value.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }
}
