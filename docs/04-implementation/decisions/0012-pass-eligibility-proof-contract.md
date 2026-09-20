# Decision 0012 — PASS Precondition Proof Contract

**Status:** Accepted for Phase 0.4 implementation

## Context

Phase 0.4 Stage 3 requires executable proof that capability limitations,
incomplete observation, unknown completeness, and operational degradation cannot
silently authorize PASS.

The locked implementation design also requires that:

- PASS is a security-relevant assertion requiring affirmative rule conditions
  plus rule-defined sufficient completeness;
- capability and observation conditions remain distinct from assessment state;
- confirmed-empty is not a universal capability or completeness state;
- operational failures remain distinct from tenant security verdicts;
- there is no universal capability-state-to-assessment-state conversion;
- collectors do not determine PASS or FAIL;
- rule-specific completeness semantics remain owned by the rule engine; and
- partial observation may support a verdict only where the consuming rule
  explicitly proves that the available subset is sufficient.

Stage 3 therefore needs a narrow mechanism that constrains PASS consideration
without assigning an assessment state or taking authority from later
rule-specific evaluation.

## Decision

### 1. Core owns a narrow PASS-precondition guard

Stage 3 MAY introduce a provider-independent Core guard that classifies whether
the generic capability/completeness preconditions for later PASS consideration
are:

1. satisfied at the generic level;
2. dependent on later rule-specific sufficiency proof; or
3. insufficient to permit PASS.

The guard is not a rule evaluator and is not an assessment-state mapper.

### 2. The guard has no verdict authority

No disposition returned by the guard means PASS.

The guard MUST NOT create or return an `AssessmentState`, finding, evidence,
diagnostic, operational result, or tenant security verdict.

Its purpose is only to prevent degraded or unproven observation conditions from
being silently treated as sufficient for PASS.

### 3. Complete observed data satisfies only the generic precondition

`CapabilityState.Observed` together with
`ObservationCompleteness.Complete` satisfies the generic Stage-3
capability/completeness precondition.

This means only that Stage 3 does not itself prohibit later PASS consideration.

It does not establish:

- rule applicability;
- satisfaction of a rule predicate;
- rule-specific completeness;
- confirmed-empty semantics;
- required evidence; or
- `AssessmentState.PASS`.

### 4. Partial observed data requires rule-specific sufficiency proof

`CapabilityState.Observed` together with
`ObservationCompleteness.Partial` MUST NOT independently authorize PASS.

It MUST remain distinguishable from both complete observation and unknown
completeness.

A later consuming rule MAY produce PASS or FAIL from partial observation only
when that rule explicitly proves that the available subset is sufficient under
its own documented semantics.

Stage 3 does not define that proof mechanism.

Therefore the Stage-3 disposition for `Observed` + `Partial` must preserve the
meaning:

> rule-specific sufficiency proof is required before PASS can be considered.

It MUST NOT mean PASS, FAIL, NOT_EVALUATED, or ERROR.

### 5. Unknown completeness prohibits PASS from current information

`CapabilityState.Observed` together with
`ObservationCompleteness.Unknown` MUST NOT authorize PASS.

Unknown completeness remains conservative until later semantics establish an
explicit handling path.

The Stage-3 guard does not decide whether the eventual assessment state is
NOT_EVALUATED or ERROR.

### 6. Non-observed capability conditions do not authorize PASS

The following capability states MUST NOT independently permit PASS:

- `NotRequested`
- `UnavailableAuthorization`
- `UnavailableLicensingService`
- `Unsupported`
- `Failed`
- `Unknown`

The guard MUST NOT convert any of them directly into an assessment state.

Where later contracts distinguish NOT_EVALUATED, NOT_APPLICABLE, or ERROR,
that decision remains with their documented owners.

### 7. Confirmed-empty remains separate

This decision does not introduce `ConfirmedEmpty` as:

- a `CapabilityState`;
- an `ObservationCompleteness` member;
- an assessment state;
- a universal sentinel; or
- a universal absence-to-PASS rule.

`Observed` + `Complete` alone MUST NOT be interpreted as confirmed-empty.

Confirmed-empty additionally requires an explicit reliable empty observation
for the covered scope, and the consuming rule must determine whether that
verified absence is sufficient for its own semantics.

The concrete representation of that explicit empty observation remains
deferred to its owning observation/collector contract.

### 8. Operational failure remains separate

`OperationalFailureCategory` MUST NOT be universally converted into a
PASS-precondition disposition or directly into an assessment state.

Operational-failure-to-NOT_EVALUATED/ERROR handling remains deferred to its
approved mechanics.

No operational failure may be converted into PASS by this guard.

### 9. No final assessment-state authority

The Stage-3 guard MUST NOT return or construct:

- `AssessmentState`;
- `Result<T>`;
- an operational error/result envelope;
- a diagnostic record;
- a finding;
- evidence;
- a collector result; or
- provider-specific state.

It MUST NOT decide between NOT_EVALUATED and ERROR.

### 10. Required Stage-3 tests

Tests for the guard MUST prove at least:

1. `Observed` + `Complete` satisfies only the generic precondition and does
   not constitute PASS.
2. `Observed` + `Partial` remains distinct and requires later rule-specific
   sufficiency proof.
3. `Observed` + `Unknown` cannot authorize PASS.
4. every non-`Observed` capability state cannot authorize PASS.
5. `Partial` never independently authorizes PASS.
6. `Unknown` completeness never authorizes PASS.
7. the guard does not return or introduce an `AssessmentState`.
8. confirmed-empty is not inferred from `Observed` + `Complete`.
9. operational failure categories are not converted into verdicts by the
   guard.
10. the guard remains provider/network/secret independent.

These are Stage-3 contract tests. They do not replace later full-pipeline
false-PASS adversarial tests required by the testing architecture.

## Security consequences

This decision establishes a conservative executable boundary against false PASS
without incorrectly prohibiting the documented rule-specific partial-data
sufficiency case.

It prevents:

- authorization limitation from appearing as sufficient data;
- unsupported or unavailable capability from appearing as secure absence;
- partial collection from independently appearing sufficient;
- unknown completeness from appearing complete; and
- generic capability/completeness logic from manufacturing a tenant verdict.

## Explicitly deferred

This decision does not select or define:

- final rule-evaluation contracts;
- rule-specific partial-data sufficiency proof mechanics;
- `Result<T>` or operational error shapes;
- diagnostic schemas or redaction implementation;
- operational-failure-to-assessment-state mapping;
- fatal-versus-isolated failure policy;
- collector envelopes;
- observation/content envelopes;
- confirmed-empty representation mechanics;
- provider or Microsoft Graph mappings;
- authentication or authorization mechanics;
- cancellation propagation mechanics;
- retry, timeout, pagination, or resource-limit constants;
- evidence/provenance schemas;
- serialization or persistence;
- output or CLI behavior.

Those remain with their previously assigned owners and stages.

## Consequences

Stage 3 gains a minimal executable false-PASS precondition boundary while
preserving later rule-engine authority.

Complete observed data satisfies only the generic precondition. Partial
observed data requires explicit rule-specific sufficiency proof. Unknown or
non-observed conditions cannot independently authorize PASS.

No disposition produced by this contract is itself an assessment verdict.
