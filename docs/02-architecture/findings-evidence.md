# Findings & Evidence Architecture

> **Status:** Phase 0.2.7
> **Date:** 2026-09-18

---

## Purpose

Define the enterprise-security architecture for EntraNHI findings, rule evaluations, evidence, provenance, diagnostics, and auditability. This architecture governs the transformation from deterministic rule evaluation output through canonical security data representations to output format adapters.

```
Normalized assessment state
        |
        v
Deterministic Rule Engine
        |
        v
RuleEvaluation
        |
        +--> Evidence References
        |
        +--> Finding (where emission policy requires)
        |
        v
Output Architecture
(JSON / SARIF / HTML / CLI / future interfaces)
```

The architecture must make security conclusions:

- explainable
- traceable
- reproducible where deterministic inputs are preserved
- provenance-aware
- resistant to evidence fabrication
- resistant to false PASS/FAIL
- safe for enterprise security data
- independent of presentation format

---

## 1. RuleEvaluation Is Authoritative

`RuleEvaluation` records the deterministic result produced by the rule engine.

Exactly these states exist:

- PASS
- FAIL
- NOT_EVALUATED
- NOT_APPLICABLE
- ERROR

Findings/evidence architecture MUST NOT:

- recompute rule logic
- change evaluation state
- upgrade/downgrade PASS/FAIL
- convert ERROR to FAIL
- convert NOT_EVALUATED to PASS
- infer a result from presentation/output behavior
- allow AI/LLM to modify evaluation state

`RuleEvaluation` remains authoritative. [INV-02, INV-05, INV-13, INV-14]

---

## 2. RuleEvaluation vs Finding

Keep these concepts distinct.

**RuleEvaluation:**
Machine-readable record of a rule evaluation for a target/context. Every evaluation state (PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, ERROR) must remain representable as a `RuleEvaluation` for auditability.

**Finding:**
Reportable security observation derived from an authoritative `RuleEvaluation` according to an explicit finding-emission policy.

Do NOT assume every `RuleEvaluation` becomes a `Finding`.

However, all evaluation states must remain representable and auditable even when no `Finding` is emitted.

Finding emission MUST NOT change the underlying evaluation state.

Exact finding-emission policy remains TBD.

---

## 3. Conceptual RuleEvaluation Contract

Define a conceptual `RuleEvaluation` containing enough information to support auditability, including conceptually:

- evaluation identifier
- assessment identifier/context reference
- tenant-context reference
- rule ID
- rule version
- target identity/context reference
- evaluation state (PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, ERROR)
- evaluation reason/context
- capability-state references relevant to evaluation
- evidence references
- provenance references
- deterministic configuration/rule-set reference
- assessment-time reference where relevant
- diagnostic reference where appropriate

Do NOT define concrete C# types.

Do NOT include secrets or raw authentication material.

---

## 4. Conceptual Finding Contract

Define a conceptual `Finding` containing security-reporting information such as:

- finding identifier
- assessment reference
- tenant-context reference
- originating `RuleEvaluation` reference
- rule ID/version
- target reference
- authoritative evaluation state
- title/summary
- security rationale
- severity metadata where applicable
- evidence references
- remediation guidance reference where eventually supported
- documentation/reference metadata

`Finding` must not become an independent source of truth for evaluation state.

Do not invent concrete security rules.

Do not define remediation execution.

---

## 5. Evidence Model

Evidence supports why an evaluation was produced.

Define conceptual evidence references/records.

Evidence may reference normalized, non-secret assessment facts such as:

- normalized identity attributes
- normalized relationship observations
- graph relationships
- capability states
- credential metadata that is explicitly non-secret
- accountability observations
- permission/access observations
- deterministic derived values
- relevant assessment context

Evidence MUST preserve enough provenance to trace back toward the collected source observation without requiring reports to expose raw provider payloads.

Evidence must be structured where possible.

Presentation text is not the authoritative evidence itself.

---

## 6. Provenance Chain

Define an explicit conceptual chain:

```
Provider observation
 -> SourceObservation
 -> Normalized domain fact
 -> Identity graph / derived normalized fact where applicable
 -> RuleEvaluation evidence reference
 -> Finding where emitted
 -> Output representation
```

Each transformation boundary must preserve traceability.

Do not claim cryptographic integrity where none has yet been designed.

Do not fabricate source provenance.

Unknown or unavailable provenance must be represented explicitly rather than invented.

---

## 7. Evidence Sufficiency

PASS and FAIL both require evidence appropriate to the rule semantics.

**FAIL:**
Must identify affirmative deterministic evidence supporting the failure predicate.

**PASS:**
Must identify evidence sufficient to establish that required observations were complete enough and the failure condition was not satisfied.

Absence can only be evidence when the relevant collection/capability is known to be sufficiently complete for that rule.

NOT_EVALUATED, NOT_APPLICABLE, and ERROR require structured reason/context, not fabricated PASS/FAIL evidence.

---

## 8. Evidence Immutability / Snapshot Semantics

Evidence represents what was observed/evaluated for a particular assessment.

Later tenant changes MUST NOT silently alter the meaning of an already produced `RuleEvaluation` or `Finding`.

