# Decision 0016 — Rule Evaluation Construction Mechanics

**Status:** Accepted for Phase 0.4 implementation

## 1. Context

Decision 0014 establishes the semantic contract for evidence and provenance:

- `RuleEvaluation` is the authoritative evaluation result.
- PASS and FAIL require evidence sufficient to support the deterministic verdict.
- NOT_EVALUATED, NOT_APPLICABLE, and ERROR require structured
  reason/context rather than fabricated verdict evidence.
- Evidence and provenance MUST NOT be fabricated.
- Tenant context MUST remain explicit and isolated.
- Finding, renderer, and AI layers MUST NOT change authoritative
  evaluation state.

Decision 0015 establishes the minimum support mechanics:

- Exactly one canonical `AssessmentState` per evaluation; no sixth state.
- State-aligned support: PASS/FAIL require non-empty `VerdictSupport`;
  NOT_EVALUATED requires `NotEvaluatedSupport`; NOT_APPLICABLE requires
  `NotApplicableSupport`; ERROR requires `ErrorSupport`.
- State/support mismatches are invalid.
- `EvidenceReference` and `ProvenanceReference` semantic boundaries.
- Capability/completeness grounding by reference to Stage-3 contracts;
  `PassPreconditionDisposition.Satisfied` is not PASS.
- Tenant-context mismatch is an integrity failure, not a verdict.
- Controlled construction or validation enforcing the §2.11 invariants.

The implementation design (`docs/03-implementation-design/findings-evidence-schema.md`)
further requires each `RuleEvaluation` to convey, at minimum:

- rule identity and version;
- tenant-scoped target subject;
- assessment and tenant-assessment context;
- exactly one canonical `AssessmentState`;
- state-aligned evidence/provenance or reason support;
- capability/completeness context grounding the result; and
- deterministic rule/configuration context where required.

Existing `EntraNHI.Core` Stage 4 primitives already provide the
state-aligned support side of that contract:

- `AssessmentState`, `TenantContext`;
- `VerdictSupport`, `EvidenceReference`, `EvidenceKey`,
  `ProvenanceReference`;
- `NotEvaluatedSupport`, `NotApplicableSupport`, `ErrorSupport`;
- `SupportText`, `NormalizedIdentityReference`, `NormalizedIdentityKey`;
- `CapabilityObservationContext`, `CapabilityState`,
  `ObservationCompleteness`;
- `PassPreconditionDisposition`, `PassPreconditionGuard`,
  `OperationalFailureCategory`.

What remains unresolved is the minimum construction and integrity
boundary for the authoritative `RuleEvaluation` aggregate itself: which
identity, subject, and assessment-context members it must carry, how the
single state binds to the single support, how capability/completeness
grounds the result without duplicating support types, and where
tenant-integrity validation occurs.

This decision resolves only that minimum construction boundary. It
authorizes no Finding type, no rule engine, no concrete rules, and no
representation beyond what is required to construct a valid
`RuleEvaluation`.

## 2. Decision

### 2.1 `RuleEvaluation` is the authoritative aggregate

The Core model SHALL represent each completed rule evaluation as one
authoritative `RuleEvaluation`.

A `RuleEvaluation` SHALL carry exactly one canonical `AssessmentState`:

- PASS
- FAIL
- NOT_EVALUATED
- NOT_APPLICABLE
- ERROR

No sixth state, alias state, presentation state, or implicit state is
introduced.

`RuleEvaluation` does not calculate verdicts for downstream layers to
reinterpret. Renderer, Finding, AI, UI, persistence, and provider layers
MUST NOT change its authoritative state, substitute a different state,
or re-derive state from presentation behavior. Where a `Finding` is
emitted, its state MUST equal its originating `RuleEvaluation` state, per
Decision 0014. This decision authorizes no concrete `Finding` type and no
finding-emission policy.

### 2.2 Minimum rule identity and version

A `RuleEvaluation` SHALL carry a minimum rule identity sufficient to
resolve the rule definition that produced the outcome, consisting of a
stable rule identifier plus a rule version.

