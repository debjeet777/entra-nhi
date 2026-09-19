namespace EntraNHI.Core;

/// <summary>
/// Identifies a normalized identity record within one tenant assessment context.
/// </summary>
/// <remarks>
/// The value is intentionally opaque and provider-independent.
/// This type does not define identifier format, generation, or provider mapping.
/// </remarks>
public sealed record NormalizedIdentityKey
{
    public NormalizedIdentityKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Normalized identity key must contain a non-empty opaque value.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }
}
