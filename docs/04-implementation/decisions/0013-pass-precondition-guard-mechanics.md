# Decision 0013 — PASS Precondition Guard Mechanics

**Status:** Accepted for Phase 0.4 implementation

## Context

Decision 0012 authorizes a narrow provider-independent Core guard that
classifies generic capability/completeness preconditions for later PASS
consideration.

Decision 0012 deliberately does not assign assessment-state authority to this
guard. It also preserves the documented case where partial observed data may
support a later verdict only when a consuming rule explicitly proves that the
available subset is sufficient.

A minimal concrete Core representation is required before implementing the
guard and its Stage-3 contract tests.

## Decision

### 1. Disposition representation

Core SHALL define one closed enum named:

`PassPreconditionDisposition`

It SHALL contain exactly these members in this order:

1. `Satisfied`
2. `RequiresRuleSpecificSufficiency`
3. `Insufficient`

These members are PASS-precondition classifications only.

None is an assessment state or tenant security verdict.

In particular:

- `Satisfied` does not mean PASS.
- `RequiresRuleSpecificSufficiency` does not mean PASS, FAIL,
  NOT_EVALUATED, NOT_APPLICABLE, or ERROR.
- `Insufficient` does not select NOT_EVALUATED, NOT_APPLICABLE, or ERROR.

No additional `Pass`, `Fail`, `Error`, `Unknown`, `NotApplicable`,
`NotEvaluated`, `Success`, or provider-specific member SHALL be introduced.

### 2. Guard representation

Core SHALL define one static class named:

`PassPreconditionGuard`

It SHALL expose one public pure classification method with the semantic shape:

`Evaluate(CapabilityState, ObservationCompleteness) -> PassPreconditionDisposition`

The guard SHALL depend only on canonical Core capability and completeness
vocabulary.

It SHALL perform no I/O, network access, provider access, authentication,
logging, persistence, serialization, evidence construction, diagnostic
construction, or rule evaluation.

### 3. Canonical mapping

The guard SHALL implement exactly this generic Stage-3 mapping:

| Capability state | Completeness | Disposition |
| --- | --- | --- |
| `Observed` | `Complete` | `Satisfied` |
| `Observed` | `Partial` | `RequiresRuleSpecificSufficiency` |
| `Observed` | `Unknown` | `Insufficient` |
| any non-`Observed` canonical capability state | any canonical completeness state | `Insufficient` |

No other combination SHALL produce `Satisfied` or
`RequiresRuleSpecificSufficiency`.

### 4. Complete observed data is not PASS

`Satisfied` means only that the generic Stage-3 capability/completeness
precondition has been established.

It SHALL NOT:

- construct or return `AssessmentState.PASS`;
- imply rule applicability;
- imply predicate success;
- imply rule-specific sufficiency;
- imply confirmed-empty;
- imply evidence sufficiency; or
- manufacture a finding or verdict.

### 5. Partial observed data remains conditional

`RequiresRuleSpecificSufficiency` preserves the documented narrow case where
partial observed data may support later PASS or FAIL only after the consuming
rule explicitly proves the available subset sufficient under that rule's
semantics.

The Stage-3 guard SHALL NOT implement that proof.

### 6. Insufficient does not choose a final state

`Insufficient` means only that the supplied generic capability/completeness
condition does not permit PASS consideration by itself.

It SHALL NOT decide between:

- NOT_EVALUATED;
- NOT_APPLICABLE; or
- ERROR.

Those mappings remain with their documented later owners.

### 7. Confirmed-empty remains outside the guard

The guard SHALL NOT accept or produce a confirmed-empty marker.

`Observed` plus `Complete` SHALL NOT imply confirmed-empty.

Concrete confirmed-empty representation remains deferred to its owning
observation/collector contract and consuming-rule semantics.

### 8. Operational failures remain outside the guard

`OperationalFailureCategory` SHALL NOT be an input to
`PassPreconditionGuard.Evaluate`.

The guard SHALL NOT perform operational-failure-to-assessment-state mapping.

No `Result<T>`, operational error/result envelope, diagnostic, or failure
payload is introduced by this decision.

### 9. Required implementation tests

Tests SHALL prove at least:

1. the disposition enum contains exactly the three approved members;
2. `Observed` + `Complete` returns `Satisfied`;
3. `Observed` + `Partial` returns
   `RequiresRuleSpecificSufficiency`;
4. `Observed` + `Unknown` returns `Insufficient`;
5. every non-`Observed` capability state combined with every canonical
   completeness state returns `Insufficient`;
6. only `Observed` + `Complete` returns `Satisfied`;
7. only `Observed` + `Partial` returns
   `RequiresRuleSpecificSufficiency`;
8. the disposition type is distinct from `AssessmentState`;
9. the guard does not accept `OperationalFailureCategory`;
10. no confirmed-empty state or assessment-state alias is introduced; and
11. the implementation remains provider/network/secret independent.

### 10. Invalid enum values

This decision does not define behavior for values outside the declared members
of `CapabilityState` or `ObservationCompleteness`.

No exception, fallback, or assessment-state behavior for invalid cast enum
values is selected here.

If such handling becomes necessary, it requires separate review so programmer
or invariant violations are not silently conflated with ordinary capability or
completeness conditions.

## Explicitly deferred

This decision does not define:

- assessment-state mapping;
- final rule evaluation;
- rule-specific partial-data sufficiency proof;
- confirmed-empty representation;
- observation or collector envelopes;
- operational result/error shapes;
- diagnostic or redaction schemas;
- provider mappings;
- Microsoft Graph behavior;
- authentication or authorization mechanics;
- cancellation propagation;
- retry, timeout, pagination, or resource limits;
- evidence/provenance mechanics;
- serialization or persistence;
- output or CLI behavior.

## Consequences

Stage 3 receives a deterministic, provider-independent and deliberately
non-authoritative PASS-precondition classifier.

The classifier makes complete, partial, unknown, and unavailable observation
conditions executable without converting them into tenant security verdicts or
preempting later rule-engine and error/result mechanics.
