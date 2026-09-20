# Decision 0014 — Evidence and Provenance Semantic Contract

**Status:** Accepted for Phase 0.4 implementation

## Context

Stage 4 — Evidence/provenance contracts requires a semantic boundary for
what proves an evaluation conclusion before any concrete evidence, finding,
or provenance C# representation is selected
(`docs/03-implementation-design/implementation-sequence.md`, Stage 4).

The authoritative semantics are preserved from
`docs/03-implementation-design/findings-evidence-schema.md` and remain
consistent with:

- Decision 0007 (canonical evaluation outcome contract);
- Decision 0012 (PASS precondition proof contract); and
- Decision 0013 (PASS precondition guard mechanics).

This decision establishes only the Stage-4 semantic boundary. It authorizes
no implementation, selects no concrete record shapes, and claims nothing as
implemented or tested.

## Decision

### 1. Authority boundary

`RuleEvaluation` remains authoritative for the evaluation outcome.

`Finding`, where emitted, is derivative of its originating
`RuleEvaluation` and MUST NOT:

- recompute rule predicates;
- change the originating `AssessmentState`;
- upgrade or downgrade `PASS`/`FAIL`;
- convert `ERROR` to `FAIL`;
- convert `NOT_EVALUATED` to `PASS`;
- infer an assessment state from presentation behavior; or
- give AI/LLM authority over evaluation state or authoritative evidence.

Finding state, where a `Finding` exists, MUST equal its originating
`RuleEvaluation` state.

AI/LLM MUST NOT determine, change, or supply authoritative evaluation
state, evidence, or provenance.

### 2. Evidence versus structured reason

`PASS` and `FAIL` require evidence appropriate to the rule semantics.

`NOT_EVALUATED`, `NOT_APPLICABLE`, and `ERROR` require structured
reason/context appropriate to their state and MUST NOT receive fabricated
`PASS`/`FAIL`-shaped evidence merely to make the result appear complete.

No sixth assessment state is introduced by this decision. The authoritative
assessment states remain exactly `PASS`, `FAIL`, `NOT_EVALUATED`,
`NOT_APPLICABLE`, and `ERROR`, with semantics owned elsewhere and
referenced here without redefinition.

### 3. Evidence source boundary

Evidence MUST be based on normalized, non-secret assessment facts.

Evidence MAY reference normalized identity facts, normalized
relationships, graph-derived facts, capability/completeness context,
explicitly non-secret credential metadata, deterministic derived values,
and relevant assessment context where authorized by the design.

Evidence MUST NOT treat presentation text as authoritative evidence.

Raw provider payloads are excluded by default.

Provider SDK types MUST NOT become Core evidence contracts.

Evidence prefers references to normalized facts over duplication of
provider objects.

Provider/tenant strings remain untrusted opaque data. This decision does
not select renderer-specific escaping mechanics.

### 4. Provenance

Evidence MUST preserve sufficient provenance to trace toward the source
observation across transformation boundaries.

The conceptual chain is preserved:

provider observation
→ source observation
→ normalized domain fact
→ graph/derived fact where applicable
→ evaluation evidence reference
→ Finding where emitted
→ output representation.

Unknown or unavailable provenance MUST be represented explicitly and MUST
never be invented.

Provenance is not itself proof that a fact satisfies a rule. Rule semantics
determine which provenance-backed facts constitute evidence.

### 5. Capability / completeness

Evidence semantics MUST preserve relevant capability/completeness context.

Missing evidence MUST never authorize `PASS`.

Absence MAY support `PASS` only where rule semantics establish sufficient
collection/completeness for that absence to be meaningful.

An absence-dependent `PASS` MUST preserve the completeness argument that
authorized the conclusion.

This decision remains consistent with Decisions 0012 and 0013 and MUST NOT
turn `PassPreconditionDisposition.Satisfied` into `PASS`.
`PassPreconditionDisposition.Satisfied` means only that the generic
Stage-3 capability/completeness precondition does not prohibit later PASS
consideration; it establishes no applicability, predicate satisfaction,
evidence sufficiency, confirmed-empty semantics, or assessment verdict.

### 6. State-specific obligations

The semantic distinctions between states are preserved:

- `NOT_EVALUATED`: structured reason for inability to evaluate, including
  relevant missing capability/data and affected scope.
- `NOT_APPLICABLE`: deterministic applicability rationale.
- `ERROR`: sanitized operational/integrity context sufficient to identify
  the failed rule/target/stage and conceptual failure category without
  leaking secrets or provider exception types.
- `PASS`: affirmative evidence sufficient for the rule semantics,
  including sufficient input/completeness basis and evidence that the
  failure condition was not satisfied.
- `FAIL`: affirmative deterministic evidence that the documented failure
  predicate was satisfied.

The following inequalities are explicitly preserved:

- `NOT_EVALUATED` != `FAIL`
- `NOT_APPLICABLE` != `PASS`
- `ERROR` != `FAIL`