Architecture should preserve assessment snapshot semantics.

Do not claim full immutable storage or cryptographic sealing yet.

Exact persistence, hashing, signing, timestamping, and archival mechanisms are TBD.

---

## 9. Data Minimization

Evidence and findings MUST contain only information necessary to:

- explain the evaluation
- identify the affected target
- support audit/review
- support safe output generation

Do not copy full provider objects merely for convenience.

Do not retain unnecessary raw payloads.

Do not duplicate sensitive directory attributes without a defined need.

Prefer references to normalized facts over raw source duplication.

---

## 10. Secret Exclusion

The following MUST NEVER become evidence/findings/reports:

- access tokens
- refresh tokens
- client-secret values
- passwords
- private keys
- authentication cookies
- recovery secrets
- raw credential material
- secret-bearing configuration
- authorization headers

Credential metadata may be evidence only when explicitly non-secret.

If unexpected secret-like material reaches the evidence boundary, future implementation must fail safely or sanitize/reject according to validated policy.

Do not invent the concrete secret-detection implementation yet.

---

## 11. Sensitive Enterprise Data

Treat findings/evidence as potentially sensitive security data.

They may reveal:

- identity names/IDs
- privilege relationships
- ownership/accountability gaps
- credential metadata
- permission exposure
- security weaknesses
- tenant structure

Therefore architecture must require:

- data minimization
- explicit output destinations
- no undisclosed telemetry
- no automatic third-party transmission
- safe logging
- tenant isolation
- controlled persistence where persistence exists
- secure defaults

Do not claim findings are harmless because secrets are excluded.

---

## 12. Tenant Isolation

Every `RuleEvaluation`, Evidence reference, and `Finding` must remain bound to one explicit tenant assessment context.

Evidence from one tenant MUST NOT support an evaluation for another tenant.

Cross-tenant evidence correlation is prohibited in V1.

Tenant-context mismatch/contamination is an integrity failure and must fail safely and visibly. [INV-10]

---

## 13. Stable Identifiers

Architecture must support stable machine-readable references for:

- assessment
- rule
- rule version
- rule evaluation
- finding
- target identity/context
- evidence
- provenance

Do not prescribe UUID/hash formats yet.

Identifiers must not depend only on display names.

Do not claim globally unique semantics until identifier strategy is designed.

---

## 14. Duplication / Deduplication

Repeated observations or evaluations must not silently create contradictory security meaning.

Architecture should support deterministic correlation/deduplication where appropriate.

Do NOT invent the exact finding fingerprint algorithm.

Finding identity/fingerprint strategy remains TBD.

---

## 15. Diagnostics

Diagnostics explain system/evaluation problems.

Diagnostics are distinct from tenant security Findings.

Examples:

- collection unavailable
- malformed normalized state
- rule exception
- output rendering problem
- evidence-reference failure

A system diagnostic MUST NOT automatically become a tenant security FAIL.

Diagnostics must be structured and sanitized.

Diagnostics MUST NOT expose secrets/tokens/raw authentication material.

---

## 16. Output Trust Boundary

Findings/evidence are canonical security data.

JSON, SARIF, HTML, CLI and future UI are representations.

Output renderers MUST NOT:

- change authoritative evaluation state
- invent evidence
- suppress ERROR/NOT_EVALUATED in a way that creates a false secure impression
- reinterpret severity
- perform provider calls
- request additional privileges
- execute remediation

Output architecture controls presentation. [INV-12]

---

## 17. Output Injection Defense

Treat provider-originated and tenant-originated strings as untrusted content.

Examples include:

- display names
- descriptions
- application names
- owner names
- identifiers where externally controlled
- future textual metadata

Future renderers must safely encode/escape data for their target format.

Architecture must anticipate:

- HTML injection
- terminal/control-character injection
- JSON correctness
- SARIF field safety
- log injection
- formula/spreadsheet injection if tabular export is ever added
- path/file-name injection where output names become configurable

Do NOT implement escaping here.

Do NOT prescribe libraries yet.

---

## 18. Evidence Fabrication Defense

No layer may fabricate evidence to make a finding appear complete.

If required evidence cannot be constructed:

- do not silently emit a fully trustworthy PASS/FAIL representation
- preserve the underlying `RuleEvaluation`
- produce explicit diagnostic/integrity handling
- fail safely according to future validated policy

Exact evidence-integrity failure policy remains TBD.

---

## 19. Tamper Awareness

Enterprise consumers may eventually require evidence that an assessment artifact has not been altered after production.

Architect for future support of:

- artifact hashing
- signing
- provenance manifests
- SBOM/release provenance association
- timestamping
- integrity verification

Do NOT claim these controls exist yet.

Do NOT choose signing algorithms/services yet.

This remains future hardening/TBD.

---

## 20. Confidentiality vs Integrity

Explicitly distinguish:

**Confidentiality:**
Prevent unauthorized disclosure of sensitive assessment data.

**Integrity:**
Prevent/detect unauthorized modification or fabricated assessment evidence.

**Availability:**
Ensure failures/resource exhaustion do not silently create misleading results.

