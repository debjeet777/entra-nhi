# Decision 0008 — Capability and Completeness Semantic Contract

- **Status:** Accepted for Phase 0.4 implementation
- **Phase:** 0.4 — Secure Core Implementation
- **Stage:** 3 — Capability / completeness / error semantics

## Context

Stage 3 requires Core to preserve whether required assessment data was
actually observable and sufficiently complete without confusing data
availability with the authoritative five-state assessment outcome.

The locked implementation design requires explicit capability and
data-availability semantics so missing, inaccessible, unsupported,
failed, partial, or indeterminate observations cannot silently produce
an authoritative security verdict.

This decision narrows that semantic boundary only. It does not select
the concrete C# representation.

## Decision

### 1. Capability and assessment outcome remain separate concepts

Capability/data availability describes what the assessment was able to
observe.

`AssessmentState` describes the authoritative result of evaluating a
rule.

Capability conditions MUST NOT become additional assessment states and
MUST NOT be represented by extending `AssessmentState`.

No capability condition directly equals `PASS`, `FAIL`,
`NOT_EVALUATED`, `NOT_APPLICABLE`, or `ERROR`.

### 2. Canonical capability observation semantics

Stage 3 MUST preserve the following conceptual capability conditions:

- observed;
- not requested / not collected;
- unavailable because of a known authorization limitation;
- unavailable because of a known licensing/service limitation;
- unsupported;
- failed;
- unknown / indeterminate.

These are provider-independent semantic conditions.

Provider exception types, SDK types, endpoint paths, permission names,
commercial-plan names, and provider-native status values MUST NOT
become Core capability semantics.

### 3. Data-presence semantics remain distinguishable

Capability state alone does not prove the content of an observation.

Normalized observation semantics MUST preserve the distinction between:

- observed value/content;
- confirmed empty;
- unavailable;
- unsupported;
- not requested / not collected;
- failed/error condition;
- unknown / indeterminate;
- genuinely not applicable.

`Confirmed empty` means that the source reliably established absence
under a sufficiently complete observation.

An empty collection, missing value, null, absent object, or lack of
returned data MUST NOT by itself mean `Confirmed empty`.

### 4. Completeness is affirmative, not assumed

Authoritative evaluation requires affirmative knowledge that the
required observation scope is sufficiently complete for the rule being
evaluated.

Missing information MUST NOT be interpreted as evidence that no
security issue exists.

A `PASS` MUST NOT be derived merely because:

- a collection is empty;
- a value is absent;
- collection was not requested;
- authorization prevented observation;
- required functionality is unsupported;
- collection failed;
- the observation is unknown or indeterminate; or
- only an unproven partial subset was available.

Whether confirmed-empty data can support `PASS` is determined by the
specific rule's explicit completeness semantics, not by a universal
empty-means-secure rule.

### 5. Partial collection remains explicit

Partial observations MUST remain distinguishable from complete
observations.

The default posture for unmet required observation scope is
conservative: partial evidence MUST NOT be treated as complete
evidence.

A later rule MAY rely on a partial subset only where that rule's
documented deterministic semantics prove that the available subset is
sufficient for the authoritative outcome.

The rule bears that proof burden. Core MUST NOT infer sufficiency.

Collectors and normalization MUST NOT hide partial collection behind a
successful empty result.

### 6. Scope of capability gaps remains explicit

Capability/completeness information MUST remain attributable to the
scope to which it applies.

A gap affecting one category or subject MUST NOT automatically erase
valid observations for unrelated categories or subjects.

Likewise, successful observations elsewhere MUST NOT hide the affected
gap.

Exact aggregation algorithms and summary schemas remain deferred.

### 7. Capability conditions constrain but do not universally map to assessment outcomes

Capability/completeness conditions constrain which authoritative
assessment outcomes are safe, but there is deliberately no universal
capability-state-to-assessment-state conversion.

In particular:

