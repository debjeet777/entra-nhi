# Decision 0010 — Operational Failure Semantic Contract

- **Status:** Accepted for Phase 0.4 implementation
- **Phase:** 0.4 — Secure Core Implementation
- **Stage:** 3 — Capability, Completeness, and Error Semantics

## Context

EntraNHI requires explicit operational-failure semantics before later
provider, orchestration, rule-engine, and output implementation can safely
compose failure information.

The architecture distinguishes four conceptual layers:

1. assessment outcome;
2. capability and data availability;
3. operational result or failure; and
4. programmer or invariant violation.

These layers MUST remain semantically separate.

Operational failures MUST NOT silently become PASS. They also MUST NOT be
treated automatically as FAIL, NOT_EVALUATED, or ERROR without the
appropriate owning layer applying its documented deterministic semantics.

The Phase 0.3 design intentionally defers concrete result/error types,
member lists, code identifiers, provider-exception mappings, retry and
timeout constants, and the exact fatal-versus-isolated engine policy.
This decision does not resolve those deferred mechanics.

## Decision

### 1. Operational failure is a distinct semantic layer

An operational failure represents unsuccessful execution of an operation
needed by the assessment pipeline.

It is not itself:

- an `AssessmentState`;
- a `CapabilityState`;
- an `ObservationCompleteness` value;
- affirmative security evidence;
- a security finding; or
- a programmer/invariant violation.

No implicit or universal conversion from operational failure to an
assessment outcome is authorized by this decision.

### 2. Failure must be explicit

An operation that fails MUST expose that failure through the contract owned
by the relevant implementation layer.

Failure MUST NOT be represented solely by:

- `null`;
- an empty collection;
- a default value;
- missing data;
- `false`;
- an incomplete observation presented as complete; or
- silent exception suppression.

A consumer MUST be able to distinguish successful execution from
unsuccessful execution without inferring failure from ordinary domain data.

### 3. Failure must never create false PASS

Operational failure, interrupted execution, unavailable required input,
resource exhaustion, or indeterminate execution MUST NOT silently produce a
PASS assessment.

Likewise, an operational failure does not by itself constitute affirmative
evidence for FAIL.

The later rule-engine and orchestration contracts own the deterministic
mapping from available facts and failure context to the appropriate
assessment behavior.

### 4. Failure transparency is required

Where a failure is exposed beyond its originating boundary, sufficient
structured information MUST be preserved to support deterministic handling,
diagnostics, auditability, and later evidence/result assembly.

The concrete diagnostic schema, error-code vocabulary, and result type are
not selected here.

### 5. Sensitive information is excluded

Operational-failure representation MUST NOT require or expose:

- access tokens;
- refresh tokens;
- client secrets;
- private keys;
- credential material;
- authentication headers;
- raw secret-bearing configuration; or
- unnecessary raw provider payloads.

Provider exception text MUST NOT automatically become trusted Core
diagnostic content.

Later provider boundaries are responsible for controlled translation and
sanitization before provider-originated failure information crosses inward.

### 6. Provider mechanics remain outside Core

Core operational-failure semantics MUST remain provider-independent.

This decision does not introduce:

- Microsoft Graph exception types;
- HTTP status-code semantics;
- SDK exception types;
- provider request identifiers;
- Graph permission names;
- licensing API mechanics; or
- provider-specific retry behavior.

Provider-specific failures will later be translated at the appropriate
provider boundary.

### 7. Capability failure and operational failure remain distinct

`CapabilityState.Failed` communicates the state of a capability observation.

It is not the canonical operational-error object and does not replace an
operation-level failure contract.

Conversely, an operational failure does not automatically determine a
`CapabilityState`.

The owning collector/orchestration contract will define any permitted
relationship while preserving the separation established here.

### 8. Completeness remains independent

Operational success does not prove `ObservationCompleteness.Complete`.

Operational failure does not by itself determine `Partial` or `Unknown`.

