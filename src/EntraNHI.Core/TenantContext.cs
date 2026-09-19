namespace EntraNHI.Core;

/// <summary>
/// Identifies the explicit tenant assessment context for tenant-scoped Core data.
/// </summary>
/// <remarks>
/// The value is intentionally opaque and provider-independent.
/// It does not define a provider tenant-identifier format or generation strategy.
/// </remarks>
public sealed record TenantContext
{
    public TenantContext(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Tenant context must contain a non-empty opaque value.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }
}
