namespace EntraNHI.Core;

/// <summary>
/// Represents opaque provider-neutral semantic text supporting a non-verdict
/// evaluation outcome.
/// </summary>
/// <remarks>
/// This type carries reason, scope, criterion, or diagnostic semantics only as
/// an opaque value. It does not define a richer reason/schema model.
/// Construction validates presence only and does NOT perform secret
/// detection, sanitization, redaction, or any other content inspection.
/// Callers must not place secrets, raw provider payloads, authentication
/// material, or other secret-bearing values in this value.
/// </remarks>
public sealed record SupportText
{
    public SupportText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Support text must contain a non-empty opaque value.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }
}