Completeness remains affirmative knowledge about observation scope and must
be established independently according to the accepted completeness
contract.

### 9. Cancellation remains operational

Cancellation or interrupted execution MUST NOT introduce a sixth assessment
state.

Its concrete representation and propagation mechanism remain deferred.

Any later mapping into assessment behavior must preserve the existing
five-state assessment vocabulary and failure-transparency invariants.

### 10. Resource exhaustion must be visible

Resource exhaustion, enforced bounds, or inability to complete required work
MUST NOT be represented as successful complete execution and MUST NOT
silently yield PASS.

Concrete memory, size, timeout, retry, concurrency, and traversal limits
remain deferred to their owning implementation decisions.

### 11. Programmer and invariant violations remain separate

Invalid programmer assumptions, impossible internal states, violated
construction invariants, or contract misuse are not automatically ordinary
operational failures.

The exact exception-versus-result mechanics for programmer and invariant
violations remain deferred.

Shared-state integrity failures may require broader fail-closed behavior,
but the exact fatal-versus-isolated policy remains unresolved and is not
selected here.

### 12. Deterministic handling

Given equivalent normalized inputs, capability/completeness facts,
operational-failure facts, deterministic configuration, and execution
context, later deterministic consumers MUST not derive different semantic
outcomes because of hidden ambient state.

Operational-failure handling MUST NOT depend on AI or LLM inference.

### 13. Tenant isolation

Failure information derived from tenant-scoped operations MUST preserve the
tenant boundary of the operation or compose with the relevant tenant-scoped
contract.

Operational-failure handling MUST NOT permit cross-tenant correlation or
contamination.

The concrete tenant-bearing failure envelope, if one is required, remains
deferred.

## Explicitly deferred

This decision deliberately does NOT select:

- `Result<T>` or equivalent concrete result shape;
- `OperationalError` or equivalent concrete error shape;
- error/failure enum names or member lists;
- canonical error-code strings or numeric values;
- the full F-01 through F-14 implementation representation;
- exception hierarchy;
- provider-exception mapping;
- HTTP or Microsoft Graph mapping;
- capability-to-failure mapping;
- failure-to-assessment-state mapping;
- per-rule ERROR versus NOT_EVALUATED policy;
- fatal-versus-isolated engine failure policy;
- cancellation representation or propagation mechanism;
- retry counts;
- timeout values;
- backoff algorithms;
- concurrency limits;
- resource-limit constants;
- diagnostic serialization schema;
- output/rendering mappings;
- CLI exit-code mapping;
- logging schema;
- telemetry schema; or
- persistence format.

Each remains owned by the later contract or implementation stage identified
by the locked Phase 0.3 design.

## Security consequences

This decision strengthens the implementation boundary by requiring:

- explicit failure rather than ambiguous absence;
- no false PASS from operational failure;
- no automatic FAIL from missing execution evidence;
- no secret-bearing failure contracts;
- provider sanitization before inward propagation;
- provider independence in Core;
- continued tenant isolation;
- deterministic failure handling; and
- preservation of the exact five-state assessment vocabulary.

## Implementation authorization

While this decision remains **Proposed**, it authorizes no C# implementation.

If accepted after independent review, any implementation must still be
bounded by a separate mechanics decision if concrete result/error types,
failure codes, or mapping behavior are required.

## Review gate

Before this decision may become Accepted, review MUST confirm:

1. operational failure remains distinct from assessment outcome;
2. operational failure remains distinct from capability state;
3. operational failure remains distinct from completeness;
4. programmer/invariant violations remain separately modeled;
5. no universal failure-to-assessment-state mapping is introduced;
6. no false-PASS path is introduced;
7. no provider-specific mechanics enter Core;
8. no secret-bearing diagnostic requirement is introduced;
9. cancellation does not become a sixth assessment state;
10. fatal-versus-isolated policy remains deferred;
11. concrete Result/Error mechanics remain deferred; and
12. no existing locked capability/completeness contract is changed.