The minimum semantic shape is:

- an opaque, provider-neutral rule identifier that is stable across
  versions; removal is deprecation, never identifier reassignment;
- an opaque, provider-neutral rule version; a behavior change produces a
  new version.

Both values SHALL be presence-validated opaque values. They SHALL NOT
select a provider identifier format, database identifier, version-string
syntax, version-ordering scheme, rule-set serialization, or deterministic
configuration schema. They carry identity only; rule semantics,
predicate definitions, and configuration interpretation remain with the
owning rule-engine contracts.

Implementation MAY introduce minimum opaque value objects (for example,
rule-identifier, rule-version, and a composite rule-identity carrying
both) following the established `TenantContext` / `EvidenceKey` opaque
pattern. Concrete names and member shapes SHALL be reviewed against this
decision immediately before implementation.

### 2.3 Minimum target-subject representation

A `RuleEvaluation` SHALL carry exactly one tenant-scoped target subject
as a `NormalizedIdentityReference`.

No new subject type is introduced. The existing
`NormalizedIdentityReference` (tenant context plus normalized identity
key) is the minimum representation. It SHALL NOT carry secrets,
display-name keys, provider SDK types, raw payloads, or source/native
references.

The target subject's tenant context MUST equal the evaluation tenant
context (§2.6). Subject-key correspondence beyond tenant equality (for
example, whether the `NotApplicableSupport` subject key equals the
target-subject key) is owned by rule applicability semantics, not by
construction mechanics, and is not constrained here beyond the tenant
integrity boundary.

### 2.4 Minimum assessment-context representation

A `RuleEvaluation` SHALL carry explicit evaluation context consisting of:

1. an explicit `TenantContext` identifying the evaluation tenant; and
2. a minimum opaque, provider-neutral assessment-context value
   identifying the assessment/run scope that produced the evaluation.

Tenant context MUST NOT be inferred from the subject, the evidence, or
ambient state. It is carried directly on the `RuleEvaluation` and
validated for equality against subject and support tenant contexts
(§2.6).

The assessment-context value is identity only: an opaque non-empty
reference to the assessment scope. It SHALL NOT define a database
identifier, provider identifier, timestamp representation, serialization
format, schema-version header, or persistence model. Implementation MAY
introduce one minimum opaque value object for this purpose following the
established opaque pattern. Concrete naming SHALL be reviewed immediately
before implementation. It is distinct from `SupportText` reason, scope,
criterion, and diagnostic text: it identifies the assessment, it does
not explain the outcome.

### 2.5 Capability and completeness grounding

Capability/completeness grounding is state-aligned so that no state
invents an observation claim and no context is duplicated:

- For PASS/FAIL, the `RuleEvaluation` SHALL carry exactly one
  `CapabilityObservationContext` as its grounding. `VerdictSupport`
  carries no capability member of its own; the `RuleEvaluation`-level
  grounding explains whether the inputs were trustworthy while the
  proof itself remains in the `VerdictSupport` evidence references.
- For NOT_EVALUATED, grounding is
  `NotEvaluatedSupport.ObservationContext`. The `RuleEvaluation` SHALL
  NOT carry a separate top-level `CapabilityObservationContext`; gap
  detail remains solely in support to avoid duplication and
  contradiction.
- For NOT_APPLICABLE, no capability/completeness grounding is required
  at this layer. Deterministic applicability context in
  `NotApplicableSupport` is sufficient. Construction MUST NOT invent a
  capability or completeness claim to fill this absence, consistent
  with the rule-engine applicability predicate consuming only
  identity-kind/context-kind facts and assessment context.
- For ERROR, no capability/completeness grounding is required at this
  layer. Construction MUST NOT invent observation completeness where
  evaluation failed before trustworthy observation state could be
  established.

This grounding is carried by reference to the canonical Stage-3
vocabulary (`CapabilityState`, `ObservationCompleteness`) and SHALL NOT
duplicate, replace, or reinterpret Stage-3 semantics:

