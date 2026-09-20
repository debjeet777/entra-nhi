# Decision 0009 — Capability and Completeness Core Representation

- **Status:** Accepted for Phase 0.4 implementation
- **Phase:** 0.4 — Secure Core Implementation
- **Stage:** 3 — Capability / completeness / error semantics

## Context

Decision 0008 fixes the Stage-3 capability and completeness semantic
boundary but deliberately leaves concrete representation mechanics
unresolved.

Stage 3 now requires a minimal provider-independent Core representation
that can preserve those semantics and support deterministic synthetic
tests without prematurely selecting collector envelopes, provider
mappings, operational Result/Error shapes, rule prerequisite syntax,
evidence schemas, or output formats.

This decision selects only that minimal Core representation boundary.

## Decision

### 1. Capability condition uses a closed Core enum

Core SHALL define a closed `CapabilityState` enum containing exactly:

- `Observed`
- `NotRequested`
- `UnavailableAuthorization`
- `UnavailableLicensingService`
- `Unsupported`
- `Failed`
- `Unknown`

These values represent capability/data-observation conditions only.

They MUST NOT be added to `AssessmentState`, treated as aliases for an
assessment outcome, or used as a substitute for the five-state
assessment contract.

No provider-specific capability condition is permitted in this enum.

### 2. Observation completeness uses a separate closed Core enum

Core SHALL define a separate `ObservationCompleteness` enum containing
exactly:

- `Complete`
- `Partial`
- `Unknown`

This representation describes knowledge about the completeness of the
applicable observation scope.

It does not describe whether content was present, empty, unavailable,
unsupported, or failed, and it does not represent an assessment
outcome.

`Complete` MUST be affirmative knowledge established by the producing
boundary. Core MUST NOT infer it merely from collection success, an
empty collection, a non-null value, or absence of an error.

`Partial` means some applicable observation scope is known to be
unobserved or unsuccessful while usable observations may still exist.

`Unknown` is the conservative state when completeness cannot be
affirmatively established.

### 3. Content presence remains separate from completeness

Stage 2 `DomainValue<T>` remains the canonical present-versus-not-
presently-known value primitive where that contract applies.

Confirmed-empty is not represented by a new universal value sentinel
and MUST NOT be inferred from `DomainValue<T>.NotPresentlyKnown()`.

For collection-shaped observations, confirmed-empty requires all of the
following semantic facts:

- the capability condition is `Observed`;
- the applicable observation scope is `Complete`;
- the producing boundary has affirmatively established that the
  observed collection contains no applicable items; and
- the consuming rule's explicit completeness semantics permit that
  empty observation to be conclusive.

This decision does not introduce a universal `ConfirmedEmpty` enum
member because confirmed-empty is a conclusion over observation
content plus capability/completeness context, not a sixth capability
state or a fourth completeness state.

### 4. Capability and completeness are independently represented

`CapabilityState` and `ObservationCompleteness` MUST remain distinct
typed values.

No single combined enum SHALL encode the Cartesian product of
capability state and completeness.

No boolean such as `IsAvailable`, `Succeeded`, `IsComplete`, or
`HasData` may replace either canonical enum where authoritative
Stage-3 semantics are required.

A capability state does not by itself establish completeness, and a
completeness state does not by itself establish capability state.

### 5. Tenant-derived capability observations require explicit tenant context

Any Core contract representing capability/completeness facts derived
from tenant data MUST carry or structurally compose the existing
`TenantContext`.

Ambient tenant state, static/global tenant state, or provider-native
tenant objects are prohibited.

The exact later collector observation envelope remains deferred.

### 6. Operational failure remains separate

`CapabilityState.Failed` records that the applicable capability
observation did not complete successfully.

It MUST NOT become the operational failure taxonomy itself.

Operational failure category, cause, retryability, cancellation,
diagnostic chain, and failure-to-assessment mapping remain owned by
`error-result-model.md` and its coordinated later decisions.

