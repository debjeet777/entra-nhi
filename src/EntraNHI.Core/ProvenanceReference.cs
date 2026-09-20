namespace EntraNHI.Core;

/// <summary>
/// References the collected source observation associated with evidence.
/// </summary>
/// <remarks>
/// This type represents a reference only. It does not define a detailed
/// provenance schema, provider payload, provider SDK type, serialization,
/// persistence, hashing, or signing.
/// </remarks>
public sealed record ProvenanceReference
{
    public ProvenanceReference(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Provenance reference must contain a non-empty opaque value.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }
}