- No new capability taxonomy is introduced.
- No universal capability-to-assessment-state mapping is introduced.
- `RuleEvaluation` SHALL NOT calculate `PassPreconditionDisposition` and
  SHALL NOT store a disposition.
- `PassPreconditionDisposition.Satisfied` does not authorize PASS, does
  not substitute for `VerdictSupport`, and SHALL NOT be accepted as a
  construction input in place of evidence.
- Partial observations support a verdict only where the consuming rule
  explicitly proves sufficiency under its own semantics; the aggregate
  carries the grounding context for PASS/FAIL, it does not perform that
  proof.
- Confirmed-empty representation, where required for absence-dependent
  PASS, travels through the rule's evidence references and completeness
  argument per Decisions 0014 and 0015, not through the disposition.

No separate reconciliation is required because grounding is not
duplicated: PASS/FAIL grounding lives at the `RuleEvaluation` level,
NOT_EVALUATED grounding lives solely in
`NotEvaluatedSupport.ObservationContext`, and NOT_APPLICABLE and ERROR
carry no grounding at this layer. If rule semantics require sufficiency
beyond presence of the required context, that rule-specific sufficiency
proof belongs to the rule, not to aggregate construction.

### 2.6 Construction strategy and state/support binding

A `RuleEvaluation` SHALL bind exactly one assessment state to exactly
one support instance of the corresponding kind:

| Assessment state | Required support |
| --- | --- |
| PASS | `VerdictSupport` |
| FAIL | `VerdictSupport` |
| NOT_EVALUATED | `NotEvaluatedSupport` |
| NOT_APPLICABLE | `NotApplicableSupport` |
| ERROR | `ErrorSupport` |

Implementation SHALL provide closed construction sufficient to make
invalid combinations structurally rejectable:

1. The aggregate constructor SHALL NOT be publicly callable with an
   arbitrary state plus an arbitrary support object.
2. Construction SHALL expose one per-state factory path (conceptually:
   create-PASS, create-FAIL, create-NOT_EVALUATED,
   create-NOT_APPLICABLE, create-ERROR), each accepting only the
   matching support type. The type system, not caller discipline,
   prevents a PASS/FAIL from being constructed with non-verdict support
   and prevents a non-verdict state from being constructed with
   `VerdictSupport`. Create-PASS and create-FAIL SHALL additionally
   require exactly one `CapabilityObservationContext`; the remaining
   factory paths SHALL NOT accept a separate top-level grounding:
   NOT_EVALUATED grounding travels solely in
   `NotEvaluatedSupport.ObservationContext`, and NOT_APPLICABLE and
   ERROR carry no grounding at this layer.
3. The aggregate SHALL expose no setters, no parameterless construction,
   and no independent nullable evidence/reason members whose
   combinations permit unsupported states.
4. `VerdictSupport` construction already rejects empty and null evidence
   collections; `RuleEvaluation` factories SHALL NOT accept a null
   support instance and SHALL NOT bypass that validation.
5. Values outside the declared `AssessmentState` members are
   programmer/invariant violations and SHALL be rejected rather than
   interpreted as domain states. Ordinary canonical domain states such
   as unknown capability/completeness remain valid domain information
   and MUST NOT be confused with invalid enum values.
6. `VerdictSupport` does not itself decide PASS or FAIL. The
   authoritative `AssessmentState` remains on the `RuleEvaluation`, and
   rule logic determines that state before construction.

### 2.7 Tenant-integrity validation boundary

Tenant-integrity validation occurs at `RuleEvaluation` construction, on
every factory path. There is no ambient, inferred, or default tenant.

Construction SHALL enforce all of the following by value equality of the
explicit `TenantContext` values:

1. The target subject tenant (`TargetSubject.TenantContext`) MUST equal
   the evaluation tenant.
2. For PASS/FAIL, every `EvidenceReference.TenantContext` in the
   `VerdictSupport` evidence collection MUST equal the evaluation
   tenant.
3. For NOT_EVALUATED, NOT_APPLICABLE, and ERROR, the support tenant
   (`NotEvaluatedSupport.TenantContext`,
   `NotApplicableSupport.TenantContext`, `ErrorSupport.TenantContext`)
   MUST equal the evaluation tenant.
