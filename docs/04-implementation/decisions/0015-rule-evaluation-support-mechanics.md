# Decision 0015 — Rule Evaluation Support Mechanics

**Status:** Accepted for Phase 0.4 implementation

## 1. Context

Decision 0014 establishes the semantic contract for evidence and provenance:

- `RuleEvaluation` is the authoritative evaluation result.
- PASS and FAIL require evidence sufficient to support the deterministic verdict.
- NOT_EVALUATED, NOT_APPLICABLE, and ERROR require structured reason/context rather than fabricated verdict evidence.
- Evidence and provenance MUST NOT be fabricated.
- Provenance MUST be preserved and MUST NOT be invented.
- Tenant context MUST remain explicit and isolated.
- Evidence and reason/context surfaces MUST exclude secrets and raw provider payloads.
- Finding, renderer, and AI layers MUST NOT change authoritative evaluation state.

The implementation design further requires `RuleEvaluation` to convey, at minimum:

- rule identity and version;
- tenant-scoped target subject;
- assessment and tenant-assessment context;
- exactly one canonical `AssessmentState`;
- evidence/provenance support for PASS and FAIL;
- structured reason/context for NOT_EVALUATED, NOT_APPLICABLE, and ERROR;
- capability/completeness context grounding the result; and
- deterministic rule/configuration context where required.

The design intentionally deferred concrete C# record shapes and member lists. Stage 4 now requires the minimum Core mechanics necessary to make the evidence/reason obligations structural before rule implementation begins.

This decision resolves only that minimum representation boundary.

It does not finalize the complete findings/evidence schema.

## 2. Decision

### 2.1 `RuleEvaluation` remains the authoritative result

The Core model SHALL represent each completed rule evaluation as one authoritative `RuleEvaluation`.

A `RuleEvaluation` SHALL carry exactly one canonical `AssessmentState`:

- PASS
- FAIL
- NOT_EVALUATED
- NOT_APPLICABLE
- ERROR

No sixth state, alias state, presentation state, or implicit state is introduced.

`Finding` remains downstream and derivative. This decision does not authorize a concrete `Finding` type or finding-emission policy.

### 2.2 Evaluation support is state-aligned

A `RuleEvaluation` SHALL NOT use independent nullable evidence and reason members whose combinations permit unsupported or contradictory states.

Instead, construction SHALL require support appropriate to the selected assessment state.

The minimum semantic partition is:

| Assessment state | Required support |
| --- | --- |
| PASS | Verdict support containing one or more evidence references sufficient for the rule's PASS semantics |
| FAIL | Verdict support containing one or more evidence references sufficient for the rule's FAIL semantics |
| NOT_EVALUATED | Structured inability-to-evaluate context |
| NOT_APPLICABLE | Structured non-applicability context |
| ERROR | Structured sanitized error context |

A PASS or FAIL without verdict support is invalid.

A non-verdict state without its corresponding structured context is invalid.

State/support mismatches are invalid.

### 2.3 Verdict support

PASS and FAIL SHALL use a common verdict-support concept because both are deterministic verdicts requiring evidence.

Verdict support SHALL contain at least one evidence reference.

An empty evidence collection MUST NOT constitute valid verdict support.

Verdict support does not itself decide PASS or FAIL. The authoritative `AssessmentState` remains on the `RuleEvaluation`, and rule logic determines that state.

Verdict support MUST NOT:

- fabricate evidence;
- convert missing evidence into PASS or FAIL;
- infer evidence merely from absence;
- contain raw provider payloads;
- contain provider SDK objects;
- contain secret-bearing values; or
- allow presentation text to become authoritative evidence.

Where PASS depends on absence, the referenced evidence must be backed by the rule's required completeness semantics. `PassPreconditionDisposition.Satisfied` alone is not evidence and is not PASS.

### 2.4 Evidence reference boundary

Stage 4 requires a concrete Core evidence-reference concept so that verdict support cannot be represented merely as arbitrary text.

At the semantic boundary, an evidence reference SHALL:

- refer only to normalized assessment evidence or derived assessment evidence;
- remain bound to explicit tenant-assessment context;
- preserve the association necessary to trace toward provenance without embedding or inventing provenance;
- remain distinct from presentation text; and
- remain provider-neutral.

These requirements define semantic obligations only. They do not prescribe the final evidence-reference member list, identifier representation, or evidence-to-provenance reference topology.

This decision does not select:

- externally stable evidence identifier formats;
- serialization formats;
- persistence identifiers;
- hashing or signing;
- output representation;
- raw-provider identifiers as evidence content; or
- a complete evidence-record schema.

Evidence references are references to normalized evidence semantics, not containers for provider payloads.

### 2.5 Provenance reference boundary

Evidence used for a verdict SHALL preserve provenance toward the collected source observation.