The findings/evidence architecture must consider all three.

---

## 21. AI / LLM Boundary

AI may eventually:

- explain an existing deterministic finding
- summarize existing evidence
- translate technical explanation into user-friendly language

AI MUST NOT:

- create factual evidence
- modify evidence
- determine evaluation state
- change severity authoritatively
- invent missing provenance
- suppress diagnostics
- convert incomplete evidence into PASS/FAIL
- alter canonical `RuleEvaluation`/`Finding` records

AI-generated explanation must be clearly downstream/non-authoritative. [INV-13]

---

## 22. Redaction / Sanitization

Architecture should allow future output-specific redaction policies.

Redaction MUST NOT silently alter canonical evaluation semantics.

A redacted artifact should indicate where security-relevant information has been intentionally withheld when necessary for correct interpretation.

Exact redaction policy and schema remain TBD.

---

## 23. Retention / Persistence

Do not assume EntraNHI requires a database or SaaS storage.

V1 may operate with generated local/CI artifacts.

Any future persistence architecture must separately define:

- access control
- encryption
- retention
- deletion
- tenant isolation
- backup/recovery
- audit access
- data residency where applicable

Do not design these mechanisms now.

---

## 24. Error Handling

Evidence/findings processing failures must fail visibly.

Examples:

- broken evidence reference
- inconsistent tenant reference
- unsupported evidence type
- invalid finding mapping
- output serialization failure

Such failures MUST NOT silently convert an assessment into a successful/secure result.

Exact fatal vs isolated handling remains TBD.

---

## 25. Testability

Future tests should cover at minimum:

- FAIL evidence traceability
- PASS evidence completeness
- NOT_EVALUATED structured reason
- NOT_APPLICABLE structured reason
- ERROR structured reason
- provenance-chain preservation
- missing provenance
- broken evidence reference
- tenant-context mismatch
- cross-tenant evidence rejection
- secret exclusion
- token leakage prevention
- excessive raw-payload rejection/minimization
- output injection strings
- malformed Unicode/control characters
- evidence fabrication prevention
- finding/evaluation state consistency
- deterministic correlation behavior
- redaction semantics
- renderer cannot change authoritative state
- AI cannot modify evidence/evaluation
- persistence-independent core behavior

Use synthetic fixtures.

Do not create tests now.

---

## 26. Open Decisions / TBDs

Preserve TBDs including:

- concrete `RuleEvaluation` type
- concrete `Finding` type
- concrete `EvidenceReference`/`EvidenceRecord` type
- provenance schema
- finding emission policy
- finding fingerprint strategy
- identifier formats
- canonical serialization format
- evidence persistence model
- retention policy
- redaction policy
- sensitive-data classification policy
- evidence-integrity failure policy
- artifact hashing/signing
- provenance manifest format
- timestamping
- output artifact permissions
- encryption-at-rest requirements where persistence exists
- fatal vs isolated evidence-processing failure policy

Do not resolve these by speculation.

---

## 27. Invariant Alignment

Cross-reference especially:

| Invariant | Alignment |
| --- | --- |
| INV-02 | Deterministic assessment — `RuleEvaluation` is authoritative and immutable for a given input set |
| INV-04 | Normalized domain boundary — evidence references normalized facts, not provider payloads |
| INV-05 | Explicit evaluation states — all five states preserved through findings/evidence layer |
| INV-06 | Evidence traceability — PASS/FAIL require evidence; other states require structured reason |
| INV-07 | Capability awareness — capability state referenced in evidence and reason |
| INV-09 | Secret exclusion — evidence/findings/reports never contain secret material |
| INV-10 | Tenant boundary preservation — every artifact bound to single tenant context |
| INV-11 | Provenance preservation — explicit chain from source observation through finding |
| INV-12 | Core/output separation — output renderers cannot alter evaluation semantics |
| INV-13 | AI non-authority — AI cannot modify evidence, evaluation, or severity |
| INV-14 | Failure transparency — failures produce explicit diagnostics, never silent PASS |
| INV-16 | Security-sensitive defaults — data minimization, no undisclosed telemetry, tenant isolation |

Do not modify `architecture-invariants.md`.

---

## 28. Self-Verification

Confirm:

1. Only `findings-evidence.md` modified.
2. Nothing created/deleted/renamed/moved.
3. `RuleEvaluation` remains authoritative.
4. `Finding` is distinct from `RuleEvaluation`.
5. PASS and FAIL require appropriate evidence.
6. Missing evidence cannot fabricate PASS/FAIL.
7. Provenance chain is explicit.
8. Secrets/tokens are excluded.
9. Sensitive enterprise assessment data is recognized.
10. Tenant isolation is explicit.
11. Diagnostics are distinct from tenant findings.
12. Output renderers cannot alter security semantics.
13. Output injection risk is addressed architecturally.
14. AI cannot create/modify evidence or verdicts.
15. Tamper controls are future architecture, not falsely claimed implemented.
16. Persistence/database/SaaS is not assumed.
17. No concrete Microsoft permissions/endpoints invented.
18. No actual security rules invented.
19. No Git operations performed.

---

(End of file)