4. For NOT_APPLICABLE, the contained subject tenant
   (`NotApplicableSupport.Subject.TenantContext`) MUST additionally
   equal the evaluation tenant.

A mismatch on any of these checks is an integrity/construction failure.
It MUST cause construction to fail without producing a `RuleEvaluation`
in any state. It MUST NOT be mapped to PASS, FAIL, NOT_EVALUATED,
NOT_APPLICABLE, or ERROR, and MUST NOT be converted into verdict
evidence or structured reason content.

Support objects validate their own member presence at their own
construction; `RuleEvaluation` validates cross-object tenant consistency
and state/support correspondence. It does not repair, coerce, normalize,
or re-scope tenant contexts.

The exact exception type and any fatal-versus-isolated handling above
construction remain deferred to the owning error/evidence design, per
Decision 0015. This decision requires only that construction fail
visibly and never yield a cross-tenant evaluation.

### 2.8 Fabrication defense and provider neutrality

Construction SHALL NOT fabricate evidence or provenance:

- Factories MUST NOT synthesize `EvidenceReference`,
  `ProvenanceReference`, or `EvidenceKey` values to satisfy the
  non-empty verdict requirement.
- Factories MUST NOT invent a `CapabilityObservationContext` for
  NOT_APPLICABLE or ERROR and MUST NOT add a separate top-level
  grounding to NOT_EVALUATED.
- Unknown or unavailable provenance MUST remain explicit per the
  Decision 0015 provenance boundary; construction MUST NOT invent a
  source in its place. The member-level choice between a mandatory
  reference member and an explicit unknown representation remains
  deferred and is not resolved here.
- `SupportText` reason, scope, criterion, and diagnostic values remain
  opaque presence-validated text. Construction validates presence only
  and performs no secret detection, sanitization, or redaction. Callers
  MUST NOT place secrets, raw provider payloads, provider SDK objects,
  authentication material, or other secret-bearing values in them.

All new value objects SHALL remain opaque and provider-neutral,
following the established `TenantContext` / `EvidenceKey` /
`ProvenanceReference` / `SupportText` pattern. Core SHALL NOT reference
provider SDK types, raw provider payloads, authentication envelopes,
persistence identifiers, or renderer types.

### 2.9 Determinism and output neutrality

For semantically equivalent normalized inputs, rule identity/version,
state-aligned capability/completeness grounding where §2.5 requires it,
subject, assessment context, and
evidence/provenance references, a constructed `RuleEvaluation` SHALL
have semantically equivalent meaning.

This decision does not require byte-identical serialization, globally
stable ordering, cryptographic identity, hashes, signatures, or
persistence-level equality.

The aggregate SHALL remain output-neutral. It SHALL NOT contain console
formatting, Markdown/HTML/SARIF formatting, renderer-specific state,
localization decisions, UI severity interpretation, or AI-generated
verdict authority. Renderers and AI consumers may explain existing
authoritative records but MUST NOT alter their state, evidence,
provenance, or structured reason semantics.

### 2.10 Construction invariants

Implementation SHALL enforce at least these invariants:

1. `RuleEvaluation` is the authoritative result; downstream layers
   cannot change its state.
2. Exactly one canonical `AssessmentState` is carried.
3. PASS requires `VerdictSupport`.
4. FAIL requires `VerdictSupport`.
5. NOT_EVALUATED requires `NotEvaluatedSupport`.
6. NOT_APPLICABLE requires `NotApplicableSupport`.
7. ERROR requires `ErrorSupport`.
8. State/support mismatches are structurally rejected.
9. PASS/FAIL cannot be constructed without non-empty `VerdictSupport`.
10. Non-verdict states cannot use `VerdictSupport` as their state
    support.
11. Evaluation tenant context is explicit on the aggregate.
12. Target-subject tenant MUST equal evaluation tenant.
13. Every PASS/FAIL `EvidenceReference` tenant MUST equal evaluation
    tenant.
