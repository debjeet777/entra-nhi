namespace EntraNHI.Core;

/// <summary>
/// References normalized assessment evidence within an explicit tenant
/// assessment context.
/// </summary>
/// <remarks>
/// Equality is defined by tenant context plus evidence key plus provenance
/// reference. This type is provider-independent and does not carry
/// presentation text, provider identifiers, raw payloads, timestamps,
/// severity, or serialization members.
/// </remarks>
public sealed record EvidenceReference
{
    public EvidenceReference(
        TenantContext tenantContext,
        EvidenceKey evidenceKey,
        ProvenanceReference provenanceReference)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);
        ArgumentNullException.ThrowIfNull(evidenceKey);
        ArgumentNullException.ThrowIfNull(provenanceReference);

        TenantContext = tenantContext;
        EvidenceKey = evidenceKey;
        ProvenanceReference = provenanceReference;
    }

    public TenantContext TenantContext { get; }

    public EvidenceKey EvidenceKey { get; }

    public ProvenanceReference ProvenanceReference { get; }
}
