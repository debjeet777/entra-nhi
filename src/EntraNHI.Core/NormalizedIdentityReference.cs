namespace EntraNHI.Core;

/// <summary>
/// References a normalized identity within an explicit tenant assessment context.
/// </summary>
/// <remarks>
/// Equality is defined by tenant context plus normalized identity key.
/// This type is provider-independent and does not represent a source/native reference.
/// </remarks>
public sealed record NormalizedIdentityReference
{
    public NormalizedIdentityReference(
        TenantContext tenantContext,
        NormalizedIdentityKey key)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);
        ArgumentNullException.ThrowIfNull(key);

        TenantContext = tenantContext;
        Key = key;
    }

    public TenantContext TenantContext { get; }

    public NormalizedIdentityKey Key { get; }
}