The minimum Core representation SHALL therefore provide an explicit provenance-reference concept.

A provenance reference SHALL be distinguishable from evidence itself.

It MUST NOT:

- claim that provenance is rule proof by itself;
- invent an unavailable source;
- silently replace unknown provenance with synthetic provenance;
- embed raw provider payloads; or
- contain secrets.

Detailed provenance-record members, manifests, serialization, persistence, and provider-specific mappings remain deferred.

Where provenance is legitimately unknown or unavailable, that condition must remain explicit. It MUST NOT be replaced with invented provenance.

This decision does not determine whether provenance association is represented by a mandatory reference member, an explicit unavailable/unknown representation, or another concrete Core shape. That member-level choice remains subject to the implementation-shape review and the deferred detailed provenance schema.

### 2.6 Structured NOT_EVALUATED context

NOT_EVALUATED support SHALL represent why an otherwise applicable or potentially applicable rule could not be evaluated.

Its minimum semantics SHALL permit preservation of:

- the required capability or data identity that was unavailable or insufficient;
- the relevant capability state;
- relevant completeness context where applicable; and
- the affected evaluation scope/context.

NOT_EVALUATED support MUST NOT be usable as PASS or FAIL evidence.

The representation SHALL NOT introduce a universal capability-to-assessment-state mapping.

### 2.7 Structured NOT_APPLICABLE context

NOT_APPLICABLE support SHALL represent the deterministic reason that the rule does not apply.

Its minimum semantics SHALL permit preservation of:

- the applicable subject/identity context;
- the assessment context relevant to applicability; and
- the applicability criterion that established non-applicability.

NOT_APPLICABLE MUST NOT be used to hide missing capability, incomplete collection, unsupported functionality, or execution failure.

### 2.8 Structured ERROR context

ERROR support SHALL represent failure preventing trustworthy evaluation.

Its minimum semantics SHALL permit preservation of:

- a sanitized failure category;
- affected evaluation scope/context; and
- sanitized diagnostic context sufficient to distinguish the failure semantically.

ERROR context MUST NOT contain:

- credentials or tokens;
- raw provider payloads;
- provider exception objects or provider exception types as Core contracts;
- authentication headers;
- credential-bearing URLs; or
- other secret-bearing material.

This decision does not establish the complete operational-failure-to-assessment-state mapping or the final diagnostic schema.

### 2.9 Capability and completeness grounding

Evaluation support SHALL preserve the capability/completeness information necessary to explain whether the evaluation was trustworthy.

The Stage-3 contracts remain authoritative:

- missing required capability cannot become PASS;
- unknown completeness cannot become PASS;
- partial observations require rule-specific sufficiency before a verdict where applicable;
- confirmed-empty observations support PASS only where explicit rule semantics and completeness establish that absence is conclusive; and
- `PassPreconditionDisposition.Satisfied` is a precondition disposition, not an assessment verdict.

Decision 0015 SHALL NOT duplicate or replace Stage-3 capability semantics.

### 2.10 Tenant-context integrity

Every `RuleEvaluation` and every evidence reference consumed by it SHALL be bound to the same explicit tenant-assessment context.

Construction or validation MUST reject tenant-context mismatches rather than producing a verdict from cross-tenant evidence.

Cross-tenant contamination remains an integrity failure, not PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR.

The exact fatal-versus-isolated integrity-failure policy remains deferred to the owning error/evidence design.

### 2.11 Construction invariants

The implementation SHALL provide controlled construction or validation sufficient to enforce at least these invariants:

1. Assessment state is a defined canonical value.
2. PASS requires non-empty verdict support.
3. FAIL requires non-empty verdict support.
4. NOT_EVALUATED requires NOT_EVALUATED context.
5. NOT_APPLICABLE requires NOT_APPLICABLE context.
6. ERROR requires ERROR context.
7. Support kind must correspond to assessment state.
8. Verdict support contains at least one evidence reference.
9. Evidence references preserve explicit tenant-assessment context.
10. Evaluation and referenced evidence tenant context must match.
11. Required provenance references cannot be silently fabricated.
12. Invalid enum values are rejected rather than interpreted as domain states.

Ordinary canonical domain states such as unknown capability/completeness remain valid domain information and MUST NOT be confused with invalid enum values.

### 2.12 Determinism

For semantically equivalent normalized inputs, rule/configuration context, and evidence/provenance references, evaluation support SHALL have semantically equivalent meaning.

This decision does not require:

- byte-identical serialization;
- globally stable serialized ordering;
- cryptographic identity;
- hashes;
- signatures; or
- persistence-level equality.

### 2.13 Output neutrality

The Core support model SHALL remain output-neutral.

It SHALL NOT contain:

- console formatting;
- Markdown/HTML/SARIF formatting;
- renderer-specific state;
- localization decisions;
- UI-specific severity interpretation; or
- AI-generated verdict authority.