14. Non-verdict support tenant MUST equal evaluation tenant.
15. NOT_APPLICABLE MUST additionally validate its contained subject
    tenant against the evaluation tenant.
16. Cross-tenant mismatch is an integrity/construction failure and MUST
    NOT be mapped to PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or
    ERROR.
17. `RuleEvaluation` does not calculate `PassPreconditionDisposition`,
    and `Satisfied` does not authorize PASS.
18. Evidence and provenance MUST NOT be fabricated; unknown provenance
    remains explicit.
19. Core remains provider-neutral: no provider SDK types, raw payloads,
    secrets, or renderer authority in the aggregate.
20. Renderer, Finding, AI, UI, persistence, and provider layers cannot
    change authoritative evaluation state.
21. Capability/completeness grounding is state-aligned per §2.5: PASS/FAIL
    carry exactly one `CapabilityObservationContext`; NOT_EVALUATED
    grounding lives solely in `NotEvaluatedSupport.ObservationContext`
    with no separate top-level grounding; NOT_APPLICABLE and ERROR
    carry no grounding and construction MUST NOT invent one.

## 3. Minimum implementation authorization

After this decision is accepted, Stage 4 MAY introduce only the minimum
`EntraNHI.Core` types and construction mechanics required to enforce §2.

Implementation MAY establish concrete Core types for:

- rule identity and version (opaque identifier, opaque version, and
  their composite);
- assessment-context scope reference (one opaque value object);
- the authoritative `RuleEvaluation` aggregate binding rule identity,
  target subject, evaluation tenant, assessment context, single
  assessment state, single state-aligned support, and state-aligned
  capability/completeness grounding per §2.5 (direct grounding for
  PASS/FAIL only);
- per-state closed factory paths with tenant-integrity validation.

Concrete names and member shapes SHALL be reviewed against this decision
immediately before implementation.

This authorization does not extend to a rule engine, concrete security
rules, Finding emission, rendering, persistence, provider or Graph
integration, authentication, or serialization.

## 4. Explicitly deferred

This decision preserves every deferral owned by Decisions 0014 and 0015
except where §2 resolves the minimum shape specifically required to
construct `RuleEvaluation` (rule identity/version presence, target
subject reuse, assessment-context presence, capability-grounding
placement, closed state/support binding, tenant-integrity boundary).

In particular, this decision does NOT resolve:

- final serialization formats or schema-version headers;
- provider identifiers, provider mappings, or provider SDK behavior;
- database or persistence identifiers;
- fingerprinting, deduplication, or correlation algorithms;
- hashing, signing, timestamping, or any cryptographic integrity claim;
- Finding emission policy or Finding record shape;
- severity, confidence, risk-score, or priority taxonomies;
- UI or API contracts, output formats, or renderer contracts;
- secret-detection or sanitization implementation mechanics;
- remediation guidance or execution;
- detailed provenance-record or provenance-manifest schema;
- complete diagnostic schema or operational-failure-to-state mapping;
- fatal-versus-isolated integrity-failure policy above construction;
- assessment-time representation;
- concrete security rules or Graph endpoints, scopes, and permissions.

No item in this list is resolved by this decision. Where this decision
does not explicitly resolve an existing TBD, that TBD remains unresolved
with its existing owner.

## 5. Required implementation tests

The subsequent implementation unit SHALL test at minimum:

### Identity, subject, and assessment context

- Rule identity without a non-empty identifier is rejected.
- Rule identity without a non-empty version is rejected.
- Missing target subject is rejected.
- Missing evaluation tenant is rejected.
- Missing assessment-context scope reference is rejected.
- Missing PASS/FAIL capability/completeness grounding is rejected.
- A separate top-level grounding on NOT_EVALUATED is rejected;
  NOT_EVALUATED grounding travels solely in
  `NotEvaluatedSupport.ObservationContext`.
- Invented capability/completeness grounding on NOT_APPLICABLE or ERROR
  is rejected; those states carry no grounding at this layer.

### State/support integrity