- unavailable required data MUST NOT silently yield `PASS`;
- unsupported required capability MUST NOT silently yield `PASS`;
- failed required collection MUST NOT silently yield `PASS`;
- unknown required data MUST NOT silently yield `PASS`;
- not-requested required data MUST NOT yield authoritative
  `PASS`/`FAIL`;
- unproven partial required data MUST NOT silently yield `PASS`;
- `NOT_APPLICABLE` MUST represent genuine rule inapplicability and
  MUST NOT hide a capability or collection gap.

The detailed rule-dependent transition mechanics remain owned by the
rule-engine and error/result contracts.

### 8. Operational failures remain a separate layer

Capability/data availability MUST NOT be collapsed with operational
execution failure.

Operational failure classification, cancellation, retryability,
failure causality, and failure-to-`NOT_EVALUATED`/`ERROR` mapping remain
separate concerns owned by the shared error/result contract.

Likewise, programmer/invariant violations remain distinct from both
capability gaps and expected operational failures.

### 9. Determinism and false-PASS resistance are mandatory

Equivalent normalized capability/completeness inputs under equivalent
assessment context MUST produce equivalent semantic interpretation.

No AI/model inference, renderer behavior, provider exception wording,
or ambient runtime state may determine capability semantics.

Ignorance, incomplete collection, or failure MUST remain visible and
MUST NOT degrade into a secure verdict.

### 10. Security and isolation boundaries

Capability/completeness Core contracts MUST:

- remain provider-independent;
- remain network-independent;
- carry no authentication token or credential;
- carry no secret-bearing payload;
- avoid provider SDK/runtime types;
- preserve explicit tenant isolation where tenant-derived data is
  represented;
- remain suitable for synthetic, provider-free, network-free Core
  testing.

## Deferred mechanics

This decision intentionally does NOT choose:

- concrete C# type names;
- enum versus record/class representation;
- exact code-constant spelling or casing;
- constructors or factories;
- concrete member lists;
- capability identifier schema;
- capability-observation envelope schema;
- serialization or wire tokens;
- persistence representation;
- provider signal mappings;
- Graph endpoints/properties;
- permission/scope names;
- commercial licensing mappings;
- capability aggregation algorithms;
- assessment-summary schemas;
- freshness/cache mechanisms or TTLs;
- concrete operational `Result`/`Error` shapes;
- operational failure code identifiers;
- per-category failure-to-assessment mappings;
- cancellation mechanism;
- retry/backoff/timeout constants;
- rule prerequisite declaration syntax;
- findings/evidence/diagnostic schemas;
- output/renderer mappings.

Those remain with their already assigned owner documents and later
bounded implementation decisions.

## Required verification before mechanics are selected

Before concrete Stage-3 implementation mechanics are accepted, tests
must be designed to prove at minimum:

1. capability semantics cannot create a sixth assessment state;
2. observed and non-observed capability conditions remain
   distinguishable;
3. confirmed-empty cannot be fabricated from ordinary absence;
4. unavailable authorization remains distinguishable from
   unavailable licensing/service;
5. unsupported, failed, unknown, and not-requested remain distinct;
6. partial collection cannot masquerade as complete collection;
7. capability gaps cannot silently produce `PASS`;
8. genuine non-applicability remains distinguishable from a
   collection/capability gap;
9. provider types and provider-specific mechanics do not enter Core;
10. synthetic tests require no network or secrets.

## Non-goals

This decision does not implement:

- a capability enum or class;
- a completeness object;
- operational error/result types;
- collectors;
- provider adapters;
- Graph access;
- rule evaluation;
- evidence/provenance;
- output rendering;
- CLI behavior.

## Acceptance condition

Decision 0008 may move from `Proposed` to
`Accepted for Phase 0.4 implementation` only after an independent
review confirms that it:

- preserves the locked capability/completeness architecture;
- introduces no provider-specific semantics;
- introduces no sixth assessment state;
- preserves false-PASS resistance;
- keeps capability, operational failure, assessment outcome, and
  invariant violation separate; and
- does not resolve any explicitly deferred implementation mechanic.