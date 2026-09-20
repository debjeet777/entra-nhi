namespace EntraNHI.Core;

/// <summary>
/// Carries the evidence references supporting a deterministic verdict.
/// </summary>
/// <remarks>
/// Verdict support contains at least one evidence reference. It does not
/// select PASS/FAIL and does not contain an assessment state.
/// </remarks>
public sealed class VerdictSupport
{
    private readonly EvidenceReference[] _evidence;

    public VerdictSupport(IEnumerable<EvidenceReference> evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        EvidenceReference[] snapshot = evidence.ToArray();

        if (snapshot.Length == 0)
        {
            throw new ArgumentException(
                "Verdict support must contain at least one evidence reference.",
                nameof(evidence));
        }

        if (Array.Exists(snapshot, static reference => reference is null))
        {
            throw new ArgumentException(
                "Verdict support must not contain null evidence references.",
                nameof(evidence));
        }

        _evidence = snapshot;
        Evidence = Array.AsReadOnly(_evidence);
    }

    public IReadOnlyList<EvidenceReference> Evidence { get; }
}