- PASS with verdict support and capability grounding is accepted.
- FAIL with verdict support and capability grounding is accepted.
- PASS without evidence support is rejected.
- FAIL without evidence support is rejected.
- PASS without capability grounding is rejected.
- FAIL without capability grounding is rejected.
- Empty verdict evidence is rejected.
- NOT_EVALUATED without its structured context is rejected.
- NOT_APPLICABLE without its structured context is rejected.
- ERROR without its structured context is rejected.
- State/support mismatches are rejected, including non-verdict states
  supplied with `VerdictSupport`.
- Invalid `AssessmentState` values are rejected rather than interpreted
  as domain states.

### Tenant isolation

- Same-tenant evaluation, subject, support, and evidence linkage is
  accepted.
- Target-subject tenant mismatch is rejected as a construction failure.
- Cross-tenant evidence linkage in PASS/FAIL is rejected as a
  construction failure.
- Non-verdict support tenant mismatch is rejected as a construction
  failure.
- NOT_APPLICABLE contained-subject tenant mismatch is rejected as a
  construction failure.
- No tenant mismatch is mapped to PASS, FAIL, NOT_EVALUATED,
  NOT_APPLICABLE, or ERROR.

### Stage-3 compatibility

- Unknown completeness does not become PASS through these mechanics.
- Non-Observed capability does not become PASS through these mechanics.
- Partial observations are not treated as universally sufficient.
- `PassPreconditionDisposition.Satisfied` does not itself construct PASS
  and is not accepted as verdict support.

### Provider, secret, and downstream boundary

- The authorized Core contracts require no raw provider payloads,
  provider SDK types, or secret-bearing authentication material.
- Presentation text cannot substitute for evidence references or
  structured reason support.
- Missing required provenance cannot be silently synthesized through
  construction.

## 6. Security consequences

This decision makes invalid `RuleEvaluation` construction structurally
rejectable before any rule implementation begins.

It reduces the risk of:

- evidence-free PASS;
- evidence-free FAIL;
- state/support confusion (verdict support on non-verdict states and
  vice versa);
- cross-tenant evaluation and cross-tenant evidence contamination;
- ambient-tenant inference;
- capability-gap PASS via disposition confusion;
- fabricated evidence or invented provenance entering through
  construction conveniences;
- invented capability/completeness grounding on states that carry none;
- provider payload or credential leakage into the authoritative record;
- renderer-driven or AI-driven verdict mutation.

It does not claim that structural typing alone detects every secret,
proves evidence authenticity, or provides cryptographic integrity.

## 7. Relationship to earlier decisions

This decision extends but does not redefine:

- Decision 0007 — canonical evaluation outcome contract;
- Decision 0012 — PASS eligibility proof contract;
- Decision 0013 — PASS precondition guard mechanics;
- Decision 0014 — evidence and provenance semantic contract; and
- Decision 0015 — rule evaluation support mechanics.

Where this decision does not explicitly resolve an existing TBD, that
TBD remains unresolved with its existing owner.

## 8. Exit condition

This decision is ready for implementation only when review confirms
that:

1. every `RuleEvaluation` carries rule identity/version, tenant-scoped
   target subject, explicit evaluation tenant, assessment-context scope,
   exactly one canonical state, exactly one matching support, and
   state-aligned capability/completeness grounding per §2.5 (direct
   grounding for PASS/FAIL; support-contained grounding for
   NOT_EVALUATED; no grounding for NOT_APPLICABLE and ERROR);
2. PASS/FAIL cannot be constructed without non-empty verdict support
   and non-verdict states cannot use verdict support;
3. every tenant-integrity check in §2.7 is enforced at construction and
   no mismatch yields a domain-state evaluation;
4. `PassPreconditionDisposition` is neither calculated nor consumed by
   construction;
5. all value objects remain opaque and provider-neutral;
6. no deferred serialization, provider, persistence, Finding, severity,
   UI/API, secret-detection, remediation, fingerprint, cryptographic, or
   detailed-provenance design has been silently resolved; and
7. the authorized implementation surface is the minimum necessary for
   the Stage-4 `RuleEvaluation` construction gate.