### 7. Fabrication defense

No layer may fabricate evidence or provenance.

Evidence and provenance MUST NOT be fabricated, invented, or synthesized
to make an evaluation or Finding appear complete.

If required evidence for `PASS`/`FAIL` cannot be constructed, the system
MUST NOT silently produce a fully trustworthy `PASS`/`FAIL`
representation.

The underlying `RuleEvaluation` MUST remain preserved and the condition
MUST remain visible for later validated integrity/error handling.

This decision does not decide the exact failure-handling policy.

This decision does not claim cryptographic evidence integrity.
Signing, hashing, and timestamping remain deferred hardening and MUST NOT
be claimed unless actually selected and implemented.

### 8. Tenant isolation

`RuleEvaluation`, evidence references, provenance context, and Findings
MUST remain bound to an explicit tenant assessment context.

Evidence from one tenant MUST never support another tenant's evaluation.

Tenant-context mismatch and cross-tenant evidence contamination are
integrity conditions and MUST fail safely and visibly.

This decision does not decide the concrete failure mapping or mechanics
for such integrity conditions.

### 9. Secret exclusion / minimization

Evidence, provenance, and diagnostic semantics MUST exclude:

- access/refresh tokens;
- client-secret values;
- private-key material;
- passwords/recovery codes;
- authentication cookies/raw credential material;
- raw authorization headers;
- credential-bearing URLs/query strings; and
- secret-bearing configuration values.

References to normalized facts are preferred over duplication of provider
objects.

Raw provider payloads MUST NOT be retained by default.

Provider/tenant strings remain untrusted opaque data. This decision does
not select renderer-specific escaping mechanics.

### 10. Output neutrality

Authoritative evidence and provenance semantics MUST NOT contain
renderer-specific authority or formatting.

Renderers cannot change `AssessmentState`, fabricate evidence or
provenance, or reinterpret an authoritative result.

Redaction of a projection MUST NOT change the underlying authoritative
record.

This decision does not define concrete redaction policy.

### 11. Determinism

Equivalent normalized inputs, capability/completeness state, rule
version/configuration, and deterministic execution context MUST produce
semantically equivalent evidence/finding meaning.

This decision does not choose concrete ordering keys, identifier
algorithms, serialization formats, or fingerprint algorithms unless
already fixed by the authoritative design.

## Explicitly deferred mechanics

The following remain deliberately unresolved and MUST NOT be treated as
decided by this ADR:

- concrete `RuleEvaluation` member shape;
- concrete `Finding` member shape;
- concrete evidence-reference/record shape;
- concrete provenance-record shape;
- concrete diagnostic shape;
- identifier formats;
- canonical serialization format;
- schema-version/header mechanics;
- finding-emission policy;
- finding fingerprint/dedup algorithm;
- detailed provenance schema/manifest;
- redaction policy/schema;
- sensitive-data classification beyond secret exclusion;
- evidence-integrity failure policy;
- signing/hashing/timestamping;
- persistence/retention/storage mechanics;
- title/summary templates/localization;
- severity taxonomy;
- secret-detection/sanitization implementation mechanics;
- assessment-time representation;
- renderer formats; and
- provider/Graph/authentication mechanics.

No item in this list is resolved by this decision.

## Implementation boundary

This semantic ADR does NOT authorize concrete evidence, finding, or
provenance C# types.

A subsequent reviewed Stage-4 mechanics decision MUST establish the
minimum concrete Core representation before implementation.

## Conceptual test obligations

The following are recorded as conceptual obligations for later
mechanics and implementation verification. Nothing here claims they are
implemented or tested:

- `PASS`/`FAIL` cannot exist as trustworthy representations without
  required evidence obligations;
- non-verdict states preserve distinct structured reasons;
- provenance is preserved across transformations;
- missing provenance is explicit and never fabricated;
- cross-tenant evidence is rejected;
- evidence/finding state cannot diverge from originating evaluation
  state;
- secret-shaped synthetic material cannot enter authoritative evidence;
- raw provider objects/types do not enter Core evidence contracts;
- absence-dependent `PASS` requires auditable completeness basis;
- missing/unconstructable evidence cannot silently yield trustworthy
  `PASS`/`FAIL`;
- equivalent inputs produce semantically equivalent evidence meaning;
  and
- renderer/AI layers cannot alter authoritative state/evidence.

## Consequences

Stage 4 gains a reviewable semantic boundary that preserves the
authoritative evidence/provenance design without selecting concrete
representation.

Evaluation authority, evidence sufficiency, provenance traceability,
capability/completeness honesty, tenant isolation, secret exclusion,
output neutrality, determinism, and fabrication defense become reviewable
constraints for the later Stage-4 mechanics decision.

## Non-goals

This decision does not define concrete record members, serialization,
emission policy, fingerprinting, redaction policy, integrity mechanisms,
persistence, severity taxonomy, renderer formats, provider behavior, or
authentication mechanics.