Therefore this decision does not create a generic `Result<T>`,
`OperationalError`, failure-code catalog, or
failure-category-to-assessment-state mapping.

### 7. No universal capability-to-assessment conversion

Neither `CapabilityState` nor `ObservationCompleteness` SHALL expose a
universal conversion to `AssessmentState`.

In particular, Core MUST NOT provide logic equivalent to:

- `Observed => PASS`;
- `Complete => PASS`;
- `Failed => ERROR`;
- `Unsupported => NOT_APPLICABLE`;
- `NotRequested => NOT_EVALUATED`; or
- `Unknown => NOT_EVALUATED`

as unconditional mappings.

Assessment outcomes remain rule- and context-dependent under the locked
rule-engine and error/result semantics.

### 8. Determinism and security boundaries

The selected Core representations MUST:

- be provider-independent;
- be network-independent;
- contain no token, credential, secret, provider exception, endpoint,
  permission name, commercial-plan name, or SDK/runtime object;
- require no ambient runtime state;
- be deterministic for equivalent normalized inputs;
- remain usable in synthetic provider-free and network-free tests; and
- preserve false-PASS resistance.

## Concrete Stage-3 implementation authorized by this decision

After this decision is independently reviewed and accepted, Stage 3 may
implement:

1. the exact `CapabilityState` enum defined above;
2. the exact `ObservationCompleteness` enum defined above;
3. narrowly scoped Core tests proving their exact closed vocabulary;
4. Core tests proving separation from `AssessmentState`;
5. Core tests proving ordinary absence does not manufacture
   confirmed-empty semantics;
6. Core tests proving partial/unknown completeness remain distinct from
   complete observation; and
7. structural tests proving no provider/runtime dependency enters Core.

Additional capability-observation record/member shapes require a
separate bounded decision if implementation cannot proceed without
them.

## Explicitly deferred

This decision does NOT select:

- a capability identifier schema;
- a `CapabilityObservation` record/member list;
- collector request/result/envelope shapes;
- source-observation envelope schema;
- provider-signal mappings;
- Graph endpoints/properties;
- permissions/scopes;
- commercial licensing detection mechanics;
- operational `Result`/`Error` record shapes;
- operational failure code identifiers;
- a complete error-code catalog;
- per-category failure-to-`NOT_EVALUATED`/`ERROR` mappings;
- cancellation mechanism;
- retry/backoff/timeout/concurrency constants;
- capability aggregation algorithms;
- rule prerequisite declaration syntax;
- evidence/provenance/diagnostic record schemas;
- serialization/wire tokens;
- persistence representation;
- output/renderer/CLI mappings;
- freshness/cache/TTL mechanisms; or
- numeric processing/resource bounds.

Those remain with their existing owner documents and later bounded
implementation decisions.

## Required verification

Before this decision may be accepted for implementation, independent
review MUST confirm that:

1. the seven `CapabilityState` values exactly preserve Decision 0008;
2. no capability value becomes a sixth assessment state;
3. completeness is represented independently from capability state;
4. `Complete` requires affirmative knowledge;
5. `Partial` cannot masquerade as `Complete`;
6. `Unknown` remains conservative;
7. confirmed-empty is not fabricated from ordinary absence;
8. `Failed` does not replace the operational failure taxonomy;
9. no universal capability/completeness-to-assessment mapping is
   introduced;
10. explicit tenant isolation remains required for tenant-derived
    capability facts;
11. provider-specific semantics do not enter Core; and
12. no explicitly deferred Stage-3 or later-stage mechanic is resolved.

## Non-goals

This decision does not implement collectors, provider adapters,
operational error handling, rule evaluation, evidence/provenance,
outputs, CLI behavior, Graph access, authentication, or remediation.

## Acceptance condition

Decision 0009 may move from `Proposed` to
`Accepted for Phase 0.4 implementation` only after independent review
against Decision 0008 and the locked implementation-design package
confirms that the selected mechanics are the minimum sufficient Core
representation and do not preempt an explicitly deferred owner.
