# 0007 — Canonical evaluation outcome contract

> **Status:** Accepted for Phase 0.4 implementation
> **Scope:** EntraNHI.Core
> **Phase:** 0.4 — Stage 2

## Context

Stage 2 requires the stable inward evaluation vocabulary that later rule,
capability, evidence, application, and output stages will consume.

`AssessmentState` already defines exactly five authoritative V1 assessment
states:

- `PASS`
- `FAIL`
- `NOT_EVALUATED`
- `NOT_APPLICABLE`
- `ERROR`

The locked implementation design requires every evaluation to resolve to
exactly one canonical assessment state.

It also requires structured reason/context on every non-`PASS`/`FAIL` path
while preserving strict separation between:

1. assessment/rule outcome;
2. capability and data-availability condition;
3. operational result or failure; and
4. programmer or invariant violation.

Stage 2 must establish the canonical evaluation-outcome boundary without
prematurely defining the Stage-3 capability/completeness taxonomy, the
operational failure taxonomy, provider mappings, evidence schema, or
serialization mechanics.

## Decision

The canonical Core evaluation outcome MUST carry exactly one
`AssessmentState`.

`PASS` and `FAIL` represent authoritative evaluated security-posture
outcomes.

`NOT_EVALUATED`, `NOT_APPLICABLE`, and `ERROR` MUST have an explicit
structured reason/context path. These states MUST NOT be represented only by
absence, null, an exception message, free-form renderer text, or an
alternative assessment state.

The evaluation-outcome contract MUST remain provider-independent and
output-neutral.

No sixth assessment state may be introduced.

## Semantic boundaries

The evaluation outcome represents only the authoritative assessment/rule
outcome.

It MUST NOT collapse into the same type or enum:

- capability or data-availability state;
- collection completeness;
- operational success or failure;
- cancellation;
- retryability;
- diagnostic severity;
- programmer or invariant violation;
- provider-specific failure state;
- renderer or CLI state.

Capability and completeness conditions may constrain which assessment outcome
is valid, but they are not themselves assessment states.

Operational failures may later be mapped into assessment behavior through the
explicit owning contracts, but an operational failure is not itself an
assessment state.

Programmer and invariant violations remain integrity failures and MUST NOT be
silently converted into ordinary assessment verdicts.

## Reason/context boundary

Stage 2 establishes only that structured reason/context MUST be representable
for:

- `NOT_EVALUATED`;
- `NOT_APPLICABLE`; and
- `ERROR`.

This decision does not define the final reason-code catalog, failure-category
catalog, diagnostic schema, capability reason vocabulary, or provider
mapping.

Reason/context MUST NOT contain secrets, credentials, tokens, private-key
material, passwords, recovery codes, raw authorization headers, or other
secret-bearing values.

Provider-originated implementation types MUST NOT enter the Core evaluation
contract.

## Tenant boundary

Any concrete evaluation contract carrying tenant-derived assessment data MUST
carry explicit `TenantContext`.

Ambient or implied tenancy is prohibited.

Tenant-context mismatch or contamination remains an integrity failure and
must not be represented as an ordinary tenant security `FAIL`.

## PASS and FAIL boundary

`PASS` and `FAIL` assert an evaluated security-posture conclusion.

Their evidence/provenance obligations remain governed by the locked
evidence/provenance design and Stage 4 implementation work.

This Stage-2 decision does not prematurely define evidence-reference member
shapes.

## Deferred mechanics

The following remain deliberately unresolved:

- concrete C# evaluation-result type name;
- record versus class mechanics;
- constructor and factory mechanics;
- concrete member list beyond the required semantic boundary;
- structured reason/context concrete type;
- reason-code spelling or catalog;
- capability-state vocabulary;
- completeness-state vocabulary;
- operational failure categories and concrete `Result`/`Error` shapes;
- failure-category-to-`NOT_EVALUATED`/`ERROR` mapping;
- cancellation mechanics;
- evidence-reference concrete shape;
- finding schema;
- diagnostic schema;
- severity representation;
- serialization and wire format;
- renderer projection;
- CLI exit-code mapping;
- provider-specific mappings.

These mechanics must be resolved only by their owning implementation stages
and coordinated contracts.

## Required verification

Before a concrete evaluation-outcome implementation is accepted, tests and
review MUST prove at minimum that:

1. exactly the five canonical `AssessmentState` values remain authoritative;
2. every evaluation carries exactly one canonical assessment state;
3. `NOT_EVALUATED` has an explicit structured reason/context path;
4. `NOT_APPLICABLE` has an explicit structured reason/context path;
5. `ERROR` has an explicit structured reason/context path;
6. no sixth assessment state is introduced;
7. capability/data availability is not collapsed into `AssessmentState`;
8. operational failure is not collapsed into `AssessmentState`;
9. tenant-derived evaluation data carries explicit `TenantContext`;
10. no provider, network, authentication, filesystem, renderer, or
    secret-bearing dependency enters Core;
11. fixtures remain synthetic and secret-free.

## Security consequences

This boundary prevents missing capability, incomplete data, operational
failure, or inapplicability from silently becoming `PASS` or `FAIL`.

It also prevents provider-specific failures and presentation concerns from
expanding the authoritative assessment-state vocabulary.

The explicit reason/context requirement keeps non-verdict outcomes visible
without prematurely fixing the later failure and capability taxonomies.

## Non-goals

This decision does not implement the rule engine.

It does not define capability/completeness semantics.

It does not define operational error taxonomy.

It does not define evidence or finding schemas.

It does not define provider collection behavior.

It does not define serialization or presentation behavior.

It does not authorize implementation of deferred mechanics.