Renderers and AI consumers may explain existing authoritative records but MUST NOT alter their state, evidence, provenance, or structured reason semantics.

## 3. Minimum implementation authorization

After this decision is accepted, Stage 4 MAY introduce only the minimum `EntraNHI.Core` types and validation/construction mechanics required to enforce the invariants in §2.

Implementation MAY establish concrete Core types for:

- authoritative rule-evaluation support;
- verdict support;
- evidence references;
- provenance references;
- NOT_EVALUATED context;
- NOT_APPLICABLE context; and
- ERROR context.

Concrete names and member shapes SHALL be reviewed against this decision immediately before implementation.

This authorization does not extend to a complete rule engine, concrete security rules, findings emission, rendering, persistence, provider integration, Graph integration, authentication, or serialization.

## 4. Explicitly deferred

Decision 0015 does NOT resolve:

- complete `RuleEvaluation` member shape beyond Stage-4-required mechanics;
- complete `Finding` record shape;
- finding-emission policy;
- finding fingerprint/deduplication;
- externally stable evaluation/finding/evidence/provenance identifier formats;
- canonical serialization or schema-version headers;
- persistence;
- detailed provenance-record or provenance-manifest schema;
- provider-specific provenance mappings;
- redaction policy/schema;
- sensitive-data classification beyond established secret exclusion;
- secret-detection implementation mechanics;
- complete diagnostic schema;
- complete operational-failure-to-state mapping;
- fatal-versus-isolated evidence-integrity policy;
- severity taxonomy;
- assessment-time representation;
- cryptographic hashing/signing/timestamping;
- output formats or renderer contracts;
- concrete security rules;
- Microsoft Graph endpoints, scopes, permissions, SDK members, or undocumented provider behavior.

Those remain with their existing owning design documents and TBDs.

## 5. Required implementation tests

The subsequent implementation unit SHALL test at minimum:

### State/support integrity

- Non-empty verdict support satisfies the PASS support requirement, not the complete PASS `RuleEvaluation` construction contract.
- Non-empty verdict support satisfies the FAIL support requirement, not the complete FAIL `RuleEvaluation` construction contract.
- PASS without evidence support is rejected.
- FAIL without evidence support is rejected.
- NOT_EVALUATED without its structured context is rejected.
- NOT_APPLICABLE without its structured context is rejected.
- ERROR without its structured context is rejected.
- State/support mismatches are rejected.
- Invalid enum values are rejected.

### Evidence integrity

- Empty verdict evidence is rejected.
- Evidence reference retains explicit tenant-assessment context.
- Evidence/provenance cannot be substituted with presentation text.
- Missing required provenance cannot be silently synthesized.

### Tenant isolation

- Same-tenant evaluation/evidence linkage is accepted.
- Cross-tenant evidence linkage is rejected.

### Stage-3 compatibility

- Unknown completeness does not become PASS through these mechanics.
- Non-Observed capability does not become PASS through these mechanics.
- Partial observations are not treated as universally sufficient.
- `PassPreconditionDisposition.Satisfied` does not itself construct PASS.

### Secret/provider boundary

Tests SHALL demonstrate that the authorized Core contracts do not require raw provider payloads, provider SDK types, or secret-bearing authentication material.

## 6. Security consequences

This decision deliberately makes invalid state/support combinations structurally rejectable.

It reduces the risk of:

- evidence-free PASS;
- evidence-free FAIL;
- missing-data FAIL;
- capability-gap PASS;
- fabricated evidence/provenance;
- cross-tenant evidence contamination;
- renderer-driven verdict mutation;
- AI-driven verdict mutation; and
- provider payload or credential leakage into Core evaluation records.

It does not claim that structural typing alone detects every secret or proves evidence authenticity.

## 7. Relationship to earlier decisions

Decision 0015 extends but does not redefine:

- Decision 0012 — PASS eligibility proof contract;
- Decision 0013 — PASS precondition guard mechanics; and
- Decision 0014 — evidence and provenance semantic contract.

Where this decision does not explicitly resolve an existing TBD, that TBD remains unresolved.

## 8. Exit condition

Decision 0015 is ready for implementation only when review confirms that:

1. every canonical state has an explicit support obligation;
2. PASS/FAIL cannot be represented as valid evaluations without evidence support;
3. non-verdict states cannot be represented as valid evaluations without structured reason/context;
4. evidence/provenance remain normalized, provider-neutral, non-secret references;
5. tenant-context mismatch is structurally rejectable;
6. Stage-3 capability/completeness semantics remain unchanged;
7. no deferred finding, serialization, persistence, provider, redaction, fingerprint, severity, or cryptographic design has been silently resolved; and
8. the authorized implementation surface is the minimum necessary for the Stage-4 evidence/provenance gate.


